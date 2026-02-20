using Microsoft.Extensions.Options;
using MongoDB.Driver;
using ProductService.Application.Common.Interfaces;
using ProductService.Application.Common.Models;
using ProductService.Domain.Entities;
using ProductService.Infrastructure.Data;

namespace ProductService.Infrastructure.Repositories;

public class ProductRepository : IProductRepository
{
    private readonly IMongoCollection<Product> _products;

    public ProductRepository(IOptions<MongoDbSettings> settings)
    {
        var client = new MongoClient(settings.Value.ConnectionString);
        var database = client.GetDatabase(settings.Value.DatabaseName);
        _products = database.GetCollection<Product>("Products");
    }

    public async Task<List<Product>> GetAllAsync()
    {
        return await _products.Find(_ => true).ToListAsync();
    }

    public async Task<PagedResult<Product>> GetAllPagedAsync(int pageNumber, int pageSize)
    {
        var totalCount = await _products.CountDocumentsAsync(_ => true);

        var items = await _products.Find(_ => true)
            .Skip((pageNumber - 1) * pageSize)
            .Limit(pageSize)
            .ToListAsync();

        return new PagedResult<Product>
        {
            Items = items,
            TotalCount = (int)totalCount,
            PageNumber = pageNumber,
            PageSize = pageSize
        };
    }

    public async Task<PagedResult<Product>> GetAllPagedWithFiltersAsync(
        int pageNumber, 
        int pageSize, 
        string? searchTerm = null,
        decimal? minPrice = null,
        decimal? maxPrice = null,
        DateTime? startDate = null,
        string? sortField = null,
        string? sortOrder = null)
    {
        // Build filter
        var filterBuilder = Builders<Product>.Filter;
        var filters = new List<FilterDefinition<Product>>();

        // Search by name or description
        if (!string.IsNullOrWhiteSpace(searchTerm))
        {
            var searchFilter = filterBuilder.Or(
                filterBuilder.Regex(p => p.Name, new MongoDB.Bson.BsonRegularExpression(searchTerm, "i")),
                filterBuilder.Regex(p => p.Description, new MongoDB.Bson.BsonRegularExpression(searchTerm, "i"))
            );
            filters.Add(searchFilter);
        }

        // Filter by price range
        if (minPrice.HasValue)
        {
            filters.Add(filterBuilder.Gte(p => p.Price, minPrice.Value));
        }
        if (maxPrice.HasValue)
        {
            filters.Add(filterBuilder.Lte(p => p.Price, maxPrice.Value));
        }

        // Filter by manufacture date
        if (startDate.HasValue)
        {
            filters.Add(filterBuilder.Gte(p => p.DateOfManufacture, startDate.Value));
        }

        // Combine all filters
        var combinedFilter = filters.Count > 0 
            ? filterBuilder.And(filters) 
            : filterBuilder.Empty;

        // Get total count with filters
        var totalCount = await _products.CountDocumentsAsync(combinedFilter);

        // Configure find options with case-insensitive collation
        var findOptions = new FindOptions<Product>
        {
            Collation = new Collation("en", strength: CollationStrength.Secondary),
            Skip = (pageNumber - 1) * pageSize,
            Limit = pageSize
        };

        // Apply sorting
        findOptions.Sort = GetSortDefinition(sortField, sortOrder);

        // Execute query
        var items = await _products.FindAsync(combinedFilter, findOptions);
        var itemsList = await items.ToListAsync();

        return new PagedResult<Product>
        {
            Items = itemsList,
            TotalCount = (int)totalCount,
            PageNumber = pageNumber,
            PageSize = pageSize
        };
    }

