using MediatR;
using Microsoft.EntityFrameworkCore;
using UserService.Application.Common.Interfaces;

namespace UserService.Application.Users.Commands.ResetPassword;

public class ResetPasswordCommandHandler : IRequestHandler<ResetPasswordCommand, bool>
{
    private readonly IUserDbContext _context;
    private readonly IPasswordHasher _passwordHasher;

    public ResetPasswordCommandHandler(IUserDbContext context, IPasswordHasher passwordHasher)
    {
        _context = context;
        _passwordHasher = passwordHasher;
    }

    public async Task<bool> Handle(ResetPasswordCommand request, CancellationToken cancellationToken)
    {
        var user = await _context.Users
            .FirstOrDefaultAsync(u => u.PasswordResetToken == request.Token, cancellationToken);

        if (user == null)
        {
            throw new UnauthorizedAccessException("Invalid or expired reset token.");
        }

        if (user.PasswordResetTokenExpiry == null || user.PasswordResetTokenExpiry < DateTime.UtcNow)
        {
            throw new UnauthorizedAccessException("Reset token has expired. Please request a new one.");
        }

        // Hash and set the new password
        user.PasswordHash = _passwordHasher.Hash(request.NewPassword);

        // Clear reset token
        user.PasswordResetToken = null;
        user.PasswordResetTokenExpiry = null;

        await _context.SaveChangesAsync(cancellationToken);

        Console.WriteLine($"[ResetPassword] Password reset successful for user: {user.Email}");

        return true;
    }
}
