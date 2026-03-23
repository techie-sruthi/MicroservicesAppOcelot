using MediatR;
using Shared.Kernel.Results;
using Shared.Kernel.Models;
using UserService.Application.Users.DTOs;

namespace UserService.Application.Users.Queries.GetAllUsers;

public record GetAllUsersQuery(
    int PageNumber = 1, 
    int PageSize = 10,
    string? SearchTerm = null,
    string? RoleFilter = null,
    string? SortField = null,
    string? SortOrder = null
) : IRequest<Result<PagedResult<UserDto>>>;
