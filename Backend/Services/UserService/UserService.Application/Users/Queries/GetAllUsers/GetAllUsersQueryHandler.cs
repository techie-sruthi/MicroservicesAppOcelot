using MediatR;
using UserService.Application.Common.Interfaces;
using UserService.Application.Common.Models;
using UserService.Application.Users.DTOs;

namespace UserService.Application.Users.Queries.GetAllUsers;

public class GetAllUsersQueryHandler
    : IRequestHandler<GetAllUsersQuery, PagedResult<UserDto>>
{
    private readonly IUserDbContext _context;

    public GetAllUsersQueryHandler(IUserDbContext context)
    {
        _context = context;
    }

    public async Task<PagedResult<UserDto>> Handle(
        GetAllUsersQuery request,
        CancellationToken cancellationToken)
    {
        return await _context.GetAllUsersPagedAsync(
            request.PageNumber,
            request.PageSize,
            request.SearchTerm,
            request.RoleFilter,
            request.SortField,
            request.SortOrder,
            cancellationToken);
    }
}
