using MediatR;
using Shared.Kernel.Results;

namespace UserService.Application.Users.Commands.Logout;

public record LogoutCommand(
    string RefreshToken
) : IRequest<Result>;
