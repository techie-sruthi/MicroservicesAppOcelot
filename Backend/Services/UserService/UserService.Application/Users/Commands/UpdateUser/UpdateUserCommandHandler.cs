using System.Text.RegularExpressions;
using MediatR;
using Shared.Kernel.Results;
using UserService.Application.Common.Interfaces;

namespace UserService.Application.Users.Commands.UpdateUser;

public partial class UpdateUserCommandHandler
    : IRequestHandler<UpdateUserCommand, Result>
{
    [GeneratedRegex(@"^[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,}$", RegexOptions.CultureInvariant)]
    private static partial Regex EmailRegex();

    private readonly IUserDbContext _context;

    public UpdateUserCommandHandler(IUserDbContext context)
    {
        _context = context;
    }

    public async Task<Result> Handle(UpdateUserCommand request, CancellationToken cancellationToken)
    {
        if (request.RouteId != request.Id)
            return Result.Failure("Route ID does not match the command ID.");

        if (string.IsNullOrWhiteSpace(request.UserName))
            return Result.Failure("Username is required.");

      if (string.IsNullOrWhiteSpace(request.Email) || !EmailRegex().IsMatch(request.Email))
            return Result.Failure("Invalid email format. Please enter a valid email address.");

        if (string.IsNullOrWhiteSpace(request.Role) || (request.Role != "User" && request.Role != "Admin"))
            return Result.Failure("Role must be either 'User' or 'Admin'.");

        var user = await _context.GetUserByIdAsync(request.Id, cancellationToken);

        if (user == null)
            return Result.Failure("User not found");

        user.UserName = request.UserName;
        user.Email = request.Email;
        user.Role = request.Role;

        await _context.SaveChangesAsync(cancellationToken);

        return Result.Success("User updated successfully.");
    }
}
