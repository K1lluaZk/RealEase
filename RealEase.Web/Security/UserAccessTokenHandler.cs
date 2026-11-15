using System.Net.Http.Headers;
using System.Security.Claims;

namespace RealEase.Web.Security;

public sealed class UserAccessTokenHandler(IHttpContextAccessor httpContextAccessor) : DelegatingHandler
{
    protected override Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request,
        CancellationToken cancellationToken)
    {
        var accessToken = httpContextAccessor.HttpContext?.User.FindFirstValue("access_token");
        if (!string.IsNullOrWhiteSpace(accessToken))
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

        return base.SendAsync(request, cancellationToken);
    }
}
