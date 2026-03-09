using MediatR;
using UserService.Application.Common.Interfaces;

namespace UserService.Application.Users.Commands.Logout;

public class LogoutCommandHandler : IRequestHandler<LogoutCommand>
{
    private readonly IUserDbContext _context;

    public LogoutCommandHandler(IUserDbContext context)
    {
        _context = context;
    }

    public async Task Handle(LogoutCommand request, CancellationToken cancellationToken)
    {
        var user = await _context.GetUserByRefreshTokenAsync(request.RefreshToken, cancellationToken);

        if (user == null)
            throw new UnauthorizedAccessException("Invalid refresh token");

        user.RefreshToken = null;
        user.RefreshTokenExpiryTime = DateTime.UtcNow;

        await _context.SaveChangesAsync(cancellationToken);
    }
}
