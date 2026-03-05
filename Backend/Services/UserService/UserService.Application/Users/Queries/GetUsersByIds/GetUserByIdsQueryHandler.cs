using MediatR;
using Microsoft.EntityFrameworkCore;
using UserService.Application.Common.Interfaces;
using UserService.Application.Users.DTOs;

namespace UserService.Application.Users.Queries.GetUserByIds
{
    public class GetUserByIdsQueryHandler
        : IRequestHandler<GetUserByIdsQuery, List<UserDto>>
    {
        private readonly IUserDbContext _context;

        public GetUserByIdsQueryHandler(IUserDbContext context)
        {
            _context = context;
        }

        public async Task<List<UserDto>> Handle(
            GetUserByIdsQuery request,
            CancellationToken cancellationToken)
        {
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

            return users;
        }
    }
}