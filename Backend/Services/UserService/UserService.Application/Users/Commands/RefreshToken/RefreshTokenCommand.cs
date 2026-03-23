using MediatR;
using Shared.Kernel.Results;
using UserService.Application.Users.DTOs;

namespace UserService.Application.Users.Commands.RefreshToken;

public record RefreshTokenCommand(
    string RefreshToken
) : IRequest<Result<LoginResponse>>;
