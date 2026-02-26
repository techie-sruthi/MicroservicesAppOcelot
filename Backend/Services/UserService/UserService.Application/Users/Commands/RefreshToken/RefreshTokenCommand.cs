using MediatR;
using UserService.Application.Users.DTOs;

namespace UserService.Application.Users.Commands.RefreshToken;

public record RefreshTokenCommand(
    string RefreshToken
) : IRequest<LoginResponse>;
