using MediatR;
using UserService.Application.Common.Interfaces;
using UserService.Application.Users.DTOs;

namespace UserService.Application.Users.Commands.RefreshToken;

public class RefreshTokenCommandHandler
    : IRequestHandler<RefreshTokenCommand, LoginResponse>
{
    private readonly IUserDbContext _context;
    private readonly IJwtService _jwtService;

    public RefreshTokenCommandHandler(
        IUserDbContext context,
        IJwtService jwtService)
    {
        _context = context;
        _jwtService = jwtService;
    }

    public async Task<LoginResponse> Handle(
        RefreshTokenCommand request,
        CancellationToken cancellationToken)
    {
        // Find user by refresh token
        var user = await _context.GetUserByRefreshTokenAsync(
            request.RefreshToken,
            cancellationToken);

        if (user == null)
            throw new UnauthorizedAccessException("Invalid refresh token");

        // Check if refresh token is expired
        if (user.RefreshTokenExpiryTime <= DateTime.UtcNow)
            throw new UnauthorizedAccessException("Refresh token expired");

        // Generate new access token
        var accessToken = _jwtService.GenerateToken(
            user.Id,
            user.Email,
            user.Role
        );

        // Generate new refresh token (token rotation)
        var newRefreshToken = _jwtService.GenerateRefreshToken();

        // Update refresh token in database
        user.RefreshToken = newRefreshToken;
        user.RefreshTokenExpiryTime = DateTime.UtcNow.AddDays(7);
        
        await _context.SaveChangesAsync(cancellationToken);

        return new LoginResponse
        {
            AccessToken = accessToken,
            RefreshToken = newRefreshToken,
            ExpiresIn = 3600  // 60 minutes in seconds
        };
    }
}
