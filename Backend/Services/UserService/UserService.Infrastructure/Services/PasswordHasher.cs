using System.Security.Cryptography;
using System.Text;
using UserService.Application.Common.Interfaces;

namespace UserService.Infrastructure.Services;

public class PasswordHasher : IPasswordHasher
{
    public string Hash(string password)
    {
        using var sha = SHA256.Create();
        var bytes = Encoding.UTF8.GetBytes(password);
        var hash = sha.ComputeHash(bytes);
        return Convert.ToBase64String(hash);
    }

    public bool Verify(string password, string passwordHash)
    {
        return Hash(password) == passwordHash;
    }
}
