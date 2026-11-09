using System;

namespace RealEase.Application.Dtos.Payment
{
    public class PaymentDto
    {
        public long Id { get; set; }
        public long? ContractId { get; set; }
        public Guid TenantId { get; set; }
        public DateTime PaymentDate { get; set; }
        public decimal Amount { get; set; }
        public string PaymentMethod { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
    }
}
