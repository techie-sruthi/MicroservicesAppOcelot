using MediatR;
using Shared.Kernel.Results;
using UserService.Application.Common.Interfaces;
using UserService.Application.Users.DTOs;

namespace UserService.Application.Users.Commands.RefreshToken;

public class RefreshTokenCommandHandler
    : IRequestHandler<RefreshTokenCommand, Result<LoginResponse>>
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

    public async Task<Result<LoginResponse>> Handle(
        RefreshTokenCommand request,
        CancellationToken cancellationToken)
    {
        var user = await _context.GetUserByRefreshTokenAsync(
            request.RefreshToken,
            cancellationToken);

        if (user == null)
            return Result<LoginResponse>.Failure("Invalid refresh token");

        if (user.RefreshTokenExpiryTime <= DateTime.UtcNow)
            return Result<LoginResponse>.Failure("Refresh token expired");

        var accessToken = _jwtService.GenerateToken(
            user.Id,
            user.Email,
            user.Role
        );

        var newRefreshToken = _jwtService.GenerateRefreshToken();

        user.RefreshToken = newRefreshToken;
        user.RefreshTokenExpiryTime = DateTime.UtcNow.AddDays(7);

        await _context.SaveChangesAsync(cancellationToken);

        return Result<LoginResponse>.Success(new LoginResponse
        {
            AccessToken = accessToken,
            RefreshToken = newRefreshToken,
            ExpiresIn = 3600  // 60 minutes in seconds
        }, "Token refreshed successfully.");
    }
}
