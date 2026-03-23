using MediatR;
using Shared.Kernel.Results;
using UserService.Application.Users.DTOs;

namespace UserService.Application.Users.Queries.GetUsersByIds;

public class GetUserByIdsQuery : IRequest<Result<List<UserDto>>>
{
    public string? Ids { get; set; }
}