    public async Task<PagedResult<Product>> GetByUserIdPagedAsync(
        int userId, 
        int pageNumber, 
        int pageSize,
        string? searchTerm = null,
        decimal? minPrice = null,
        decimal? maxPrice = null,
        DateTime? startDate = null,
        string? sortField = null,
        string? sortOrder = null)
    {
        // Build filters
        var filterBuilder = Builders<Product>.Filter;
        var filters = new List<FilterDefinition<Product>>
        {
            filterBuilder.Eq(p => p.CreatedByUserId, userId)
        };

        // Search by name or description
        if (!string.IsNullOrWhiteSpace(searchTerm))
        {
            var searchFilter = filterBuilder.Or(
                filterBuilder.Regex(p => p.Name, new MongoDB.Bson.BsonRegularExpression(searchTerm, "i")),
                filterBuilder.Regex(p => p.Description, new MongoDB.Bson.BsonRegularExpression(searchTerm, "i"))
            );
            filters.Add(searchFilter);
        }

        // Filter by price range
        if (minPrice.HasValue)
        {
            filters.Add(filterBuilder.Gte(p => p.Price, minPrice.Value));
        }
        if (maxPrice.HasValue)
        {
            filters.Add(filterBuilder.Lte(p => p.Price, maxPrice.Value));
        }

        // Filter by manufacture date
        if (startDate.HasValue)
        {
            filters.Add(filterBuilder.Gte(p => p.DateOfManufacture, startDate.Value));
        }

        var combinedFilter = filterBuilder.And(filters);
        var totalCount = await _products.CountDocumentsAsync(combinedFilter);

        // Configure find options with case-insensitive collation
        var findOptions = new FindOptions<Product>
        {
            Collation = new Collation("en", strength: CollationStrength.Secondary),
            Skip = (pageNumber - 1) * pageSize,
            Limit = pageSize
        };

        // Apply sorting
        findOptions.Sort = GetSortDefinition(sortField, sortOrder);

        // Execute query
        var items = await _products.FindAsync(combinedFilter, findOptions);
        var itemsList = await items.ToListAsync();

        return new PagedResult<Product>
        {
            Items = itemsList,
            TotalCount = (int)totalCount,
            PageNumber = pageNumber,
            PageSize = pageSize
        };
    }

    public async Task<Product?> GetByIdAsync(string id)
    {
        return await _products.Find(p => p.Id == id).FirstOrDefaultAsync();
    }

    public async Task<List<Product>> GetByUserIdAsync(int userId)
    {
        return await _products.Find(p => p.CreatedByUserId == userId).ToListAsync();
    }

    private SortDefinition<Product> GetSortDefinition(string? sortField, string? sortOrder)
    {
        var sortBuilder = Builders<Product>.Sort;

        // Default sort: CreatedAt descending (newest first)
        if (string.IsNullOrWhiteSpace(sortField))
        {
            return sortBuilder.Descending(p => p.CreatedAt);
        }

        var isDescending = sortOrder?.ToLower() == "desc" || sortOrder == "-1";

        return sortField.ToLower() switch
        {
            "name" => isDescending ? sortBuilder.Descending(p => p.Name) : sortBuilder.Ascending(p => p.Name),
            "price" => isDescending ? sortBuilder.Descending(p => p.Price) : sortBuilder.Ascending(p => p.Price),
            "dateofmanufacture" => isDescending ? sortBuilder.Descending(p => p.DateOfManufacture) : sortBuilder.Ascending(p => p.DateOfManufacture),
            "createdat" => isDescending ? sortBuilder.Descending(p => p.CreatedAt) : sortBuilder.Ascending(p => p.CreatedAt),
            _ => sortBuilder.Descending(p => p.CreatedAt) // Default
        };
    }

    public async Task<string> AddAsync(Product product)
    {
        await _products.InsertOneAsync(product);
        return product.Id;
    }

    public async Task UpdateAsync(Product product)
    {
        await _products.ReplaceOneAsync(p => p.Id == product.Id, product);
    }

    public async Task DeleteAsync(string id)
    {
        await _products.DeleteOneAsync(p => p.Id == id);
    }
}
