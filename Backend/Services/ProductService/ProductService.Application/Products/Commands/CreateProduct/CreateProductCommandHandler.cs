using MediatR;
using ProductService.Application.Common.Interfaces;
using ProductService.Domain.Entities;
using Shared.Kernel.Results;

namespace ProductService.Application.Products.Commands.CreateProduct;

public class CreateProductCommandHandler : IRequestHandler<CreateProductCommand, Result<string>>
{
    private readonly IProductRepository _repository;
    private readonly ICurrentUserService _currentUser;

    public CreateProductCommandHandler(IProductRepository repository, ICurrentUserService currentUser)
    {
        _repository = repository;
        _currentUser = currentUser;
    }

    public async Task<Result<string>> Handle(CreateProductCommand request, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.Name))
            return Result<string>.Failure("Product name is required.");

        if (request.Price <= 0)
            return Result<string>.Failure("Price must be greater than zero.");

        var userId = _currentUser.GetUserId();

        var allProducts = await _repository.GetAllAsync();
        var nameExists = allProducts.Any(p =>
            p.Name.Equals(request.Name, StringComparison.OrdinalIgnoreCase));

        if (nameExists)
            return Result<string>.Failure("A product with the same name already exists.");

        var product = new Product
        {
            Name = request.Name,
            Description = request.Description,
            Price = request.Price,
            DateOfManufacture = request.DateOfManufacture,
            CreatedByUserId = userId,
            ImageUrl = request.ImageUrl,
            CreatedAt = DateTime.UtcNow 
        };

        var id = await _repository.AddAsync(product);
        return Result<string>.Success(id, "Product created successfully.");
    }
}
