using MediatR;
using Shared.Kernel.Results;

namespace UserService.Application.Users.Commands.CreateUser;

public record CreateUserCommand(
    string UserName,
    string Email,
    string Role
) : IRequest<Result<int>>;
