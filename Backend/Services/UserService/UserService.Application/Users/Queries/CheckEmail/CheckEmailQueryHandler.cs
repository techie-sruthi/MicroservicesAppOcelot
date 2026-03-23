using MediatR;
using Shared.Kernel.Results;
using UserService.Application.Common.Interfaces;

namespace UserService.Application.Users.Queries.CheckEmail;

public class CheckEmailQueryHandler : IRequestHandler<CheckEmailQuery, Result<bool>>
{
    private readonly IUserDbContext _context;

    public CheckEmailQueryHandler(IUserDbContext context)
    {
        _context = context;
    }

    public async Task<Result<bool>> Handle(CheckEmailQuery request, CancellationToken cancellationToken)
    {
        var exists = await _context.UserExistsAsync(request.Email, cancellationToken);
        return Result<bool>.Success(exists, "Email check completed successfully.");
    }
}
