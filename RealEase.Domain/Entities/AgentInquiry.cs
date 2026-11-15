using System.ComponentModel.DataAnnotations.Schema;

namespace RealEase.Domain.Entities;

[Table("agent_inquiries")]
public class AgentInquiry
{
    public long Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string PhoneNumber { get; set; } = string.Empty;
    public string Location { get; set; } = string.Empty;
    public string Intent { get; set; } = string.Empty;
    public string? Message { get; set; }
    public long? PropertyId { get; set; }
    public Guid? AgentId { get; set; }
    public string Status { get; set; } = "pending";
    public DateTime CreatedAt { get; set; }
}
