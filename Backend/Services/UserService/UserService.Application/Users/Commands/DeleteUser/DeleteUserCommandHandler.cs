using MediatR;
using UserService.Application.Common.Interfaces;

namespace UserService.Application.Users.Commands.DeleteUser;

public class DeleteUserCommandHandler
    : IRequestHandler<DeleteUserCommand, Unit>
{
    private readonly IUserDbContext _context;

    public DeleteUserCommandHandler(IUserDbContext context)
    {
        _context = context;
    }

    public async Task<Unit> Handle(DeleteUserCommand request, CancellationToken cancellationToken)
    {
        var user = await _context.GetUserByIdAsync(request.Id, cancellationToken);

        if (user == null)
            throw new Exception("User not found");

        _context.RemoveEntity(user);
        await _context.SaveChangesAsync(cancellationToken);

        return Unit.Value;
    }
}
