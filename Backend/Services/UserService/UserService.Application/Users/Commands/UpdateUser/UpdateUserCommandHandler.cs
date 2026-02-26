using MediatR;
using UserService.Application.Common.Interfaces;

namespace UserService.Application.Users.Commands.UpdateUser;

public class UpdateUserCommandHandler
    : IRequestHandler<UpdateUserCommand, Unit>
{
    private readonly IUserDbContext _context;

    public UpdateUserCommandHandler(IUserDbContext context)
    {
        _context = context;
    }

    public async Task<Unit> Handle(UpdateUserCommand request, CancellationToken cancellationToken)
    {
        var user = await _context.GetUserByIdAsync(request.Id, cancellationToken);

        if (user == null)
            throw new Exception("User not found");

        user.UserName = request.UserName;
        user.Email = request.Email;
        user.Role = request.Role;

        await _context.SaveChangesAsync(cancellationToken);

        return Unit.Value;
    }
}
