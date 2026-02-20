using ProductService.Application.Common.Models;
using ProductService.Domain.Entities;

namespace ProductService.Application.Common.Interfaces;

public interface IProductRepository
{
    Task<List<Product>> GetAllAsync();
    Task<PagedResult<Product>> GetAllPagedAsync(int pageNumber, int pageSize);
    Task<PagedResult<Product>> GetAllPagedWithFiltersAsync(
        int pageNumber, 
        int pageSize, 
        string? searchTerm = null,
        decimal? minPrice = null,
        decimal? maxPrice = null,
        DateTime? startDate = null,
        string? sortField = null,
        string? sortOrder = null);
    Task<Product?> GetByIdAsync(string id);
    Task<List<Product>> GetByUserIdAsync(int userId);
    Task<PagedResult<Product>> GetByUserIdPagedAsync(
        int userId, 
        int pageNumber, 
        int pageSize,
        string? searchTerm = null,
        decimal? minPrice = null,
        decimal? maxPrice = null,
        DateTime? startDate = null,
        string? sortField = null,
        string? sortOrder = null);
    Task<string> AddAsync(Product product);
    Task UpdateAsync(Product product);
    Task DeleteAsync(string id);
}
