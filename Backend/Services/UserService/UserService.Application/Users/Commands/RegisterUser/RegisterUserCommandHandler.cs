using MediatR;
using Shared.Kernel.Results;
using UserService.Application.Common.Interfaces;
using Microsoft.EntityFrameworkCore;
using UserService.Domain.Entities;

namespace UserService.Application.Users.Commands.RegisterUser;

public class RegisterUserCommandHandler
    : IRequestHandler<RegisterUserCommand, Result<int>>
{
    private readonly IUserDbContext _context;
    private readonly IPasswordHasher _passwordHasher;

    public RegisterUserCommandHandler(
        IUserDbContext context,
        IPasswordHasher passwordHasher)
    {
        _context = context;
        _passwordHasher = passwordHasher;
    }

    public async Task<Result<int>> Handle(
    RegisterUserCommand request,
    CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.UserName))
            return Result<int>.Failure("Username is required.");

        if (string.IsNullOrWhiteSpace(request.Password) || request.Password.Length < 6)
            return Result<int>.Failure("Password must be at least 6 characters long.");

        var exists = await _context.Users
            .AnyAsync(x => x.Email == request.Email, cancellationToken);

        if (exists)
            return Result<int>.Failure("User already exists");

        var user = new User
        {
            UserName = request.UserName,
            Email = request.Email,
            PasswordHash = _passwordHasher.Hash(request.Password),
            Role = "User",
            CreatedAt = DateTime.UtcNow
        };

        await _context.AddEntityAsync(user, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);

        return Result<int>.Success(user.Id, "User registered successfully.");
    }
}
