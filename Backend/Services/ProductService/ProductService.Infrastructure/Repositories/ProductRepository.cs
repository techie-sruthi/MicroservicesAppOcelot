using Microsoft.Extensions.Options;
using MongoDB.Driver;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.Serializers;
using ProductService.Application.Common.Interfaces;
using ProductService.Application.Common.Models;
using ProductService.Domain.Entities;
using ProductService.Infrastructure.Data;

namespace ProductService.Infrastructure.Repositories;

public class ProductRepository : IProductRepository
{
    private readonly IMongoCollection<Product> _products;

    public ProductRepository(
        IMongoClient mongoClient,
        IOptions<MongoDbSettings> settings)
    {
        var mongoSettings = settings.Value;

        //// Register MongoDB class map (only once)
        //if (!BsonClassMap.IsClassMapRegistered(typeof(Product)))
        //{
        //    BsonClassMap.RegisterClassMap<Product>(cm =>
        //    {
        //        cm.AutoMap();
        //        cm.SetIgnoreExtraElements(true);
        //        cm.MapIdMember(c => c.Id)
        //          .SetElementName("_id")
        //          .SetSerializer(new StringSerializer(BsonType.ObjectId));
        //    });
        //}

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
        int pageNumber,
        int pageSize,
        string? searchTerm = null,
        decimal? minPrice = null,
        decimal? maxPrice = null,
        DateTime? startDate = null,
        string? sortField = null,
        string? sortOrder = null)
    {
        var filterBuilder = Builders<Product>.Filter;
        var filters = new List<FilterDefinition<Product>>();

        if (!string.IsNullOrWhiteSpace(searchTerm))
        {
            var regex = new BsonRegularExpression(searchTerm, "i");

            filters.Add(filterBuilder.Or(
                filterBuilder.Regex(p => p.Name, regex),
                filterBuilder.Regex(p => p.Description, regex)
            ));
        }

        if (minPrice.HasValue)
            filters.Add(filterBuilder.Gte(p => p.Price, minPrice.Value));

        if (maxPrice.HasValue)
            filters.Add(filterBuilder.Lte(p => p.Price, maxPrice.Value));

        if (startDate.HasValue)
            filters.Add(filterBuilder.Gte(p => p.DateOfManufacture, startDate.Value));

        var combinedFilter = filters.Count > 0
            ? filterBuilder.And(filters)
            : filterBuilder.Empty;

        var totalCount = await _products.CountDocumentsAsync(combinedFilter);

        var findOptions = new FindOptions<Product>
        {
            Collation = new Collation("en", strength: CollationStrength.Secondary),
            Skip = (pageNumber - 1) * pageSize,
            Limit = pageSize,
            Sort = GetSortDefinition(sortField, sortOrder)
        };

        var cursor = await _products.FindAsync(combinedFilter, findOptions);
        var items = await cursor.ToListAsync();

        return new PagedResult<Product>
        {
            Items = items,
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
        var filterBuilder = Builders<Product>.Filter;

        var filters = new List<FilterDefinition<Product>>
        {
            filterBuilder.Eq(p => p.CreatedByUserId, userId)
        };

        if (!string.IsNullOrWhiteSpace(searchTerm))
        {
            var regex = new BsonRegularExpression(searchTerm, "i");

            filters.Add(filterBuilder.Or(
                filterBuilder.Regex(p => p.Name, regex),
                filterBuilder.Regex(p => p.Description, regex)
            ));
        }

        if (minPrice.HasValue)
            filters.Add(filterBuilder.Gte(p => p.Price, minPrice.Value));

        if (maxPrice.HasValue)
            filters.Add(filterBuilder.Lte(p => p.Price, maxPrice.Value));

        if (startDate.HasValue)
            filters.Add(filterBuilder.Gte(p => p.DateOfManufacture, startDate.Value));

        var combinedFilter = filterBuilder.And(filters);

        var totalCount = await _products.CountDocumentsAsync(combinedFilter);

        var findOptions = new FindOptions<Product>
        {
            Collation = new Collation("en", strength: CollationStrength.Secondary),
            Skip = (pageNumber - 1) * pageSize,
            Limit = pageSize,
            Sort = GetSortDefinition(sortField, sortOrder)
        };

        var cursor = await _products.FindAsync(combinedFilter, findOptions);
        var items = await cursor.ToListAsync();

        return new PagedResult<Product>
        {
            Items = items,
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
