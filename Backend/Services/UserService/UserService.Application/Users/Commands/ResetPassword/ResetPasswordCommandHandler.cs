using MediatR;
using Microsoft.EntityFrameworkCore;
using Shared.Kernel.Models;
using Shared.Kernel.Results;
using UserService.Application.Common.Interfaces;

namespace UserService.Application.Users.Commands.ResetPassword;

public class ResetPasswordCommandHandler : IRequestHandler<ResetPasswordCommand, Result<MessageResponse>>
{
    private readonly IUserDbContext _context;
    private readonly IPasswordHasher _passwordHasher;

    public ResetPasswordCommandHandler(IUserDbContext context, IPasswordHasher passwordHasher)
    {
        _context = context;
        _passwordHasher = passwordHasher;
    }

    public async Task<Result<MessageResponse>> Handle(ResetPasswordCommand request, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.NewPassword) || request.NewPassword.Length < 6)
            return Result<MessageResponse>.Failure("Password must be at least 6 characters long.");

        var user = await _context.Users
            .FirstOrDefaultAsync(u => u.PasswordResetToken == request.Token, cancellationToken);

        if (user == null)
            return Result<MessageResponse>.Failure("Invalid or expired reset token.");

        if (user.PasswordResetTokenExpiry == null || user.PasswordResetTokenExpiry < DateTime.UtcNow)
            return Result<MessageResponse>.Failure("Reset token has expired. Please request a new one.");

        user.PasswordHash = _passwordHasher.Hash(request.NewPassword);

        user.PasswordResetToken = null;
        user.PasswordResetTokenExpiry = null;

        await _context.SaveChangesAsync(cancellationToken);

        return Result<MessageResponse>.Success(new MessageResponse("Password has been reset successfully."), "Password has been reset successfully.");
    }
}
