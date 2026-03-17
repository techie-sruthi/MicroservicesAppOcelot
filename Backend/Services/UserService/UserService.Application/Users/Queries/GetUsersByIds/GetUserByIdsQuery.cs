using MediatR;
using UserService.Application.Users.DTOs;

namespace UserService.Application.Users.Queries.GetUsersByIds;

public class GetUserByIdsQuery : IRequest<List<UserDto>>
{
    public string? Ids { get; set; }
}