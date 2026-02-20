using MediatR;
using UserService.Application.Common.Interfaces;

namespace UserService.Application.Users.Queries.CheckEmail;

public class CheckEmailQueryHandler : IRequestHandler<CheckEmailQuery, bool>
{
    private readonly IUserDbContext _context;

    public CheckEmailQueryHandler(IUserDbContext context)
    {
        _context = context;
    }

    public async Task<bool> Handle(CheckEmailQuery request, CancellationToken cancellationToken)
    {
        // Check if email already exists (case-insensitive)
        var exists = await _context.UserExistsAsync(request.Email, cancellationToken);
        
        return exists; // Returns true if email is already taken
    }
}
