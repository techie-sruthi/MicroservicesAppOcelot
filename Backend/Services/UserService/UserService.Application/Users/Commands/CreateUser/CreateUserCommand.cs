using MediatR;

namespace UserService.Application.Users.Commands.CreateUser;

public record CreateUserCommand(
    string UserName,
    string Email,
    string Password,
    string Role
) : IRequest<int>;
