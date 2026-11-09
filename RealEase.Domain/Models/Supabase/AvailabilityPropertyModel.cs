using System;
using Postgrest.Attributes;
using Postgrest.Models;

namespace RealEase.Domain.Models.Supabase
{
    [Table("availability_property")]
    public class AvailabilityPropertyModel : BaseModel
    {
        [PrimaryKey("id", shouldInsert: false)]
        public string? Id { get; set; }

        [Column("property_id")]
        public string PropertyId { get; set; } = string.Empty;

        [Column("available_range")]
        public string AvailableRange { get; set; } = string.Empty;

        [Column("created_at")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
