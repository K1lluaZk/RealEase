using System;

namespace RealEase.Application.Dtos.Contract
{
    public class ContractDto
    {
        public long Id { get; set; }
        public Guid ClientId { get; set; }
        public Guid? AgentId { get; set; }
        public long PropertyId { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public decimal TotalAmount { get; set; }
        public string Status { get; set; } = string.Empty;
    }
}
