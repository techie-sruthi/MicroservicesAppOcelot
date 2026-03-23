using MediatR;
using Shared.Kernel.Results;

namespace UserService.Application.Users.Commands.DeleteUser;

public record DeleteUserCommand(int Id) : IRequest<Result>;
