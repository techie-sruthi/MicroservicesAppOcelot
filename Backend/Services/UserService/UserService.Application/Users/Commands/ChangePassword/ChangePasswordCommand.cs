using MediatR;
using Shared.Kernel.Results;
using Shared.Kernel.Models;

namespace UserService.Application.Users.Commands.ChangePassword;

public record ChangePasswordCommand(
    string CurrentPassword,
    string NewPassword
) : IRequest<Result<MessageResponse>>;
