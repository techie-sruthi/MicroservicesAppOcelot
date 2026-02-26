using MediatR;
using UserService.Application.Common.Interfaces;
using Microsoft.EntityFrameworkCore;
using UserService.Domain.Entities;

namespace UserService.Application.Users.Commands.RegisterUser;

public class RegisterUserCommandHandler
    : IRequestHandler<RegisterUserCommand, int>
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

    public async Task<int> Handle(
    RegisterUserCommand request,
    CancellationToken cancellationToken)
    {
        var exists = await _context.Users
            .AnyAsync(x => x.Email == request.Email, cancellationToken);

        if (exists)
            throw new Exception("User already exists");

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

        return user.Id;
    }
}
