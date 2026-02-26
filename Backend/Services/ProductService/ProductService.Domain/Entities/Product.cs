namespace ProductService.Domain.Entities;

public class Product
{
    public string Id { get; set; } = default!;

    public string Name { get; set; } = default!;
    public string? Description { get; set; }
    public decimal Price { get; set; }
    public DateTime DateOfManufacture { get; set; }
    public int CreatedByUserId { get; set; }
    public string? ImageUrl { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
