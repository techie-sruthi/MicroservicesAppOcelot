using MediatR;
using ProductService.Application.Common.Interfaces;
using ProductService.Domain.Entities;
using Shared.Kernel.Results;

namespace ProductService.Application.Products.Commands.UpdateProduct;

public class UpdateProductCommandHandler : IRequestHandler<UpdateProductCommand, Result>
{
    private readonly IProductRepository _repository;
    private readonly ICurrentUserService _currentUser;

    public UpdateProductCommandHandler(IProductRepository repository, ICurrentUserService currentUser)
    {
        _repository = repository;
        _currentUser = currentUser;
    }

    public async Task<Result> Handle(UpdateProductCommand request, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.Name))
            return Result.Failure("Product name is required.");

        if (request.Price <= 0)
            return Result.Failure("Price must be greater than zero.");

        var userId = _currentUser.GetUserId();
        var isAdmin = _currentUser.IsAdmin;

        var product = await _repository.GetByIdAsync(request.Id);

        if (product == null)
            return Result.Failure($"Product with ID '{request.Id}' was not found.");

        if (!isAdmin && product.CreatedByUserId != userId)
            return Result.Failure($"User {userId} is not authorized to update product {request.Id}");

        product.Name = request.Name;
        product.Description = request.Description;
        product.Price = request.Price;
        product.DateOfManufacture = request.DateOfManufacture;
        product.ImageUrl = request.ImageUrl;

        await _repository.UpdateAsync(product);
        return Result.Success("Product updated successfully.");
    }
}
