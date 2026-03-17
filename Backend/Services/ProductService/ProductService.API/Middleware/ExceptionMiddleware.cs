namespace ProductService.API.Middleware;

public class ConfigurationMissingException : Exception
{
    public string ConfigurationKey { get; }

    public ConfigurationMissingException(string configurationKey)
        : base($"{configurationKey} is missing in configuration.")
    {
        ConfigurationKey = configurationKey;
    }

    public ConfigurationMissingException(string configurationKey, Exception innerException)
        : base($"{configurationKey} is missing in configuration.", innerException)
    {
        ConfigurationKey = configurationKey;
    }
}

public class ExceptionMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionMiddleware> _logger;

    public ExceptionMiddleware(RequestDelegate next, ILogger<ExceptionMiddleware> logger)
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
        catch (KeyNotFoundException ex)
        {
            _logger.LogWarning(ex, "Resource not found");
            if (!context.Response.HasStarted)
            {
                context.Response.StatusCode = StatusCodes.Status404NotFound;
                await context.Response.WriteAsJsonAsync(new { error = ex.Message });
            }
        }
        catch (UnauthorizedAccessException ex)
        {
            _logger.LogWarning(ex, "Unauthorized access attempt");
            if (!context.Response.HasStarted)
            {
                context.Response.StatusCode = StatusCodes.Status403Forbidden;
                await context.Response.WriteAsJsonAsync(new { error = ex.Message });
            }
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning(ex, "Bad request");
            if (!context.Response.HasStarted)
            {
                context.Response.StatusCode = StatusCodes.Status400BadRequest;
                await context.Response.WriteAsJsonAsync(new { error = ex.Message });
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unhandled exception occurred");
            if (!context.Response.HasStarted)
            {
                context.Response.StatusCode = StatusCodes.Status500InternalServerError;
                await context.Response.WriteAsJsonAsync(new { error = "An internal server error occurred" });
            }
        }
    }
}
