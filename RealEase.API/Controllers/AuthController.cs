using System.Text.Json;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RealEase.Application.Dtos.Supabase;
using RealEase.Application.Services;
using RealEase.Domain.Models;
using RealEase.Persistence.Context;
using Supabase.Gotrue.Exceptions;

namespace RealEase.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public sealed class AuthController(
    IAuthService authService,
    RealEaseDbContext dbContext) : ControllerBase
{
    [AllowAnonymous]
    [HttpPost("login")]
    public async Task<ActionResult<LoginResponse>> Login(
        [FromBody] LoginUserRequest request,
        CancellationToken cancellationToken)
    {
        AuthResponse result;
        try
        {
            result = await authService.LoginAsync(request);
        }
        catch (UnauthorizedAccessException)
        {
            return Unauthorized();
        }
        catch (GotrueException exception) when (
            exception.StatusCode is (int)System.Net.HttpStatusCode.BadRequest or (int)System.Net.HttpStatusCode.Unauthorized)
        {
            return Unauthorized();
        }

        if (!Guid.TryParse(result.UserId, out var userId) || string.IsNullOrWhiteSpace(result.AccessToken))
            return Unauthorized();

        var user = await dbContext.Users.AsNoTracking()
            .SingleOrDefaultAsync(candidate => candidate.Id == userId, cancellationToken);
        var role = UserRoles.Normalize(user?.Role);
        if (user is null || !user.IsActive || role is null)
            return Unauthorized();

        if (!TryReadTokenExpiry(result.AccessToken, out var expiresAt) || expiresAt <= DateTimeOffset.UtcNow)
            return Unauthorized();

        return Ok(new LoginResponse(
            result.AccessToken,
            user.Id,
            user.Email,
            $"{user.FirstName} {user.LastName}".Trim(),
            role,
            expiresAt));
    }

    private static bool TryReadTokenExpiry(string accessToken, out DateTimeOffset expiresAt)
    {
        expiresAt = default;
        var parts = accessToken.Split('.');
        if (parts.Length != 3)
            return false;

        var payload = parts[1].Replace('-', '+').Replace('_', '/');
        payload = payload.PadRight(payload.Length + (4 - payload.Length % 4) % 4, '=');
        try
        {
            using var document = JsonDocument.Parse(Convert.FromBase64String(payload));
            if (!document.RootElement.TryGetProperty("exp", out var expiry) ||
                !expiry.TryGetInt64(out var unixSeconds))
                return false;

            expiresAt = DateTimeOffset.FromUnixTimeSeconds(unixSeconds);
            return true;
        }
        catch (FormatException)
        {
            return false;
        }
        catch (JsonException)
        {
            return false;
        }
    }
}

public sealed record LoginResponse(
    string AccessToken,
    Guid UserId,
    string Email,
    string FullName,
    string Role,
    DateTimeOffset ExpiresAt);
