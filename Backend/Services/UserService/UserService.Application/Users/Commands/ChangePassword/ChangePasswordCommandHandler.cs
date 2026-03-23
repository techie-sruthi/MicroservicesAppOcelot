using MediatR;
using Shared.Kernel.Interfaces;
using Shared.Kernel.Models;
using Shared.Kernel.Results;
using UserService.Application.Common.Interfaces;

namespace UserService.Application.Users.Commands.ChangePassword;

public class ChangePasswordCommandHandler : IRequestHandler<ChangePasswordCommand, Result<MessageResponse>>
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

    public async Task<Result<MessageResponse>> Handle(ChangePasswordCommand request, CancellationToken cancellationToken)
    {
        var userId = _currentUser.GetUserId();

        var user = await _context.GetUserByIdAsync(userId, cancellationToken);

        if (user == null)
            return Result<MessageResponse>.Failure("User not found.");

        if (!_passwordHasher.Verify(request.CurrentPassword, user.PasswordHash))
            return Result<MessageResponse>.Failure("Current password is incorrect.");

        if (request.NewPassword.Length < 6)
            return Result<MessageResponse>.Failure("New password must be at least 6 characters long.");

        user.PasswordHash = _passwordHasher.Hash(request.NewPassword);
        await _context.SaveChangesAsync(cancellationToken);

        return Result<MessageResponse>.Success(new MessageResponse("Password changed successfully."), "Password changed successfully.");
    }
}
