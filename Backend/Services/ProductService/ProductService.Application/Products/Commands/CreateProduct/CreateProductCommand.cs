using MediatR;

namespace ProductService.Application.Products.Commands.CreateProduct;

public class CreateProductCommand : IRequest<string>
{
    public string Name { get; set; } = default!;
    public string? Description { get; set; }
    public decimal Price { get; set; }
    public DateTime DateOfManufacture { get; set; }
    public int CreatedByUserId { get; set; }
    public string? ImageUrl { get; set; }
}
