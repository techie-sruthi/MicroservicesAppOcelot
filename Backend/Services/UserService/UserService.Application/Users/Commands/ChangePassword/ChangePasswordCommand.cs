using MediatR;
using UserService.Application.Common.Models;

namespace UserService.Application.Users.Commands.ChangePassword;

public record ChangePasswordCommand(
    string CurrentPassword,
    string NewPassword
) : IRequest<MessageResponse>;
