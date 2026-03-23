using MediatR;
using Shared.Kernel.Results;
using UserService.Application.Common.Interfaces;
using Shared.Kernel.Models;
using UserService.Application.Users.DTOs;

namespace UserService.Application.Users.Queries.GetAllUsers;

public class GetAllUsersQueryHandler
    : IRequestHandler<GetAllUsersQuery, Result<PagedResult<UserDto>>>
{
    private readonly IUserDbContext _context;

    public GetAllUsersQueryHandler(IUserDbContext context)
    {
        _context = context;
    }

    public async Task<Result<PagedResult<UserDto>>> Handle(
        GetAllUsersQuery request,
        CancellationToken cancellationToken)
    {
        request = request with
        {
            PageNumber = PaginationParams.ClampPageNumber(request.PageNumber),
            PageSize = PaginationParams.ClampPageSize(request.PageSize)
        };

        var result = await _context.GetAllUsersPagedAsync(
            request.PageNumber,
            request.PageSize,
            request.SearchTerm,
            request.RoleFilter,
            request.SortField,
            request.SortOrder,
            cancellationToken);

        return Result<PagedResult<UserDto>>.Success(result, "Users fetched successfully.");
    }
}
