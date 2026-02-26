using MediatR;
using UserService.Application.Users.DTOs;

namespace UserService.Application.Users.Queries.GetUserById;

public record GetUserByIdQuery(int Id) : IRequest<UserDto>;
