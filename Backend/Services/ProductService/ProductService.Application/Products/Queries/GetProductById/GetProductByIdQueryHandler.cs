using MediatR;
using ProductService.Application.Common.Interfaces;
using ProductService.Application.Products.DTOs;

namespace ProductService.Application.Products.Queries.GetProductById;

public class GetProductByIdQueryHandler : IRequestHandler<GetProductByIdQuery, ProductDto>
{
    private readonly IProductRepository _repository;

    public GetProductByIdQueryHandler(IProductRepository repository)
    {
        _repository = repository;
    }

    public async Task<ProductDto> Handle(GetProductByIdQuery request, CancellationToken cancellationToken)
    {
        var product = await _repository.GetByIdAsync(request.Id);

        if (product == null)
            throw new Exception("Product not found");

        // Authorization: Non-admin users can only view their own products
        if (!request.IsAdmin && product.CreatedByUserId != request.CurrentUserId)
        {
            throw new UnauthorizedAccessException($"User {request.CurrentUserId} is not authorized to access product {request.Id}");
        }

        return new ProductDto
        {
            Id = product.Id,
            Name = product.Name,
            Description = product.Description,
            Price = product.Price,
            DateOfManufacture = product.DateOfManufacture,
            CreatedByUserId = product.CreatedByUserId,
            ImageUrl = product.ImageUrl
        };
    }
}
