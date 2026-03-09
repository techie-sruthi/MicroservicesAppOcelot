using System.Net;
using System.Net.Mail;
using Microsoft.Extensions.Options;
using UserService.Application.Contracts;
using UserService.Infrastructure.Settings;

namespace UserService.Infrastructure.Services;

public class EmailService : IEmailService
{
    private readonly EmailSettings _emailSettings;

    public EmailService(IOptions<EmailSettings> emailSettings)
    {
        _emailSettings = emailSettings.Value;
    }

    public async Task SendOtpEmailAsync(string toEmail, string otpCode)
    {
        var subject = "Your OTP Code - ProductUser App";
        var body = $@"
            <html>
            <body style='font-family: Arial, sans-serif;'>
                <div style='max-width: 600px; margin: 0 auto; padding: 20px;'>
                    <h2 style='color: #4F46E5;'>Login Verification</h2>
                    <p>Your One-Time Password (OTP) is:</p>
                    <div style='background-color: #F3F4F6; padding: 20px; border-radius: 8px; text-align: center; margin: 20px 0;'>
                        <h1 style='color: #4F46E5; font-size: 32px; letter-spacing: 8px; margin: 0;'>{otpCode}</h1>
                    </div>
                    <p>This code will expire in <strong>5 minutes</strong>.</p>
                    <p>If you didn't request this code, please ignore this email.</p>
                    <hr style='border: 1px solid #E5E7EB; margin: 20px 0;'>
                    <p style='color: #6B7280; font-size: 12px;'>This is an automated message, please do not reply.</p>
                </div>
            </body>
            </html>
        ";

        await SendEmailAsync(toEmail, subject, body);
    }

    public async Task SendWelcomeEmailAsync(string toEmail, string userName)
    {
        var subject = "Welcome to ProductUser App!";
        var body = $@"
            <html>
            <body style='font-family: Arial, sans-serif;'>
                <div style='max-width: 600px; margin: 0 auto; padding: 20px;'>
                    <h2 style='color: #4F46E5;'>Welcome, {userName}!</h2>
                    <p>Thank you for registering with ProductUser App.</p>
                    <p>You can now login and start managing your products.</p>
                </div>
            </body>
            </html>
        ";

        await SendEmailAsync(toEmail, subject, body);
    }

    public async Task SendPasswordResetEmailAsync(string toEmail, string resetLink)
    {
        var subject = "Password Reset - ProductUser App";
        var body = $@"
            <html>
            <body style='font-family: Arial, sans-serif;'>
                <div style='max-width: 600px; margin: 0 auto; padding: 20px;'>
                    <h2 style='color: #4F46E5;'>Password Reset Request</h2>
                    <p>We received a request to reset your password.</p>
                    <p>Click the button below to set a new password:</p>
                    <div style='text-align: center; margin: 30px 0;'>
                        <a href='{resetLink}' style='background-color: #4F46E5; color: white; padding: 14px 28px; text-decoration: none; border-radius: 8px; font-size: 16px; font-weight: 600;'>
                            Reset Password
                        </a>
                    </div>
                    <p>Or copy and paste this link in your browser:</p>
                    <p style='word-break: break-all; color: #4F46E5;'>{resetLink}</p>
                    <p>This link will expire in <strong>1 hour</strong>.</p>
                    <p>If you didn't request a password reset, please ignore this email.</p>
                    <hr style='border: 1px solid #E5E7EB; margin: 20px 0;'>
                    <p style='color: #6B7280; font-size: 12px;'>This is an automated message, please do not reply.</p>
                </div>
            </body>
            </html>
        ";

        await SendEmailAsync(toEmail, subject, body);
    }

    public async Task SendNewUserCredentialsEmailAsync(string toEmail, string userName, string password)
    {
        var subject = "Your Account Has Been Created - ProductUser App";
        var body = $@"
            <html>
            <body style='font-family: Arial, sans-serif;'>
                <div style='max-width: 600px; margin: 0 auto; padding: 20px;'>
                    <h2 style='color: #4F46E5;'>Welcome, {userName}!</h2>
                    <p>An account has been created for you on <strong>ProductUser App</strong>.</p>
                    <p>Here are your login credentials:</p>
                    <div style='background-color: #F3F4F6; padding: 20px; border-radius: 8px; margin: 20px 0;'>
                        <p style='margin: 8px 0;'><strong>Email:</strong> {toEmail}</p>
                        <p style='margin: 8px 0;'><strong>Password:</strong> <code style='background-color: #E5E7EB; padding: 4px 8px; border-radius: 4px; font-size: 14px;'>{password}</code></p>
                    </div>
                    <p style='color: #DC2626; font-weight: 600;'>⚠️ Please change your password after your first login for security.</p>
                    <hr style='border: 1px solid #E5E7EB; margin: 20px 0;'>
                    <p style='color: #6B7280; font-size: 12px;'>This is an automated message, please do not reply.</p>
                </div>
            </body>
            </html>
        ";

        await SendEmailAsync(toEmail, subject, body);
    }

    private async Task SendEmailAsync(string toEmail, string subject, string body)
    {
        using var client = new SmtpClient(_emailSettings.SmtpServer, _emailSettings.SmtpPort)
        {
            EnableSsl = _emailSettings.EnableSSL,
            Credentials = new NetworkCredential(_emailSettings.SenderEmail, _emailSettings.Password)
        };

        var mailMessage = new MailMessage
        {
            From = new MailAddress(_emailSettings.SenderEmail, _emailSettings.SenderName),
            Subject = subject,
            Body = body,
            IsBodyHtml = true
        };

        mailMessage.To.Add(toEmail);

        await client.SendMailAsync(mailMessage);
    }
}
