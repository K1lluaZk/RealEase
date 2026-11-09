using System;
using System.Collections.Generic;
using System.Linq;
using System.Globalization;
using System.Threading.Tasks;
using RealEase.Application.Dtos.Supabase;
using RealEase.Domain.Models.Supabase;

namespace RealEase.Application.Services
{
    public interface IBookingService
    {
        Task<AvailabilityCheckResult> ValidateAvailabilityAsync(string propertyId, DateTime checkIn, DateTime checkOut);
        Task<ReservationResponse> CreateReservationAsync(string guestId, CreateReservationRequest request);
        Task<ReservationResponse> UpdateReservationStatusAsync(string reservationId, string userId, ReservationStatus newStatus);
        Task<List<ReservationResponse>> GetGuestReservationsAsync(string guestId);
        Task<List<ReservationResponse>> GetHostReservationsAsync(string hostId);
    }

    public class BookingService : IBookingService
    {
        private readonly Supabase.Client _supabase;

        public BookingService(Supabase.Client supabase)
        {
            _supabase = supabase ?? throw new ArgumentNullException(nameof(supabase));
        }

        /// <summary>
        /// Validates if a property is available for the given check-in and check-out dates.
        /// Checks host availability calendar and existing reservations for overlaps.
        /// </summary>
        public async Task<AvailabilityCheckResult> ValidateAvailabilityAsync(string propertyId, DateTime checkIn, DateTime checkOut)
        {
            if (string.IsNullOrWhiteSpace(propertyId))
                throw new ArgumentNullException(nameof(propertyId));

            if (!long.TryParse(propertyId, NumberStyles.None, CultureInfo.InvariantCulture, out var propertyIdValue))
            {
                return new AvailabilityCheckResult
                {
                    IsAvailable = false,
                    Message = "Property ID must be a numeric database ID."
                };
            }

            if (checkOut.Date <= checkIn.Date)
            {
                return new AvailabilityCheckResult
                {
                    IsAvailable = false,
                    Message = "Check-out date must be after check-in date."
                };
            }

            if (checkIn.Date < DateTime.UtcNow.Date)
            {
                return new AvailabilityCheckResult
                {
                    IsAvailable = false,
                    Message = "Check-in date cannot be in the past."
                };
            }

            // 1. Fetch Property info
            var propResponse = await _supabase
                .From<PropertyModel>()
                .Where(p => p.Id == propertyIdValue)
                .Get();

            if (propResponse.Models.Count == 0)
            {
                return new AvailabilityCheckResult
                {
                    IsAvailable = false,
                    Message = "Property does not exist."
                };
            }

            var property = propResponse.Models[0];
            if (string.Equals(property.Status, "inactive", StringComparison.OrdinalIgnoreCase))
            {
                return new AvailabilityCheckResult
                {
                    IsAvailable = false,
                    Message = "Property is currently inactive."
                };
            }

            var requestedRange = new DateRange(checkIn, checkOut);

            // 2. Fetch Availability declared by host
            var availResponse = await _supabase
                .From<AvailabilityPropertyModel>()
                .Where(a => a.PropertyId == propertyId)
                .Get();

            bool coveredByHostCalendar = false;
            foreach (var avail in availResponse.Models)
            {
                if (DateRange.TryParse(avail.AvailableRange, out var range))
                {
                    if (range.ContainsRange(requestedRange))
                    {
                        coveredByHostCalendar = true;
                        break;
                    }
                }
            }

            if (availResponse.Models.Count > 0 && !coveredByHostCalendar)
            {
                return new AvailabilityCheckResult
                {
                    IsAvailable = false,
                    Message = "Selected dates are outside the host's declared availability calendar."
                };
            }

            // 3. Fetch existing active reservations to verify no overlap
            var resResponse = await _supabase
                .From<ReservationModel>()
                .Where(r => r.PropertyId == propertyId)
                .Get();

            var activeReservations = resResponse.Models
                .Where(r => r.Status == "pending" || r.Status == "confirmed")
                .ToList();

            foreach (var existing in activeReservations)
            {
                if (DateRange.TryParse(existing.StayRange, out var existingRange))
                {
                    if (existingRange.Overlaps(requestedRange))
                    {
                        return new AvailabilityCheckResult
                        {
                            IsAvailable = false,
                            Message = "Requested dates overlap with an existing reservation."
                        };
                    }
                }
            }

            var nights = requestedRange.TotalNights;
            var estimatedPrice = nights * property.PricePerNight;

            return new AvailabilityCheckResult
            {
                IsAvailable = true,
                TotalNights = nights,
                PricePerNight = property.PricePerNight,
                TotalEstimatedPrice = estimatedPrice,
                Message = "Dates are available for booking."
            };
        }

