using MediatR;

namespace UserService.Application.Users.Commands.RegisterUser;

public record RegisterUserCommand(
    string UserName,
    string Email,
    string Password
) : IRequest<int>;
