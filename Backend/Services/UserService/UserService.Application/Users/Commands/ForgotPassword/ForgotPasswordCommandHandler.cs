using MediatR;
using Microsoft.EntityFrameworkCore;
using UserService.Application.Common.Interfaces;
using UserService.Application.Common.Models;
using UserService.Application.Contracts;
using System.Security.Cryptography;

namespace UserService.Application.Users.Commands.ForgotPassword;

public class ForgotPasswordCommandHandler : IRequestHandler<ForgotPasswordCommand, MessageResponse>
{
    private readonly IUserDbContext _context;
    private readonly IEmailService _emailService;

    public ForgotPasswordCommandHandler(IUserDbContext context, IEmailService emailService)
    {
        _context = context;
        _emailService = emailService;
    }

    public async Task<MessageResponse> Handle(ForgotPasswordCommand request, CancellationToken cancellationToken)
    {
        var user = await _context.Users
            .FirstOrDefaultAsync(u => u.Email == request.Email, cancellationToken);

        if (user == null)
        {
            // Return success even if user doesn't exist (security: don't reveal which emails exist)
            return new MessageResponse("If your email exists, you will receive a password reset link.");
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

        // Send email with reset link
        try
        {
            await _emailService.SendPasswordResetEmailAsync(user.Email, resetLink);
        }
        catch (Exception ex)
        {
            // Log the error but don't expose the token or email
            Console.WriteLine($"[ForgotPassword] Email send failed: {ex.Message}");
        }

        return new MessageResponse("If your email exists, you will receive a password reset link.");
    }
}
