using System.Security.Claims;
using ProductService.Application.Common.Interfaces;

namespace ProductService.API.Services;

public class CurrentUserService : ICurrentUserService
{
	private readonly IHttpContextAccessor _httpContextAccessor;

	public CurrentUserService(IHttpContextAccessor httpContextAccessor)
	{
		_httpContextAccessor = httpContextAccessor;
	}

	public int GetUserId()
	{
		var user = _httpContextAccessor.HttpContext?.User;

		var userIdClaim = user?.FindFirst(ClaimTypes.NameIdentifier)?.Value
						  ?? user?.FindFirst("sub")?.Value
						  ?? user?.FindFirst("userId")?.Value;

		if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out int userId))
		{
			throw new UnauthorizedAccessException("User ID not found or invalid in token");
		}

		return userId;
	}

	public bool IsAdmin =>
		_httpContextAccessor.HttpContext?.User.IsInRole("Admin") ?? false;
}