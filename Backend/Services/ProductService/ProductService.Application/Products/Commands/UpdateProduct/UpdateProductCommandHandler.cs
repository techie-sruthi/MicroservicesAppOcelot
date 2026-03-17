using MediatR;
using ProductService.Application.Common.Interfaces;
using ProductService.Domain.Entities;

namespace ProductService.Application.Products.Commands.UpdateProduct;

public class UpdateProductCommandHandler : IRequestHandler<UpdateProductCommand, Unit>
{
    private readonly IProductRepository _repository;
    private readonly ICurrentUserService _currentUser;

    public UpdateProductCommandHandler(IProductRepository repository, ICurrentUserService currentUser)
    {
        _repository = repository;
        _currentUser = currentUser;
    }

    public async Task<Unit> Handle(UpdateProductCommand request, CancellationToken cancellationToken)
    {
        var userId = _currentUser.GetUserId();
        var isAdmin = _currentUser.IsAdmin;

        var product = await _repository.GetByIdAsync(request.Id);

        if (product == null)
            throw new KeyNotFoundException($"Product with ID '{request.Id}' was not found.");

        if (!isAdmin && product.CreatedByUserId != userId)
        {
            throw new UnauthorizedAccessException($"User {userId} is not authorized to update product {request.Id}");
        }

        product.Name = request.Name;
        product.Description = request.Description;
        product.Price = request.Price;
        product.DateOfManufacture = request.DateOfManufacture;
        product.ImageUrl = request.ImageUrl;

        await _repository.UpdateAsync(product);
        return Unit.Value;
    }
}
