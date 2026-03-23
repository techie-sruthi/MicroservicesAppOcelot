using MediatR;
using Shared.Kernel.Results;
using UserService.Application.Common.Interfaces;

namespace UserService.Application.Users.Commands.Logout;

public class LogoutCommandHandler : IRequestHandler<LogoutCommand, Result>
{
    private readonly IUserDbContext _context;

    public LogoutCommandHandler(IUserDbContext context)
    {
        _context = context;
    }

    public async Task<Result> Handle(LogoutCommand request, CancellationToken cancellationToken)
    {
        var user = await _context.GetUserByRefreshTokenAsync(request.RefreshToken, cancellationToken);

        if (user == null)
            return Result.Failure("Invalid refresh token");

        user.RefreshToken = null;
        user.RefreshTokenExpiryTime = DateTime.UtcNow;

        await _context.SaveChangesAsync(cancellationToken);
        return Result.Success("User logged out successfully.");
    }
}
