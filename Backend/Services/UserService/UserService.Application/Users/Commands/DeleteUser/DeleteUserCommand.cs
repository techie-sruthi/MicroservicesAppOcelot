using MediatR;

namespace UserService.Application.Users.Commands.DeleteUser;

public record DeleteUserCommand(int Id) : IRequest<Unit>;
