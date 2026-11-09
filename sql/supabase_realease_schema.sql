-- ==============================================================================
-- RealEase - Supabase PostgreSQL Schema & Security Script
-- Roles: Guest, Host, Administrator (RBAC)
-- ==============================================================================

-- 0. EXTENSIONS
CREATE EXTENSION IF NOT EXISTS "uuid-ossp";
CREATE EXTENSION IF NOT EXISTS "btree_gist"; -- Required for UUID + Range GIST exclusion constraints

-- 1. ENUMERATIONS
DO $$
BEGIN
    IF NOT EXISTS (SELECT 1 FROM pg_type WHERE typname = 'role_enum') THEN
        CREATE TYPE public.role_enum AS ENUM ('guest', 'host', 'administrator');
    END IF;
    IF NOT EXISTS (SELECT 1 FROM pg_type WHERE typname = 'reservation_status_enum') THEN
        CREATE TYPE public.reservation_status_enum AS ENUM ('pending', 'confirmed', 'cancelled', 'completed');
    END IF;
END $$;

-- 2. TABLES

-- 2.1 Profiles (extends Supabase auth.users)
CREATE TABLE IF NOT EXISTS public.profiles (
    id UUID PRIMARY KEY REFERENCES auth.users(id) ON DELETE CASCADE,
    full_name TEXT,
    avatar_url TEXT,
    role public.role_enum NOT NULL DEFAULT 'guest',
    phone TEXT,
    is_active BOOLEAN NOT NULL DEFAULT TRUE,
    created_at TIMESTAMPTZ NOT NULL DEFAULT NOW(),
    updated_at TIMESTAMPTZ NOT NULL DEFAULT NOW()
);

-- Helper function to check if the current authenticated user is an administrator
CREATE OR REPLACE FUNCTION public.is_admin()
RETURNS BOOLEAN
LANGUAGE sql
SECURITY DEFINER
STABLE
AS $$
    SELECT EXISTS (
        SELECT 1
        FROM public.profiles
        WHERE id = auth.uid() AND role = 'administrator'
    );
$$;

-- 2.2 Properties
CREATE TABLE IF NOT EXISTS public.properties (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    host_id UUID NOT NULL REFERENCES public.profiles(id) ON DELETE RESTRICT,
    title TEXT NOT NULL,
    description TEXT,
    address TEXT NOT NULL,
    city TEXT NOT NULL,
    country TEXT NOT NULL,
    price_per_night NUMERIC(12, 2) NOT NULL CHECK (price_per_night >= 0),
    price_per_month NUMERIC(12, 2) CHECK (price_per_month >= 0),
    max_guests INT NOT NULL DEFAULT 1 CHECK (max_guests > 0),
    bedrooms INT NOT NULL DEFAULT 1 CHECK (bedrooms >= 0),
    bathrooms INT NOT NULL DEFAULT 1 CHECK (bathrooms >= 0),
    images TEXT[] DEFAULT ARRAY[]::TEXT[],
    is_active BOOLEAN NOT NULL DEFAULT TRUE,
    is_verified BOOLEAN NOT NULL DEFAULT FALSE, -- Admin verification flag
    created_at TIMESTAMPTZ NOT NULL DEFAULT NOW(),
    updated_at TIMESTAMPTZ NOT NULL DEFAULT NOW()
);

-- 2.3 Property Availability (Host defined calendar availability)
CREATE TABLE IF NOT EXISTS public.availability_property (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    property_id UUID NOT NULL REFERENCES public.properties(id) ON DELETE CASCADE,
    available_range DATERANGE NOT NULL,
    created_at TIMESTAMPTZ NOT NULL DEFAULT NOW(),
    -- Prevent overlapping available ranges defined for the exact same property
    EXCLUDE USING gist (
        property_id WITH =,
        available_range WITH &&
    )
);

