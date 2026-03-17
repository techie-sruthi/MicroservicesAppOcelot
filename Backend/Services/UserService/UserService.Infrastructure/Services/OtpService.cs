using System.Security.Cryptography;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;
using UserService.Application.Contracts;
using UserService.Infrastructure.Settings;
using System.Text;

namespace UserService.Infrastructure.Services;

public class OtpService : IOtpService
{
    private readonly IMemoryCache _cache;
    private readonly OtpSettings _otpSettings;

    public OtpService(IMemoryCache cache, IOptions<OtpSettings> otpSettings)
    {
        _cache = cache;
        _otpSettings = otpSettings.Value;
    }

    public string GenerateOtp()
    {
        var sb = new StringBuilder(_otpSettings.Length);
        for (int i = 0; i < _otpSettings.Length; i++)
            sb.Append(RandomNumberGenerator.GetInt32(10));

        return sb.ToString();
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

    public string? GetOtp(string email)
    {
        var cacheKey = $"OTP_{email}";
        if (_cache.TryGetValue(cacheKey, out string? storedOtp))
            return storedOtp;
        return null;
    }

    public int GetExpirySeconds()
    {
        return _otpSettings.ExpiryMinutes * 60;
    }
}
