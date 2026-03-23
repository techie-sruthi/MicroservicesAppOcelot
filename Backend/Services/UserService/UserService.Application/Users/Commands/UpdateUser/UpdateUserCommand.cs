using MediatR;
using Shared.Kernel.Results;

namespace UserService.Application.Users.Commands.UpdateUser;

public record UpdateUserCommand(
    int Id,
    string UserName,
    string Email,
    string Role
) : IRequest<Result>
{
    public int RouteId { get; set; }
}
