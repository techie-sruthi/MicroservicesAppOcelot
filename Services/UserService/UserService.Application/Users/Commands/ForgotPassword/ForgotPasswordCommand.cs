using MediatR;

namespace UserService.Application.Users.Commands.ForgotPassword;

public record ForgotPasswordCommand(string Email) : IRequest<bool>;
