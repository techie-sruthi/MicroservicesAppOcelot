using Microsoft.EntityFrameworkCore;
using UserService.Application.Common.Interfaces;
using UserService.Application.Common.Models;
using UserService.Domain.Entities;
using UserService.Application.Users.DTOs;

namespace UserService.Infrastructure.Data;

public class UserDbContext : DbContext, IUserDbContext
{
    public UserDbContext(DbContextOptions<UserDbContext> options)
        : base(options)
    {
    }

    public DbSet<User> Users { get; set; }

    IQueryable<User> IUserDbContext.Users => Users;

    public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        => base.SaveChangesAsync(cancellationToken);

    public async Task<bool> UserExistsAsync(string email, CancellationToken cancellationToken)
    {
        return await Users.AnyAsync(x => x.Email == email, cancellationToken);
    }

    public async Task AddEntityAsync<T>(T entity, CancellationToken cancellationToken)
    where T : class
    {
        await Set<T>().AddAsync(entity, cancellationToken);
    }

    public async Task<User?> GetUserByIdAsync(int id, CancellationToken cancellationToken)
    {
        return await Users.FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
    }

    public async Task<User?> GetUserByRefreshTokenAsync(string refreshToken, CancellationToken cancellationToken)
    {
        return await Users.FirstOrDefaultAsync(x => x.RefreshToken == refreshToken, cancellationToken);
    }

    public void RemoveEntity<T>(T entity) where T : class
    {
        Set<T>().Remove(entity);
    }

    public async Task<List<UserDto>> GetAllUsersAsync(CancellationToken cancellationToken)
    {
        return await Users
            .AsNoTracking()
            .Select(user => new UserDto
            {
                Id = user.Id,
                UserName = user.UserName,
                Email = user.Email,
                Role = user.Role,
                CreatedAt = user.CreatedAt
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
    
        var query = Users.AsNoTracking();

        if (!string.IsNullOrWhiteSpace(searchTerm))
        {
            var searchLower = searchTerm.ToLower();
            query = query.Where(u => 
                u.UserName.ToLower().Contains(searchLower) || 
                u.Email.ToLower().Contains(searchLower));
        }

        if (!string.IsNullOrWhiteSpace(roleFilter) && roleFilter != "all")
        {
            query = query.Where(u => u.Role == roleFilter);
        }

        var totalCount = await query.CountAsync(cancellationToken);

        query = ApplySorting(query, sortField, sortOrder);

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

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<User>()
            .HasIndex(u => u.Email)
            .IsUnique();
    }
}
