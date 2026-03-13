using MediatR;

namespace ProductService.Application.Products.Commands.UpdateProduct;

public class UpdateProductCommand : IRequest<Unit>
{
    public string Id { get; set; } = default!;
    public string Name { get; set; } = default!;
    public string? Description { get; set; }
    public decimal Price { get; set; }
    public DateTime DateOfManufacture { get; set; }
    public string? ImageUrl { get; set; }

    public string RouteId { get; set; } = default!;

    // Authorization context
    public int CurrentUserId { get; set; }
    public bool IsAdmin { get; set; }
}
