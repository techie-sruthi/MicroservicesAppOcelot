using MediatR;
using Shared.Kernel.Results;
using UserService.Application.Common.Interfaces;

namespace UserService.Application.Users.Commands.DeleteUser;

public class DeleteUserCommandHandler
    : IRequestHandler<DeleteUserCommand, Result>
{
    private readonly IUserDbContext _context;

    public DeleteUserCommandHandler(IUserDbContext context)
    {
        _context = context;
    }

    public async Task<Result> Handle(DeleteUserCommand request, CancellationToken cancellationToken)
    {
        var user = await _context.GetUserByIdAsync(request.Id, cancellationToken);

        if (user == null)
            return Result.Failure("User not found");

        _context.RemoveEntity(user);
        await _context.SaveChangesAsync(cancellationToken);

        return Result.Success("User deleted successfully.");
    }
}
