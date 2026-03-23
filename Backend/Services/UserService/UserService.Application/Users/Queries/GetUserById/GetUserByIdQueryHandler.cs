using MediatR;
using Shared.Kernel.Results;
using UserService.Application.Common.Interfaces;
using UserService.Application.Users.DTOs;

namespace UserService.Application.Users.Queries.GetUserById;

public class GetUserByIdQueryHandler
    : IRequestHandler<GetUserByIdQuery, Result<UserDto>>
{
    private readonly IUserDbContext _context;

    public GetUserByIdQueryHandler(IUserDbContext context)
    {
        _context = context;
    }

    public async Task<Result<UserDto>> Handle(GetUserByIdQuery request, CancellationToken cancellationToken)
    {
        var user = await _context.GetUserByIdAsync(request.Id, cancellationToken);

        if (user == null)
            return Result<UserDto>.Failure("User not found");

        return Result<UserDto>.Success(new UserDto
        {
            Id = user.Id,
            UserName = user.UserName,
            Email = user.Email,
            Role = user.Role,
            CreatedAt = user.CreatedAt
        }, "User fetched successfully.");
    }
}
