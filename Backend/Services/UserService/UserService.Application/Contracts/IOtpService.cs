namespace UserService.Application.Contracts;

public interface IOtpService
{
    string GenerateOtp();
    void StoreOtp(string email, string otp);
    bool ValidateOtp(string email, string otp);
    void ClearOtp(string email);

    string? GetOtp(string email);
    int GetExpirySeconds();
}
