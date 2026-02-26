using MediatR;

namespace UserService.Application.Users.Commands.LoginUser;

public record LoginUserCommand(
    string Email,
    string Password
) : IRequest<object>;
