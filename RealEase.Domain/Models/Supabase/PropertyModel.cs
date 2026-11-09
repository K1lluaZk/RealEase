using System;
using System.Collections.Generic;
using Postgrest.Attributes;
using Postgrest.Models;

namespace RealEase.Domain.Models.Supabase
{
    [Table("properties")]
    public class PropertyModel : BaseModel
    {
        [PrimaryKey("id", shouldInsert: false)]
        public long Id { get; set; }

        [Column("title")]
        public string Title { get; set; } = string.Empty;

        [Column("description")]
        public string? Description { get; set; }

        [Column("address")]
        public string Address { get; set; } = string.Empty;

        [Column("price_per_night")]
        public decimal PricePerNight { get; set; }

        [Column("property_type")]
        public string PropertyType { get; set; } = string.Empty;

        [Column("status")]
        public string? Status { get; set; }

        [Column("owner_id")]
        public Guid? OwnerId { get; set; }

        [Column("images")]
        public List<string>? Images { get; set; }

        [Column("bedrooms")]
        public int? Bedrooms { get; set; }

        [Column("bathrooms")]
        public int? Bathrooms { get; set; }

        [Column("max_guests")]
        public int? MaxGuests { get; set; }

        [Column("created_at")]
        public DateTime CreatedAt { get; set; }

        [Column("updated_at")]
        public DateTime? UpdatedAt { get; set; }
    }
}
