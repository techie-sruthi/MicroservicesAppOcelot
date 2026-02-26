using MediatR;
using Microsoft.EntityFrameworkCore;
using UserService.Application.Common.Interfaces;
using UserService.Application.Contracts;
using System.Security.Cryptography;

namespace UserService.Application.Users.Commands.ForgotPassword;

public class ForgotPasswordCommandHandler : IRequestHandler<ForgotPasswordCommand, bool>
{
    private readonly IUserDbContext _context;
    private readonly IEmailService _emailService;

    public ForgotPasswordCommandHandler(IUserDbContext context, IEmailService emailService)
    {
        _context = context;
        _emailService = emailService;
    }

    public async Task<bool> Handle(ForgotPasswordCommand request, CancellationToken cancellationToken)
    {
        var user = await _context.Users
            .FirstOrDefaultAsync(u => u.Email == request.Email, cancellationToken);

        if (user == null)
        {
            // Return true even if user doesn't exist (security: don't reveal which emails exist)
            return true;
        }

        // Generate secure random token
        var tokenBytes = RandomNumberGenerator.GetBytes(32);
        var token = Convert.ToBase64String(tokenBytes)
            .Replace("+", "-")
            .Replace("/", "_")
            .Replace("=", "");

        user.PasswordResetToken = token;
        user.PasswordResetTokenExpiry = DateTime.UtcNow.AddHours(1);

        await _context.SaveChangesAsync(cancellationToken);

        // Build reset link
        var resetLink = $"http://localhost:4200/reset-password?token={token}";

        Console.WriteLine($"[ForgotPassword] Reset link for {user.Email}: {resetLink}");

        // Send email with reset link
        try
        {
            await _emailService.SendPasswordResetEmailAsync(user.Email, resetLink);
            Console.WriteLine($"[ForgotPassword] Reset email sent to {user.Email}");
        }
        catch (Exception ex)
        {
            // Log the error but don't fail the request (token is still saved)
            Console.WriteLine($"[ForgotPassword] Email send failed: {ex.Message}");
            Console.WriteLine($"[ForgotPassword] Use this link manually: {resetLink}");
        }

        return true;
    }
}
