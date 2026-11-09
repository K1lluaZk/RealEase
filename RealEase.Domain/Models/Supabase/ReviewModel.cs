using System;
using Postgrest.Attributes;
using Postgrest.Models;

namespace RealEase.Domain.Models.Supabase
{
    [Table("reviews")]
    public class ReviewModel : BaseModel
    {
        [PrimaryKey("id", shouldInsert: false)]
        public string? Id { get; set; }

        [Column("property_id")]
        public string PropertyId { get; set; } = string.Empty;

        [Column("guest_id")]
        public string GuestId { get; set; } = string.Empty;

        [Column("reservation_id")]
        public string ReservationId { get; set; } = string.Empty;

        [Column("rating")]
        public short Rating { get; set; }

        [Column("comment")]
        public string? Comment { get; set; }

        [Column("created_at")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [Column("updated_at")]
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    }
}
