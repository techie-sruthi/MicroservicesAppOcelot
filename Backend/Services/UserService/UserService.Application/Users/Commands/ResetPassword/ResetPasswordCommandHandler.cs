using MediatR;
using Microsoft.EntityFrameworkCore;
using UserService.Application.Common.Interfaces;
using UserService.Application.Common.Models;

namespace UserService.Application.Users.Commands.ResetPassword;

public class ResetPasswordCommandHandler : IRequestHandler<ResetPasswordCommand, MessageResponse>
{
    private readonly IUserDbContext _context;
    private readonly IPasswordHasher _passwordHasher;

    public ResetPasswordCommandHandler(IUserDbContext context, IPasswordHasher passwordHasher)
    {
        _context = context;
        _passwordHasher = passwordHasher;
    }

    public async Task<MessageResponse> Handle(ResetPasswordCommand request, CancellationToken cancellationToken)
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

        user.PasswordHash = _passwordHasher.Hash(request.NewPassword);

        user.PasswordResetToken = null;
        user.PasswordResetTokenExpiry = null;

        await _context.SaveChangesAsync(cancellationToken);

        return new MessageResponse("Password has been reset successfully.");
    }
}
