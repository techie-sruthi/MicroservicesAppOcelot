using Microsoft.EntityFrameworkCore;
using UserService.Domain.Entities;
using UserService.Application.Common.Models;
using UserService.Application.Users.DTOs;
using UserService.Application.Common.Interfaces;

namespace UserService.Infrastructure.Persistence;

// Partial class extension to implement IUserDbContext methods
public partial class UserDbContextScaffolded
{
    // Explicitly implement Users property to return IQueryable
    IQueryable<User> IUserDbContext.Users => Users.AsQueryable();

    public async Task<bool> UserExistsAsync(string email, CancellationToken cancellationToken)
    {
        return await Users.AnyAsync(u => u.Email == email, cancellationToken);
    }

    public async Task AddEntityAsync<T>(T entity, CancellationToken cancellationToken) where T : class
    {
        await Set<T>().AddAsync(entity, cancellationToken);
    }

    public async Task<User?> GetUserByIdAsync(int id, CancellationToken cancellationToken)
    {
        return await Users.FirstOrDefaultAsync(u => u.Id == id, cancellationToken);
    }

    public async Task<User?> GetUserByRefreshTokenAsync(string refreshToken, CancellationToken cancellationToken)
    {
        return await Users.FirstOrDefaultAsync(u => u.RefreshToken == refreshToken, cancellationToken);
    }

    public void RemoveEntity<T>(T entity) where T : class
    {
        Set<T>().Remove(entity);
    }

    public async Task<List<UserDto>> GetAllUsersAsync(CancellationToken cancellationToken)
    {
        return await Users
            .Select(u => new UserDto
            {
                Id = u.Id,
                UserName = u.UserName,
                Email = u.Email,
                Role = u.Role
            })
            .ToListAsync(cancellationToken);
    }

    public async Task<PagedResult<UserDto>> GetAllUsersPagedAsync(
        int pageNumber, 
        int pageSize,
        string? searchTerm,
        string? roleFilter,
        string? sortField,
        string? sortOrder,
        CancellationToken cancellationToken)
    {
        // Start with base query
        var query = Users.AsNoTracking();

        // Apply search filter (search by username or email)
        if (!string.IsNullOrWhiteSpace(searchTerm))
        {
            var searchLower = searchTerm.ToLower();
            query = query.Where(u => 
                u.UserName.ToLower().Contains(searchLower) || 
                u.Email.ToLower().Contains(searchLower));
        }

        // Apply role filter
        if (!string.IsNullOrWhiteSpace(roleFilter) && roleFilter != "all")
        {
            query = query.Where(u => u.Role == roleFilter);
        }

        // Get total count with filters
        var totalCount = await query.CountAsync(cancellationToken);

        // Apply sorting
        query = ApplySorting(query, sortField, sortOrder);

        // Apply pagination
        var items = await query
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .Select(user => new UserDto
            {
                Id = user.Id,
                UserName = user.UserName,
                Email = user.Email,
                Role = user.Role,
                CreatedAt = user.CreatedAt
            })
            .ToListAsync(cancellationToken);

        return new PagedResult<UserDto>
        {
            Items = items,
            TotalCount = totalCount,
            PageNumber = pageNumber,
            PageSize = pageSize
        };
    }

    private IQueryable<User> ApplySorting(IQueryable<User> query, string? sortField, string? sortOrder)
    {
        // Default sort: CreatedAt descending (newest first)
        if (string.IsNullOrWhiteSpace(sortField))
        {
            return query.OrderByDescending(u => u.CreatedAt);
        }

        var isDescending = sortOrder?.ToLower() == "desc" || sortOrder == "-1";

        return sortField.ToLower() switch
        {
            "id" => isDescending ? query.OrderByDescending(u => u.Id) : query.OrderBy(u => u.Id),
            "username" => isDescending ? query.OrderByDescending(u => u.UserName) : query.OrderBy(u => u.UserName),
            "email" => isDescending ? query.OrderByDescending(u => u.Email) : query.OrderBy(u => u.Email),
            "role" => isDescending ? query.OrderByDescending(u => u.Role) : query.OrderBy(u => u.Role),
            "createdat" => isDescending ? query.OrderByDescending(u => u.CreatedAt) : query.OrderBy(u => u.CreatedAt),
            _ => query.OrderByDescending(u => u.CreatedAt) // Default
        };
    }
}
