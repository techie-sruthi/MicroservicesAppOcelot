using MediatR;
using Shared.Kernel.Results;
using UserService.Application.Users.DTOs;

namespace UserService.Application.Users.Commands.VerifyOtp;

public record VerifyOtpCommand(
    string Email,
    string Otp
) : IRequest<Result<LoginResponse>>;
