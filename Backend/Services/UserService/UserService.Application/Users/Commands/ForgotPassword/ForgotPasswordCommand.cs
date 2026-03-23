using MediatR;
using Shared.Kernel.Results;
using Shared.Kernel.Models;

namespace UserService.Application.Users.Commands.ForgotPassword;

public record ForgotPasswordCommand(string Email) : IRequest<Result<MessageResponse>>;
