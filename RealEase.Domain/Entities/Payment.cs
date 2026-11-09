using System;

namespace RealEase.Domain.Entities;
using System.ComponentModel.DataAnnotations.Schema;

[Table("payments")]
    public class Payment
    {
        public long Id { get; set; }
        public long? ContractId { get; set; }
        public Guid TenantId { get; set; }
        public decimal Amount { get; set; }
        public string PaymentMethod { get; set; } = string.Empty;
        public string? Status { get; set; }
        public DateTime PaymentDate { get; set; }
    }
