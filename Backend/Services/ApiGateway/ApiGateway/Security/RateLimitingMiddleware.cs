using System.Security.Claims;

namespace ApiGateway.Security;

public class RateLimitingMiddleware
{
    private readonly RequestDelegate _next;

    public RateLimitingMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        // If user is authenticated
        if (context.User?.Identity?.IsAuthenticated == true)
        {
            // Pick ONE claim as ClientId
            var clientId =
                context.User.FindFirst(ClaimTypes.NameIdentifier)?.Value // sub
                ?? context.User.FindFirst("sub")?.Value
                ?? context.User.FindFirst(ClaimTypes.Email)?.Value;

            if (!string.IsNullOrEmpty(clientId))
            {
                // Add ClientId header for Ocelot
                context.Request.Headers["ClientId"] = clientId;
            }
        }

        await _next(context);
    }
}
