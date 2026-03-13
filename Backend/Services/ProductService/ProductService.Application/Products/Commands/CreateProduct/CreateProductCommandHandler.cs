using MediatR;
using ProductService.Application.Common.Interfaces;
using ProductService.Domain.Entities;

namespace ProductService.Application.Products.Commands.CreateProduct;

public class CreateProductCommandHandler : IRequestHandler<CreateProductCommand, string>
{
    private readonly IProductRepository _repository;
    private readonly ICurrentUserService _currentUser;

    public CreateProductCommandHandler(IProductRepository repository, ICurrentUserService currentUser)
    {
        _repository = repository;
        _currentUser = currentUser;
    }

    public async Task<string> Handle(CreateProductCommand request, CancellationToken cancellationToken)
    {
        var userId = _currentUser.UserId;
        var isAdmin = _currentUser.IsAdmin;

        var allProducts = await _repository.GetAllAsync();
        var nameExists = allProducts.Any(p =>
            p.Name.Equals(request.Name, StringComparison.OrdinalIgnoreCase));

        if (nameExists)
            throw new ArgumentException("A product with the same name already exists.");

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
        return id;
    }
}
