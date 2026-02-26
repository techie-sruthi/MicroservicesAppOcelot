using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace ProductService.API.Helpers;

public static class JwtHelper
{
    public static int GetUserIdFromToken(ControllerBase controller, ILogger logger)
    {
        var userIdClaim = controller.User.FindFirst(ClaimTypes.NameIdentifier)?.Value
                       ?? controller.User.FindFirst("sub")?.Value
                       ?? controller.User.FindFirst("userId")?.Value;

        if (string.IsNullOrEmpty(userIdClaim))
        {
            logger.LogError("UserId claim not found in JWT token. Available claims: {Claims}",
                string.Join(", ", controller.User.Claims.Select(c => $"{c.Type}={c.Value}")));
            throw new UnauthorizedAccessException("User ID not found in token");
        }

        if (!int.TryParse(userIdClaim, out int userId))
        {
            logger.LogError($"Invalid userId format in token: {userIdClaim}");
            throw new UnauthorizedAccessException("Invalid user ID format");
        }

        logger.LogInformation($"Extracted userId: {userId} from token");
        return userId;
    }

   
    public static string? GetUserRole(ControllerBase controller)
    {
        return controller.User.FindFirst(ClaimTypes.Role)?.Value
            ?? controller.User.FindFirst("role")?.Value;
    }

    
    public static bool IsAdmin(ControllerBase controller)
    {
        return controller.User.IsInRole("Admin");
    }

    public static (int CurrentUserId, bool IsAdmin) GetAuthorizationContext(ControllerBase controller, ILogger logger)
    {
        return (
            CurrentUserId: GetUserIdFromToken(controller, logger),
            IsAdmin: IsAdmin(controller)
        );
    }
}
