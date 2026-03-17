using Microsoft.Extensions.Options;
using MongoDB.Driver;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.Serializers;
using ProductService.Application.Common.Interfaces;
using ProductService.Application.Common.Models;
using ProductService.Domain.Entities;
using ProductService.Infrastructure.Data;
using ProductService.Application.Products.Queries.GetAllProducts;

namespace ProductService.Infrastructure.Repositories;

public class ProductRepository : IProductRepository
{
    private readonly IMongoCollection<Product> _products;

    public ProductRepository(
        IMongoClient mongoClient,
        IOptions<MongoDbSettings> settings)
    {
        var mongoSettings = settings.Value;

        var database = mongoClient.GetDatabase(mongoSettings.DatabaseName);
        _products = database.GetCollection<Product>("Products");
        CreateIndexes();

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
    GetAllProductsQuery query,
    CancellationToken cancellationToken)
    {
        var filterBuilder = Builders<Product>.Filter;
        var filters = new List<FilterDefinition<Product>>();

        if (!string.IsNullOrWhiteSpace(query.SearchTerm))
        {
            var regex = new BsonRegularExpression(query.SearchTerm, "i");

            filters.Add(filterBuilder.Or(
                filterBuilder.Regex(p => p.Name, regex),
                filterBuilder.Regex(p => p.Description, regex)
            ));
        }

        if (query.MinPrice.HasValue)
            filters.Add(filterBuilder.Gte(p => p.Price, query.MinPrice.Value));

        if (query.MaxPrice.HasValue)
            filters.Add(filterBuilder.Lte(p => p.Price, query.MaxPrice.Value));

        if (query.StartDate.HasValue)
            filters.Add(filterBuilder.Gte(p => p.DateOfManufacture, query.StartDate.Value));

        var combinedFilter = filters.Count > 0
            ? filterBuilder.And(filters)
            : filterBuilder.Empty;

        var totalCount = await _products.CountDocumentsAsync(combinedFilter, cancellationToken: cancellationToken);

        var findOptions = new FindOptions<Product>
        {
            Collation = new Collation("en", strength: CollationStrength.Secondary),
            Skip = (query.PageNumber - 1) * query.PageSize,
            Limit = query.PageSize,
            Sort = GetSortDefinition(query.SortField, query.SortOrder)
        };

        var cursor = await _products.FindAsync(combinedFilter, findOptions, cancellationToken);
        var items = await cursor.ToListAsync(cancellationToken);

        return new PagedResult<Product>
        {
            Items = items,
            TotalCount = (int)totalCount,
            PageNumber = query.PageNumber,
            PageSize = query.PageSize
        };
    }

    public async Task<PagedResult<Product>> GetByUserIdPagedAsync(UserProductFilter filter)
    {
        var filterBuilder = Builders<Product>.Filter;

        var filters = new List<FilterDefinition<Product>>
        {
            filterBuilder.Eq(p => p.CreatedByUserId, filter.UserId)
        };

        if (!string.IsNullOrWhiteSpace(filter.SearchTerm))
        {
            var regex = new BsonRegularExpression(filter.SearchTerm, "i");

            filters.Add(filterBuilder.Or(
                filterBuilder.Regex(p => p.Name, regex),
                filterBuilder.Regex(p => p.Description, regex)
            ));
        }

        if (filter.MinPrice.HasValue)
            filters.Add(filterBuilder.Gte(p => p.Price, filter.MinPrice.Value));

        if (filter.MaxPrice.HasValue)
            filters.Add(filterBuilder.Lte(p => p.Price, filter.MaxPrice.Value));

        if (filter.StartDate.HasValue)
            filters.Add(filterBuilder.Gte(p => p.DateOfManufacture, filter.StartDate.Value));

        var combinedFilter = filterBuilder.And(filters);

        var totalCount = await _products.CountDocumentsAsync(combinedFilter);

        var findOptions = new FindOptions<Product>
        {
            Collation = new Collation("en", strength: CollationStrength.Secondary),
            Skip = (filter.PageNumber - 1) * filter.PageSize,
            Limit = filter.PageSize,
            Sort = GetSortDefinition(filter.SortField, filter.SortOrder)
        };

        var cursor = await _products.FindAsync(combinedFilter, findOptions);
        var items = await cursor.ToListAsync();

        return new PagedResult<Product>
        {
            Items = items,
            TotalCount = (int)totalCount,
            PageNumber = filter.PageNumber,
            PageSize = filter.PageSize
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

    public async Task<string> AddAsync(Product product)
    {
        if (string.IsNullOrWhiteSpace(product.Id))
        {
            product.Id = ObjectId.GenerateNewId().ToString();
        }
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

    private static SortDefinition<Product> GetSortDefinition(
        string? sortField,
        string? sortOrder)
    {
        var sortBuilder = Builders<Product>.Sort;

        if (string.IsNullOrWhiteSpace(sortField))
            return sortBuilder.Descending(p => p.CreatedAt);

        var isDescending = sortOrder?.ToLower() == "desc" || sortOrder == "-1";

        return sortField.ToLower() switch
        {
            "name" => isDescending
                ? sortBuilder.Descending(p => p.Name)
                : sortBuilder.Ascending(p => p.Name),

            "price" => isDescending
                ? sortBuilder.Descending(p => p.Price)
                : sortBuilder.Ascending(p => p.Price),

            "dateofmanufacture" => isDescending
                ? sortBuilder.Descending(p => p.DateOfManufacture)
                : sortBuilder.Ascending(p => p.DateOfManufacture),

            "createdat" => isDescending
                ? sortBuilder.Descending(p => p.CreatedAt)
                : sortBuilder.Ascending(p => p.CreatedAt),

            _ => sortBuilder.Descending(p => p.CreatedAt)
        };
    }

    private void CreateIndexes()
    {
        var indexKeys = Builders<Product>.IndexKeys;

        var indexModels = new List<CreateIndexModel<Product>>
    {
        // Index on Name (Ascending)
        new CreateIndexModel<Product>(
            indexKeys.Ascending(p => p.Name),
            new CreateIndexOptions
            {
                Name = "IX_Product_Name",
                Collation = new Collation("en", strength: CollationStrength.Secondary)
            })
    };

        _products.Indexes.CreateMany(indexModels);
    }

}
