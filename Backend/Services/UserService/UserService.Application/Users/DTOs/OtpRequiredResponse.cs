namespace UserService.Application.Users.DTOs;

public class OtpRequiredResponse
{
    public string Email { get; set; } = string.Empty;
    public string Message { get; set; } = "OTP sent to your email";
    public bool OtpRequired { get; set; } = true;
}
