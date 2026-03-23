using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Shared.Kernel.Responses;

namespace Shared.Kernel.Middleware;

public class GlobalExceptionMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<GlobalExceptionMiddleware> _logger;

    public GlobalExceptionMiddleware(RequestDelegate next, ILogger<GlobalExceptionMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unhandled exception occurred");

            if (!context.Response.HasStarted)
            {
                context.Response.StatusCode = StatusCodes.Status500InternalServerError;
                var response = ApiResponse<object>.Fail("An internal server error occurred.");
                await context.Response.WriteAsJsonAsync(response);
            }
        }
    }
}
