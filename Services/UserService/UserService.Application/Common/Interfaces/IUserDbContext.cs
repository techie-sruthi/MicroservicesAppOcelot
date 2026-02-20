using UserService.Application.Common.Models;
using UserService.Domain.Entities;
using UserService.Application.Users.DTOs;

namespace UserService.Application.Common.Interfaces;

public interface IUserDbContext
{
    IQueryable<User> Users { get; }
    Task<bool> UserExistsAsync(string email, CancellationToken cancellationToken);
    Task AddEntityAsync<T>(T entity, CancellationToken cancellationToken) where T : class;
    Task<int> SaveChangesAsync(CancellationToken cancellationToken);
    Task<User?> GetUserByIdAsync(int id, CancellationToken cancellationToken);
    Task<User?> GetUserByRefreshTokenAsync(string refreshToken, CancellationToken cancellationToken);
    void RemoveEntity<T>(T entity) where T : class;
    Task<List<UserDto>> GetAllUsersAsync(CancellationToken cancellationToken);
    Task<PagedResult<UserDto>> GetAllUsersPagedAsync(
     int pageNumber,
     int pageSize,
     string? searchTerm,
     string? roleFilter,
     string? sortField,
     string? sortOrder,
     CancellationToken cancellationToken);
}
