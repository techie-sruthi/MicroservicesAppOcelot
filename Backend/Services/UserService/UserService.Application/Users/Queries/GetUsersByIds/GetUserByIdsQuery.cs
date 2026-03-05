using MediatR;
using UserService.Application.Users.DTOs;

public class GetUserByIdsQuery : IRequest<List<UserDto>>
{
    public string? Ids { get; set; }
}