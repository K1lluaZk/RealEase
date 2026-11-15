namespace RealEase.Application.Dtos.AgentInquiry;

public sealed class AgentInquiryResponseDto
{
    public string AgentName { get; set; } = string.Empty;
    public string AgentEmail { get; set; } = string.Empty;
    public string? AgentPhoneNumber { get; set; }
}
