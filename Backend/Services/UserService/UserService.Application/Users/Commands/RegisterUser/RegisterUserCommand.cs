using MediatR;
using Shared.Kernel.Results;

namespace UserService.Application.Users.Commands.RegisterUser;

public record RegisterUserCommand(
    string UserName,
    string Email,
    string Password
) : IRequest<Result<int>>;
