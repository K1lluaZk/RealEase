namespace RealEase.Web.Models;

public sealed class LoginResponseModel
{
    public string AccessToken { get; set; } = string.Empty;
    public Guid UserId { get; set; }
    public string Email { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public string Role { get; set; } = string.Empty;
    public DateTimeOffset ExpiresAt { get; set; }
}
