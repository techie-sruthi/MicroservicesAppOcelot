using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Shared.Kernel.Exceptions;
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
                var (statusCode, message) = ex switch
                {
                    ConfigurationMissingException =>
                        (StatusCodes.Status500InternalServerError, ex.Message),
                    _ =>
                        (StatusCodes.Status500InternalServerError, "An internal server error occurred.")
                };

                context.Response.StatusCode = statusCode;
                var response = ApiResponse<object>.Fail(message);
                await context.Response.WriteAsJsonAsync(response);
            }
        }
    }
}
