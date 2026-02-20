using MediatR;
using ProductService.Application.Common.Interfaces;
using ProductService.Domain.Entities;

namespace ProductService.Application.Products.Commands.CreateProduct;

public class CreateProductCommandHandler : IRequestHandler<CreateProductCommand, string>
{
    private readonly IProductRepository _repository;

    public CreateProductCommandHandler(IProductRepository repository)
    {
        _repository = repository;
    }

    public async Task<string> Handle(CreateProductCommand request, CancellationToken cancellationToken)
    {
        var product = new Product
        {
            Name = request.Name,
            Description = request.Description,
            Price = request.Price,
            DateOfManufacture = request.DateOfManufacture,
            CreatedByUserId = request.CreatedByUserId,
            ImageUrl = request.ImageUrl,
            CreatedAt = DateTime.UtcNow // Set creation timestamp
        };

        var id = await _repository.AddAsync(product);
        return id;
    }
}
