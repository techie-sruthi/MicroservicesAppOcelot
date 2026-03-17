using MediatR;
using UserService.Application.Common.Interfaces;
using UserService.Application.Common.Models;

namespace UserService.Application.Users.Commands.ChangePassword;

public class ChangePasswordCommandHandler : IRequestHandler<ChangePasswordCommand, MessageResponse>
{
    private readonly IUserDbContext _context;
    private readonly IPasswordHasher _passwordHasher;
    private readonly ICurrentUserService _currentUser;

    public ChangePasswordCommandHandler(IUserDbContext context, IPasswordHasher passwordHasher, ICurrentUserService currentUser)
    {
        _context = context;
        _passwordHasher = passwordHasher;
        _currentUser = currentUser;
    }

    public async Task<MessageResponse> Handle(ChangePasswordCommand request, CancellationToken cancellationToken)
    {
        var userId = _currentUser.GetUserId();

        var user = await _context.GetUserByIdAsync(userId, cancellationToken);

        if (user == null)
            throw new KeyNotFoundException("User not found.");

        if (!_passwordHasher.Verify(request.CurrentPassword, user.PasswordHash))
            throw new UnauthorizedAccessException("Current password is incorrect.");

        if (request.NewPassword.Length < 6)
            throw new ArgumentException("New password must be at least 6 characters long.");

        user.PasswordHash = _passwordHasher.Hash(request.NewPassword);
        await _context.SaveChangesAsync(cancellationToken);

        return new MessageResponse("Password changed successfully.");
    }
}
