using MediatR;
using Shared.Kernel.Results;

namespace UserService.Application.Users.Commands.LoginUser;

public record LoginUserCommand(
    string Email,
    string Password
) : IRequest<Result<object>>;
