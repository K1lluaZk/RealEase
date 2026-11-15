namespace RealEase.Web.Helpers
{
    public static class PropertyImageHelper
    {
        private static readonly string[] DefaultImages = new[]
        {
            "https://images.unsplash.com/photo-1600585154340-be6161a56a0c?auto=format&fit=crop&w=1200&q=80", // Modern luxury villa with pool
            "https://images.unsplash.com/photo-1600596542815-ffad4c1539a9?auto=format&fit=crop&w=1200&q=80", // Suburban modern home
            "https://images.unsplash.com/photo-1545324418-cc1a3fa10c00?auto=format&fit=crop&w=1200&q=80", // Chic urban apartment
            "https://images.unsplash.com/photo-1600607687939-ce8a6c25118c?auto=format&fit=crop&w=1200&q=80", // Modern living room / penthouse
            "https://images.unsplash.com/photo-1512917774080-9991f1c4c750?auto=format&fit=crop&w=1200&q=80", // Sunny luxury residence
            "https://images.unsplash.com/photo-1613490493576-7fde63acd811?auto=format&fit=crop&w=1200&q=80", // Minimalist architectural villa
            "https://images.unsplash.com/photo-1560448204-e02f11c3d0e2?auto=format&fit=crop&w=1200&q=80", // Contemporary condominium
            "https://images.unsplash.com/photo-1600566753376-12c8ab7fb75b?auto=format&fit=crop&w=1200&q=80"  // Architectural patio & garden
        };

        public static string GetImageUrl(string? existingImage, long propertyId = 0, string? propertyType = null)
        {
            if (!string.IsNullOrWhiteSpace(existingImage))
            {
                return existingImage;
            }

            var index = (int)(Math.Abs(propertyId) % DefaultImages.Length);
            return DefaultImages[index];
        }

        public static IReadOnlyList<string> GetGallery(IEnumerable<string>? existingImages, long propertyId = 0)
        {
            var list = existingImages?.Where(img => !string.IsNullOrWhiteSpace(img)).ToList() ?? new List<string>();
            if (list.Count > 0)
            {
                return list;
            }

            // Return a curated gallery set based on the property id
            var startIndex = (int)(Math.Abs(propertyId) % DefaultImages.Length);
            var gallery = new List<string>();
            for (int i = 0; i < 4; i++)
            {
                gallery.Add(DefaultImages[(startIndex + i) % DefaultImages.Length]);
            }
            return gallery;
        }
    }
}