-- 2.4 Reservations
CREATE TABLE IF NOT EXISTS public.reservations (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    property_id UUID NOT NULL REFERENCES public.properties(id) ON DELETE RESTRICT,
    guest_id UUID NOT NULL REFERENCES public.profiles(id) ON DELETE RESTRICT,
    stay_range DATERANGE NOT NULL, -- [check_in, check_out)
    status public.reservation_status_enum NOT NULL DEFAULT 'pending',
    total_amount NUMERIC(12, 2) NOT NULL CHECK (total_amount >= 0),
    notes TEXT,
    created_at TIMESTAMPTZ NOT NULL DEFAULT NOW(),
    updated_at TIMESTAMPTZ NOT NULL DEFAULT NOW(),
    -- Prevent double-booking: exclusion constraint for active (pending / confirmed) reservations
    EXCLUDE USING gist (
        property_id WITH =,
        stay_range WITH &&
    ) WHERE (status IN ('pending', 'confirmed'))
);

-- 2.5 Reviews
CREATE TABLE IF NOT EXISTS public.reviews (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    property_id UUID NOT NULL REFERENCES public.properties(id) ON DELETE CASCADE,
    guest_id UUID NOT NULL REFERENCES public.profiles(id) ON DELETE CASCADE,
    reservation_id UUID NOT NULL REFERENCES public.reservations(id) ON DELETE CASCADE,
    rating SMALLINT NOT NULL CHECK (rating BETWEEN 1 AND 5),
    comment TEXT,
    created_at TIMESTAMPTZ NOT NULL DEFAULT NOW(),
    updated_at TIMESTAMPTZ NOT NULL DEFAULT NOW(),
    CONSTRAINT unique_reservation_review UNIQUE (reservation_id)
);

-- 2.6 Payments
CREATE TABLE IF NOT EXISTS public.payments (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    reservation_id UUID NOT NULL REFERENCES public.reservations(id) ON DELETE RESTRICT,
    payer_id UUID NOT NULL REFERENCES public.profiles(id) ON DELETE RESTRICT,
    amount NUMERIC(12, 2) NOT NULL CHECK (amount >= 0),
    platform_commission NUMERIC(12, 2) NOT NULL DEFAULT 0.00 CHECK (platform_commission >= 0),
    status TEXT NOT NULL DEFAULT 'pending', -- 'pending', 'completed', 'failed', 'refunded'
    payment_method TEXT,
    transaction_reference TEXT,
    created_at TIMESTAMPTZ NOT NULL DEFAULT NOW(),
    updated_at TIMESTAMPTZ NOT NULL DEFAULT NOW()
);

-- 3. SYNCHRONIZATION TRIGGERS (auth.users -> public.profiles)

CREATE OR REPLACE FUNCTION public.handle_new_user()
RETURNS TRIGGER
LANGUAGE plpgsql
SECURITY DEFINER
SET search_path = public
AS $$
DECLARE
    user_role public.role_enum := 'guest';
    meta_role TEXT;
BEGIN
    meta_role := NEW.raw_user_meta_data->>'role';
    IF meta_role IS NOT NULL AND meta_role IN ('guest', 'host', 'administrator') THEN
        user_role := meta_role::public.role_enum;
    END IF;

    INSERT INTO public.profiles (id, full_name, avatar_url, role)
    VALUES (
        NEW.id,
        COALESCE(NEW.raw_user_meta_data->>'full_name', NEW.email),
        NEW.raw_user_meta_data->>'avatar_url',
        user_role
    )
    ON CONFLICT (id) DO UPDATE
    SET
        full_name = EXCLUDED.full_name,
        avatar_url = EXCLUDED.avatar_url,
        updated_at = NOW();

    RETURN NEW;
END;
$$;

DROP TRIGGER IF EXISTS on_auth_user_created ON auth.users;
CREATE TRIGGER on_auth_user_created
AFTER INSERT ON auth.users
FOR EACH ROW EXECUTE FUNCTION public.handle_new_user();

