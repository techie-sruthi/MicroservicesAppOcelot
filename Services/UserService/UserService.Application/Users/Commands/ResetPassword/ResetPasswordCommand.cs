using MediatR;

namespace UserService.Application.Users.Commands.ResetPassword;

public record ResetPasswordCommand(string Token, string NewPassword) : IRequest<bool>;
