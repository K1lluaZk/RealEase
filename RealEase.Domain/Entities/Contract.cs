using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace RealEase.Domain.Entities;

[Table("contracts")]
public class Contract
{
    public long Id { get; set; }
    public Guid ClientId { get; set; }
    public Guid? AgentId { get; set; }
    public long PropertyId { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public decimal TotalAmount { get; set; }
    public string? Status { get; set; }
    public DateTime CreatedAt { get; set; }

    [NotMapped]
    public decimal MonthlyAmount
    {
        get => TotalAmount;
        set => TotalAmount = value;
    }
}
