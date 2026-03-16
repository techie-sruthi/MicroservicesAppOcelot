using MediatR;
using UserService.Application.Common.Models;

namespace UserService.Application.Users.Commands.ForgotPassword;

public record ForgotPasswordCommand(string Email) : IRequest<MessageResponse>;
