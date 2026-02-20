using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace ProductService.Domain.Entities;

[BsonIgnoreExtraElements] // Ignore fields not in the model (like DateOfExpiry from old data)
public class Product
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string Id { get; set; } = default!;

    public string Name { get; set; } = default!;
    public string? Description { get; set; }
    public decimal Price { get; set; }
    public DateTime DateOfManufacture { get; set; }
    public int CreatedByUserId { get; set; }
    public string? ImageUrl { get; set; }

    [BsonDateTimeOptions(Kind = DateTimeKind.Utc)]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