-- Automatic updated_at timestamp trigger
CREATE OR REPLACE FUNCTION public.set_updated_at()
RETURNS TRIGGER
LANGUAGE plpgsql
AS $$
BEGIN
    NEW.updated_at = NOW();
    RETURN NEW;
END;
$$;

DO $$
DECLARE
    t TEXT;
BEGIN
    FOR t IN SELECT unnest(ARRAY['profiles', 'properties', 'reservations', 'reviews', 'payments']) LOOP
        EXECUTE format('DROP TRIGGER IF EXISTS set_updated_at_trigger ON public.%I;', t);
        EXECUTE format('CREATE TRIGGER set_updated_at_trigger BEFORE UPDATE ON public.%I FOR EACH ROW EXECUTE FUNCTION public.set_updated_at();', t);
    END LOOP;
END $$;

-- 4. ROW LEVEL SECURITY (RLS) POLICIES

-- Enable RLS on all tables
ALTER TABLE public.profiles ENABLE ROW LEVEL SECURITY;
ALTER TABLE public.properties ENABLE ROW LEVEL SECURITY;
ALTER TABLE public.availability_property ENABLE ROW LEVEL SECURITY;
ALTER TABLE public.reservations ENABLE ROW LEVEL SECURITY;
ALTER TABLE public.reviews ENABLE ROW LEVEL SECURITY;
ALTER TABLE public.payments ENABLE ROW LEVEL SECURITY;

-- 4.1 Profiles Policies
CREATE POLICY "profiles_select_authenticated"
    ON public.profiles FOR SELECT
    TO authenticated
    USING (TRUE);

CREATE POLICY "profiles_update_owner"
    ON public.profiles FOR UPDATE
    TO authenticated
    USING (auth.uid() = id)
    WITH CHECK (auth.uid() = id);

CREATE POLICY "profiles_admin_all"
    ON public.profiles FOR ALL
    TO authenticated
    USING (public.is_admin());

-- 4.2 Properties Policies
CREATE POLICY "properties_select_all"
    ON public.properties FOR SELECT
    TO authenticated, anon
    USING (is_active = TRUE OR auth.uid() = host_id OR public.is_admin());

CREATE POLICY "properties_insert_host"
    ON public.properties FOR INSERT
    TO authenticated
    WITH CHECK (
        auth.uid() = host_id
        AND EXISTS (
            SELECT 1 FROM public.profiles
            WHERE id = auth.uid() AND role IN ('host', 'administrator')
        )
    );

CREATE POLICY "properties_update_host"
    ON public.properties FOR UPDATE
    TO authenticated
    USING (auth.uid() = host_id OR public.is_admin())
    WITH CHECK (auth.uid() = host_id OR public.is_admin());

CREATE POLICY "properties_delete_host"
    ON public.properties FOR DELETE
    TO authenticated
    USING (auth.uid() = host_id OR public.is_admin());

-- 4.3 Availability Property Policies
CREATE POLICY "availability_select_all"
    ON public.availability_property FOR SELECT
    TO authenticated, anon
    USING (TRUE);

CREATE POLICY "availability_manage_host"
    ON public.availability_property FOR ALL
    TO authenticated
    USING (
        EXISTS (
            SELECT 1 FROM public.properties
            WHERE properties.id = availability_property.property_id
            AND (properties.host_id = auth.uid() OR public.is_admin())
        )
    )
    WITH CHECK (
        EXISTS (
            SELECT 1 FROM public.properties
            WHERE properties.id = availability_property.property_id
            AND (properties.host_id = auth.uid() OR public.is_admin())
        )
    );

-- 4.4 Reservations Policies
CREATE POLICY "reservations_select_participant"
    ON public.reservations FOR SELECT
    TO authenticated
    USING (
        guest_id = auth.uid()
        OR EXISTS (
            SELECT 1 FROM public.properties
            WHERE properties.id = reservations.property_id AND properties.host_id = auth.uid()
        )
        OR public.is_admin()
    );

