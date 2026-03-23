using MediatR;
using ProductService.Application.Common.Interfaces;
using Shared.Kernel.Results;

namespace ProductService.Application.Products.Commands.DeleteProduct;

public class DeleteProductCommandHandler : IRequestHandler<DeleteProductCommand, Result>
{
    private readonly IProductRepository _repository;
    private readonly ICurrentUserService _currentUser;

    public DeleteProductCommandHandler(IProductRepository repository, ICurrentUserService currentUser)
    {
        _repository = repository;
        _currentUser = currentUser;
    }

    public async Task<Result> Handle(DeleteProductCommand request, CancellationToken cancellationToken)
    {
        var userId = _currentUser.GetUserId();
        var isAdmin = _currentUser.IsAdmin;
        var product = await _repository.GetByIdAsync(request.Id);

        if (product == null)
            return Result.Failure($"Product with ID '{request.Id}' was not found.");

        if (!isAdmin && product.CreatedByUserId != userId)
            return Result.Failure($"User {userId} is not authorized to delete product {request.Id}");

        await _repository.DeleteAsync(request.Id);
        return Result.Success("Product deleted successfully.");
    }
}
