using MediatR;

namespace UserService.Application.Users.Commands.Logout;

public record LogoutCommand(
    string RefreshToken
) : IRequest;
