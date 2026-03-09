namespace UserService.Application.Contracts;

public interface IEmailService
{
    Task SendOtpEmailAsync(string toEmail, string otpCode);
    Task SendWelcomeEmailAsync(string toEmail, string userName);
    Task SendPasswordResetEmailAsync(string toEmail, string resetLink);
    Task SendNewUserCredentialsEmailAsync(string toEmail, string userName, string password);
}
