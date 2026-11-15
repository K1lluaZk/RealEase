using System.Security.Claims;
using System.Text.Json;
using Microsoft.AspNetCore.Authentication;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using RealEase.Domain.Models;
using RealEase.Persistence.Context;

namespace RealEase.API.Authentication;

public sealed class SupabaseBearerHandler(
    IOptionsMonitor<AuthenticationSchemeOptions> options,
    ILoggerFactory logger,
    System.Text.Encodings.Web.UrlEncoder encoder,
    IHttpClientFactory httpClientFactory,
    RealEaseDbContext dbContext)
    : AuthenticationHandler<AuthenticationSchemeOptions>(options, logger, encoder)
{
    protected override async Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        var authorization = Request.Headers.Authorization.ToString();
        if (!authorization.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase))
            return AuthenticateResult.NoResult();

        var accessToken = authorization["Bearer ".Length..].Trim();
        if (string.IsNullOrWhiteSpace(accessToken))
            return AuthenticateResult.Fail("Bearer token is missing.");

        try
        {
            using var request = new HttpRequestMessage(HttpMethod.Get, "auth/v1/user");
            request.Headers.Authorization =
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", accessToken);

            using var response = await httpClientFactory.CreateClient("SupabaseAuth")
                .SendAsync(request, Context.RequestAborted);
            if (!response.IsSuccessStatusCode)
                return AuthenticateResult.Fail("Supabase rejected the access token.");

            await using var responseStream = await response.Content.ReadAsStreamAsync(Context.RequestAborted);
            using var userDocument = await JsonDocument.ParseAsync(responseStream, cancellationToken: Context.RequestAborted);
            if (!userDocument.RootElement.TryGetProperty("id", out var idElement) ||
                !Guid.TryParse(idElement.GetString(), out var userId))
                return AuthenticateResult.Fail("Supabase returned an invalid user identity.");

            var user = await dbContext.Users.AsNoTracking()
                .SingleOrDefaultAsync(candidate => candidate.Id == userId, Context.RequestAborted);
            var role = UserRoles.Normalize(user?.Role);
            if (user is null || !user.IsActive || role is null)
                return AuthenticateResult.Fail("The account is inactive or has no supported role.");

            var claims = new List<Claim>
            {
                new(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new(ClaimTypes.Name, $"{user.FirstName} {user.LastName}".Trim()),
                new(ClaimTypes.Email, user.Email),
                new(ClaimTypes.Role, role),
                new("access_token", accessToken)
            };
            var identity = new ClaimsIdentity(claims, Scheme.Name);
            var ticket = new AuthenticationTicket(new ClaimsPrincipal(identity), Scheme.Name);
            return AuthenticateResult.Success(ticket);
        }
        catch (OperationCanceledException) when (Context.RequestAborted.IsCancellationRequested)
        {
            throw;
        }
        catch (Exception exception) when (exception is HttpRequestException or JsonException)
        {
            Logger.LogWarning(exception, "Supabase bearer token validation failed.");
            return AuthenticateResult.Fail("Unable to validate the access token.");
        }
    }
}
