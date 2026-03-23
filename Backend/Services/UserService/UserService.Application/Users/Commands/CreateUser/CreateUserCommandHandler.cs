using System.Security.Cryptography;
using System.Text.RegularExpressions;
using MediatR;
using Shared.Kernel.Results;
using UserService.Application.Common.Interfaces;
using UserService.Application.Contracts;
using UserService.Domain.Entities;

namespace UserService.Application.Users.Commands.CreateUser;

public partial class CreateUserCommandHandler
    : IRequestHandler<CreateUserCommand, Result<int>>
{
    [GeneratedRegex(@"^[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,}$", RegexOptions.CultureInvariant)]
    private static partial Regex EmailRegex();

    private readonly IUserDbContext _context;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IEmailService _emailService;

    public CreateUserCommandHandler(
        IUserDbContext context,
        IPasswordHasher passwordHasher,
        IEmailService emailService)
    {
        _context = context;
        _passwordHasher = passwordHasher;
        _emailService = emailService;
    }

    public async Task<Result<int>> Handle(CreateUserCommand request, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.UserName))
            return Result<int>.Failure("Username is required.");

        if (string.IsNullOrWhiteSpace(request.Email) || !EmailRegex().IsMatch(request.Email))
            return Result<int>.Failure(
                "Invalid email format. Please enter a valid email");

        if (string.IsNullOrWhiteSpace(request.Role) || (request.Role != "User" && request.Role != "Admin"))
            return Result<int>.Failure("Role must be either 'User' or 'Admin'.");

        var exists = await _context.UserExistsAsync(request.Email, cancellationToken);
        if (exists)
            return Result<int>.Failure("User already exists");

        var generatedPassword = GenerateStrongPassword(16);

        var user = new User
        {
            UserName = request.UserName,
            Email = request.Email,
            PasswordHash = _passwordHasher.Hash(generatedPassword),
            Role = request.Role,
            CreatedAt = DateTime.UtcNow
        };

        await _context.AddEntityAsync(user, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);

        await _emailService.SendNewUserCredentialsEmailAsync(
            request.Email, request.UserName, generatedPassword);

        return Result<int>.Success(user.Id, "User created successfully.");
    }

    private static string GenerateStrongPassword(int length)
    {
        const string upper = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
        const string lower = "abcdefghijklmnopqrstuvwxyz";
        const string digits = "0123456789";
        const string special = "!@#$%^&*";
        const string all = upper + lower + digits + special;

        var password = new char[length];
        var bytes = new byte[length];

        RandomNumberGenerator.Fill(bytes);

        password[0] = upper[bytes[0] % upper.Length];
        password[1] = lower[bytes[1] % lower.Length];
        password[2] = digits[bytes[2] % digits.Length];
        password[3] = special[bytes[3] % special.Length];

        for (int i = 4; i < length; i++)
            password[i] = all[bytes[i] % all.Length];

        RandomNumberGenerator.Fill(bytes);
        for (int i = password.Length - 1; i > 0; i--)
        {
            int j = bytes[i] % (i + 1);
            (password[i], password[j]) = (password[j], password[i]);
        }

        return new string(password);
    }
}
