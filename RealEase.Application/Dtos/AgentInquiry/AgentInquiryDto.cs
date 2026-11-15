using System.ComponentModel.DataAnnotations;

namespace RealEase.Application.Dtos.AgentInquiry;

public sealed class AgentInquiryDto
{
    [Required, StringLength(120)]
    public string Name { get; set; } = string.Empty;

    [Required, EmailAddress, StringLength(254)]
    public string Email { get; set; } = string.Empty;

    [Required, Phone, StringLength(40)]
    public string PhoneNumber { get; set; } = string.Empty;

    [Required, StringLength(120)]
    public string Location { get; set; } = string.Empty;

    [Required, RegularExpression("^(rent|sell)$")]
    public string Intent { get; set; } = string.Empty;

    [StringLength(1000)]
    public string? Message { get; set; }

    public long? PropertyId { get; set; }
}
