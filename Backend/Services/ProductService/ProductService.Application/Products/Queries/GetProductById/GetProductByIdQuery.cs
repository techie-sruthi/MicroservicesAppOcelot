using MediatR;
using ProductService.Application.Products.DTOs;
using Shared.Kernel.Results;

namespace ProductService.Application.Products.Queries.GetProductById;

public class GetProductByIdQuery : IRequest<Result<ProductDto>>
{
    public string Id { get; set; }

    public int CurrentUserId { get; set; }
    public bool IsAdmin { get; set; }

    public GetProductByIdQuery(string id)
    {
        Id = id;
    }
}
