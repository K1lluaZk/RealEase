using System;
using Postgrest.Attributes;
using Postgrest.Models;

namespace RealEase.Domain.Models.Supabase
{
    [Table("payments")]
    public class PaymentModel : BaseModel
    {
        [PrimaryKey("id", shouldInsert: false)]
        public string? Id { get; set; }

        [Column("reservation_id")]
        public string ReservationId { get; set; } = string.Empty;

        [Column("payer_id")]
        public string PayerId { get; set; } = string.Empty;

        [Column("amount")]
        public decimal Amount { get; set; }

        [Column("platform_commission")]
        public decimal PlatformCommission { get; set; } = 0.00m;

        [Column("status")]
        public string Status { get; set; } = "pending";

        [Column("payment_method")]
        public string? PaymentMethod { get; set; }

        [Column("transaction_reference")]
        public string? TransactionReference { get; set; }

        [Column("created_at")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [Column("updated_at")]
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    }
}
