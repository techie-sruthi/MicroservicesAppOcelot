using MediatR;
using Shared.Kernel.Results;
using Shared.Kernel.Models;

namespace UserService.Application.Users.Commands.ResetPassword;

public record ResetPasswordCommand(string Token, string NewPassword) : IRequest<Result<MessageResponse>>;
