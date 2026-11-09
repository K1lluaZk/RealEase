using System;
using Postgrest.Attributes;
using Postgrest.Models;

namespace RealEase.Domain.Models.Supabase
{
    [Table("reservations")]
    public class ReservationModel : BaseModel
    {
        [PrimaryKey("id", shouldInsert: false)]
        public string? Id { get; set; }

        [Column("property_id")]
        public string PropertyId { get; set; } = string.Empty;

        [Column("guest_id")]
        public string GuestId { get; set; } = string.Empty;

        [Column("stay_range")]
        public string StayRange { get; set; } = string.Empty;

        [Column("status")]
        public string Status { get; set; } = "pending";

        [Column("total_amount")]
        public decimal TotalAmount { get; set; }

        [Column("notes")]
        public string? Notes { get; set; }

        [Column("created_at")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [Column("updated_at")]
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    }
}
