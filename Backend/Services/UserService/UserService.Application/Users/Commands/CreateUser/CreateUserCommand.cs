using MediatR;

namespace UserService.Application.Users.Commands.CreateUser;

public record CreateUserCommand(
    string UserName,
    string Email,
    string Role
) : IRequest<int>;
