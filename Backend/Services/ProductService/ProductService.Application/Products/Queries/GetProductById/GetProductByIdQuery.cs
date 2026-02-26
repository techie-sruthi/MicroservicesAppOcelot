using MediatR;
using ProductService.Application.Products.DTOs;

namespace ProductService.Application.Products.Queries.GetProductById;

public class GetProductByIdQuery : IRequest<ProductDto>
{
    public string Id { get; set; } = default!;

    // Authorization context
    public int CurrentUserId { get; set; }
    public bool IsAdmin { get; set; }

    public GetProductByIdQuery(string id)
    {
        Id = id;
    }
}
