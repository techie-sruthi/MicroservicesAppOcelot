using MediatR;
using Microsoft.EntityFrameworkCore;
using Shared.Kernel.Results;
using UserService.Application.Common.Interfaces;
using UserService.Application.Users.DTOs;

namespace UserService.Application.Users.Queries.GetUsersByIds;

public class GetUserByIdsQueryHandler
    : IRequestHandler<GetUserByIdsQuery, Result<List<UserDto>>>
{
    private readonly IUserDbContext _context;

    public GetUserByIdsQueryHandler(IUserDbContext context)
    {
        _context = context;
    }

    public async Task<Result<List<UserDto>>> Handle(
        GetUserByIdsQuery request,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.Ids))
            return Result<List<UserDto>>.Success(new List<UserDto>(), "No user IDs provided.");

        var idList = request.Ids
            .Split(',')
            .Select(int.Parse)
            .ToList();

        var users = await _context.Users
            .Where(u => idList.Contains(u.Id))
            .Select(u => new UserDto
            {
                Id = u.Id,
                UserName = u.UserName
            })
            .ToListAsync(cancellationToken);

        return Result<List<UserDto>>.Success(users, "Users fetched successfully.");
    }
}