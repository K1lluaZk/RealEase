namespace RealEase.Domain.Models;

public static class UserRoles
{
    public const string Client = "client";
    public const string Agent = "agent";
    public const string Admin = "admin";

    public static string? Normalize(string? role) => role?.Trim().ToLowerInvariant() switch
    {
        "guest" or "client" or "cliente" => Client,
        "host" or "agent" or "agente" or "agente de ventas" => Agent,
        "administrator" or "admin" or "administrador" => Admin,
        _ => null
    };

    public static string ToStorageRole(string normalizedRole) => normalizedRole switch
    {
        Client => "guest",
        Agent => "host",
        Admin => "administrator",
        _ => throw new ArgumentOutOfRangeException(nameof(normalizedRole), "Unsupported user role.")
    };
}
