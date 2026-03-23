using MediatR;
using ProductService.Application.Common.Interfaces;
using ProductService.Application.Products.DTOs;
using Shared.Kernel.Results;

namespace ProductService.Application.Products.Queries.GetProductById;

public class GetProductByIdQueryHandler : IRequestHandler<GetProductByIdQuery, Result<ProductDto>>
{
    private readonly IProductRepository _repository;
    private readonly ICurrentUserService _currentUser;

    public GetProductByIdQueryHandler(IProductRepository repository, ICurrentUserService currentUser)
    {
        _repository = repository;
        _currentUser = currentUser;
    }

    public async Task<Result<ProductDto>> Handle(GetProductByIdQuery request, CancellationToken cancellationToken)
    {
        var userId = _currentUser.GetUserId();
        var isAdmin = _currentUser.IsAdmin;
        var product = await _repository.GetByIdAsync(request.Id);

        if (product == null)
            return Result<ProductDto>.Failure($"Product with ID '{request.Id}' was not found.");

        if (!isAdmin && product.CreatedByUserId != userId)
            return Result<ProductDto>.Failure($"User {userId} is not authorized to access product {request.Id}");

        return Result<ProductDto>.Success(new ProductDto
        {
            Id = product.Id,
            Name = product.Name,
            Description = product.Description,
            Price = product.Price,
            DateOfManufacture = product.DateOfManufacture,
            CreatedByUserId = product.CreatedByUserId,
            ImageUrl = product.ImageUrl
        }, "Product fetched successfully.");
    }
}
