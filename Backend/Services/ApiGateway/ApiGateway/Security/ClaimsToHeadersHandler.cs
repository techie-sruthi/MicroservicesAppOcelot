using System.Net.Http;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace ApiGateway.Security;

public class ClaimsToHeadersHandler : DelegatingHandler
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public ClaimsToHeadersHandler(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        var user = _httpContextAccessor.HttpContext?.User;
        if (user?.Identity?.IsAuthenticated == true)
        {
            var id = user.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (!string.IsNullOrEmpty(id) && !request.Headers.Contains("X-User-Id"))
                request.Headers.Add("X-User-Id", id);

            var role = user.FindFirst(ClaimTypes.Role)?.Value;
            if (!string.IsNullOrEmpty(role) && !request.Headers.Contains("X-User-Role"))
                request.Headers.Add("X-User-Role", role);

            var email = user.FindFirst("email")?.Value;
            if (!string.IsNullOrEmpty(email) && !request.Headers.Contains("X-User-Email"))
                request.Headers.Add("X-User-Email", email);
        }

        return base.SendAsync(request, cancellationToken);
    }
}
