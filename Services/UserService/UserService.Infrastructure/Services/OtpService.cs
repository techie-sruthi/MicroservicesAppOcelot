using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;
using UserService.Application.Contracts;
using UserService.Infrastructure.Settings;

namespace UserService.Infrastructure.Services;

public class OtpService : IOtpService
{
    private readonly IMemoryCache _cache;
    private readonly OtpSettings _otpSettings;
    private readonly Random _random = new();

    public OtpService(IMemoryCache cache, IOptions<OtpSettings> otpSettings)
    {
        _cache = cache;
        _otpSettings = otpSettings.Value;
    }

    public string GenerateOtp()
    {
        var otp = "";
        for (int i = 0; i < _otpSettings.Length; i++)
        {
            otp += _random.Next(0, 10).ToString();
        }
        return otp;
    }

    public void StoreOtp(string email, string otp)
    {
        var cacheKey = $"OTP_{email}";
        var cacheOptions = new MemoryCacheEntryOptions
        {
            AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(_otpSettings.ExpiryMinutes)
        };
        _cache.Set(cacheKey, otp, cacheOptions);
    }

    public bool ValidateOtp(string email, string otp)
    {
        var cacheKey = $"OTP_{email}";
        if (_cache.TryGetValue(cacheKey, out string? storedOtp))
        {
            return storedOtp == otp;
        }
        return false;
    }

    public void ClearOtp(string email)
    {
        var cacheKey = $"OTP_{email}";
        _cache.Remove(cacheKey);
    }

    // Dev/testing helper: retrieve stored OTP if present
    public string? GetOtp(string email)
    {
        var cacheKey = $"OTP_{email}";
        if (_cache.TryGetValue(cacheKey, out string? storedOtp))
            return storedOtp;
        return null;
    }

    // Return expiry in seconds for client consumption
    public int GetExpirySeconds()
    {
        return _otpSettings.ExpiryMinutes * 60;
    }
}