        /// <summary>
        /// Creates a reservation after validating availability.
        /// Relies on PostgreSQL GIST exclusion constraint for strict concurrency safety.
        /// </summary>
        public async Task<ReservationResponse> CreateReservationAsync(string guestId, CreateReservationRequest request)
        {
            if (string.IsNullOrWhiteSpace(guestId))
                throw new ArgumentNullException(nameof(guestId));

            // Validate availability
            var availability = await ValidateAvailabilityAsync(request.PropertyId, request.CheckIn, request.CheckOut);
            if (!availability.IsAvailable)
            {
                throw new InvalidOperationException($"Cannot create reservation: {availability.Message}");
            }

            var stayRange = new DateRange(request.CheckIn, request.CheckOut);

            var reservation = new ReservationModel
            {
                PropertyId = request.PropertyId,
                GuestId = guestId,
                StayRange = stayRange.ToString(),
                Status = "pending",
                TotalAmount = availability.TotalEstimatedPrice,
                Notes = request.Notes,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            // Post to Supabase (PostgreSQL will enforce the GIST exclusion constraint)
            var insertResponse = await _supabase
                .From<ReservationModel>()
                .Insert(reservation);

            if (insertResponse.Models.Count == 0)
            {
                throw new InvalidOperationException("Failed to persist reservation.");
            }

            var created = insertResponse.Models[0];

            // Fetch property and guest for detailed response
            var propertyId = ParsePropertyId(request.PropertyId);
            var prop = await _supabase.From<PropertyModel>().Where(p => p.Id == propertyId).Single();
            var guest = await _supabase.From<ProfileModel>().Where(p => p.Id == guestId).Single();

            return new ReservationResponse
            {
                Id = created.Id ?? string.Empty,
                PropertyId = created.PropertyId,
                PropertyTitle = prop?.Title ?? string.Empty,
                GuestId = created.GuestId,
                GuestName = guest is null ? string.Empty : JoinName(guest.FirstName, guest.LastName),
                CheckIn = stayRange.Start,
                CheckOut = stayRange.End,
                TotalNights = stayRange.TotalNights,
                Status = created.Status,
                TotalAmount = created.TotalAmount,
                Notes = created.Notes,
                CreatedAt = created.CreatedAt
            };
        }

        /// <summary>
        /// Updates the status of a reservation (e.g. host confirms or cancels).
        /// </summary>
        public async Task<ReservationResponse> UpdateReservationStatusAsync(string reservationId, string userId, ReservationStatus newStatus)
        {
            var resResponse = await _supabase
                .From<ReservationModel>()
                .Where(r => r.Id == reservationId)
                .Single();

            if (resResponse == null)
                throw new KeyNotFoundException($"Reservation with ID {reservationId} not found.");

            resResponse.Status = newStatus.ToString().ToLowerInvariant();
            resResponse.UpdatedAt = DateTime.UtcNow;

            var updateResponse = await _supabase
                .From<ReservationModel>()
                .Update(resResponse);

            var updated = updateResponse.Models[0];
            DateRange.TryParse(updated.StayRange, out var range);

            return new ReservationResponse
            {
                Id = updated.Id ?? string.Empty,
                PropertyId = updated.PropertyId,
                GuestId = updated.GuestId,
                CheckIn = range.Start,
                CheckOut = range.End,
                TotalNights = range.TotalNights,
                Status = updated.Status,
                TotalAmount = updated.TotalAmount,
                Notes = updated.Notes,
                CreatedAt = updated.CreatedAt
            };
        }

        /// <summary>
        /// Lists all reservations made by a specific guest.
        /// </summary>
        public async Task<List<ReservationResponse>> GetGuestReservationsAsync(string guestId)
        {
            var response = await _supabase
                .From<ReservationModel>()
                .Where(r => r.GuestId == guestId)
                .Get();

            return response.Models.Select(m =>
            {
                DateRange.TryParse(m.StayRange, out var range);
                return new ReservationResponse
                {
                    Id = m.Id ?? string.Empty,
                    PropertyId = m.PropertyId,
                    GuestId = m.GuestId,
                    CheckIn = range.Start,
                    CheckOut = range.End,
                    TotalNights = range.TotalNights,
                    Status = m.Status,
                    TotalAmount = m.TotalAmount,
                    Notes = m.Notes,
                    CreatedAt = m.CreatedAt
                };
            }).ToList();
        }

        /// <summary>
        /// Lists all reservations for properties owned by a host.
        /// </summary>
        public async Task<List<ReservationResponse>> GetHostReservationsAsync(string hostId)
        {
            // Get host properties
            if (!Guid.TryParse(hostId, out var ownerId))
                throw new ArgumentException("Host ID must be a valid UUID.", nameof(hostId));

            var props = await _supabase
                .From<PropertyModel>()
                .Where(p => p.OwnerId == ownerId)
                .Get();

            var propIds = props.Models.Select(p => p.Id).ToList();
            if (propIds.Count == 0) return new List<ReservationResponse>();

            var allReservations = new List<ReservationResponse>();
            foreach (var propId in propIds)
            {
                var response = await _supabase
                    .From<ReservationModel>()
                    .Where(r => r.PropertyId == propId.ToString(CultureInfo.InvariantCulture))
                    .Get();

                foreach (var m in response.Models)
                {
                    DateRange.TryParse(m.StayRange, out var range);
                    allReservations.Add(new ReservationResponse
                    {
                        Id = m.Id ?? string.Empty,
                        PropertyId = m.PropertyId,
                        GuestId = m.GuestId,
                        CheckIn = range.Start,
                        CheckOut = range.End,
                        TotalNights = range.TotalNights,
                        Status = m.Status,
                        TotalAmount = m.TotalAmount,
                        Notes = m.Notes,
                        CreatedAt = m.CreatedAt
                    });
                }
            }

            return allReservations;
        }

        private static long ParsePropertyId(string propertyId) =>
            long.TryParse(propertyId, NumberStyles.None, CultureInfo.InvariantCulture, out var id)
                ? id
                : throw new ArgumentException("Property ID must be a numeric database ID.", nameof(propertyId));

        private static string JoinName(string firstName, string lastName) =>
            string.Join(' ', new[] { firstName, lastName }.Where(part => !string.IsNullOrWhiteSpace(part)));
    }
}
