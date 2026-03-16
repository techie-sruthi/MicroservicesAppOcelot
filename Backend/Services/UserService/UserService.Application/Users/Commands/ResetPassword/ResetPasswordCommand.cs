using MediatR;
using UserService.Application.Common.Models;

namespace UserService.Application.Users.Commands.ResetPassword;

public record ResetPasswordCommand(string Token, string NewPassword) : IRequest<MessageResponse>;