CREATE POLICY "reservations_insert_guest"
    ON public.reservations FOR INSERT
    TO authenticated
    WITH CHECK (
        guest_id = auth.uid()
        AND EXISTS (
            SELECT 1 FROM public.profiles
            WHERE id = auth.uid() AND role IN ('guest', 'administrator')
        )
    );

CREATE POLICY "reservations_update_participant"
    ON public.reservations FOR UPDATE
    TO authenticated
    USING (
        guest_id = auth.uid()
        OR EXISTS (
            SELECT 1 FROM public.properties
            WHERE properties.id = reservations.property_id AND properties.host_id = auth.uid()
        )
        OR public.is_admin()
    )
    WITH CHECK (
        guest_id = auth.uid()
        OR EXISTS (
            SELECT 1 FROM public.properties
            WHERE properties.id = reservations.property_id AND properties.host_id = auth.uid()
        )
        OR public.is_admin()
    );

-- 4.5 Reviews Policies
CREATE POLICY "reviews_select_all"
    ON public.reviews FOR SELECT
    TO authenticated, anon
    USING (TRUE);

CREATE POLICY "reviews_insert_completed_guest"
    ON public.reviews FOR INSERT
    TO authenticated
    WITH CHECK (
        guest_id = auth.uid()
        AND EXISTS (
            SELECT 1 FROM public.reservations r
            WHERE r.id = reviews.reservation_id
              AND r.guest_id = auth.uid()
              AND r.property_id = reviews.property_id
              AND r.status = 'completed'
        )
    );

CREATE POLICY "reviews_update_delete_owner_or_admin"
    ON public.reviews FOR ALL
    TO authenticated
    USING (guest_id = auth.uid() OR public.is_admin())
    WITH CHECK (guest_id = auth.uid() OR public.is_admin());

-- 4.6 Payments Policies
CREATE POLICY "payments_select_participant"
    ON public.payments FOR SELECT
    TO authenticated
    USING (
        payer_id = auth.uid()
        OR EXISTS (
            SELECT 1 FROM public.reservations r
            JOIN public.properties p ON p.id = r.property_id
            WHERE r.id = payments.reservation_id AND p.host_id = auth.uid()
        )
        OR public.is_admin()
    );

CREATE POLICY "payments_insert_payer"
    ON public.payments FOR INSERT
    TO authenticated
    WITH CHECK (payer_id = auth.uid() OR public.is_admin());

CREATE POLICY "payments_admin_all"
    ON public.payments FOR ALL
    TO authenticated
    USING (public.is_admin());

-- 5. SUPABASE STORAGE BUCKET & POLICIES (property-photos)
-- Structure: host_id/property_id/filename.ext
INSERT INTO storage.buckets (id, name, public)
VALUES ('property-photos', 'property-photos', true)
ON CONFLICT (id) DO UPDATE SET public = true;

-- Allow public viewing of photos
CREATE POLICY "Public Access Property Photos"
    ON storage.objects FOR SELECT
    TO authenticated, anon
    USING (bucket_id = 'property-photos');

-- Allow authenticated hosts/admins to upload to host_id/property_id/...
CREATE POLICY "Host Upload Property Photos"
    ON storage.objects FOR INSERT
    TO authenticated
    WITH CHECK (
        bucket_id = 'property-photos'
        AND (
            (storage.foldername(name))[1] = auth.uid()::text
            OR public.is_admin()
        )
    );

-- Allow host/admin to update/delete their own property photos
CREATE POLICY "Host Manage Property Photos"
    ON storage.objects FOR UPDATE
    TO authenticated
    USING (
        bucket_id = 'property-photos'
        AND (
            (storage.foldername(name))[1] = auth.uid()::text
            OR public.is_admin()
        )
    );

CREATE POLICY "Host Delete Property Photos"
    ON storage.objects FOR DELETE
    TO authenticated
    USING (
        bucket_id = 'property-photos'
        AND (
            (storage.foldername(name))[1] = auth.uid()::text
            OR public.is_admin()
        )
    );
