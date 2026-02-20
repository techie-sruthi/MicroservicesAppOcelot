namespace UserService.Application.Contracts;

public interface IOtpService
{
    string GenerateOtp();
    void StoreOtp(string email, string otp);
    bool ValidateOtp(string email, string otp);
    void ClearOtp(string email);

    // Dev / testing helper: return stored OTP if present (nullable)
    string? GetOtp(string email);

    // Return OTP expiry in seconds (for client consumption)
    int GetExpirySeconds();
}
