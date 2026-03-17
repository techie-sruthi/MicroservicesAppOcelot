using MediatR;
using ProductService.Application.Common.Interfaces;

namespace ProductService.Application.Products.Commands.DeleteProduct;

public class DeleteProductCommandHandler : IRequestHandler<DeleteProductCommand, Unit>
{
    private readonly IProductRepository _repository;
    private readonly ICurrentUserService _currentUser;

    public DeleteProductCommandHandler(IProductRepository repository, ICurrentUserService currentUser)
    {
        _repository = repository;
        _currentUser = currentUser;
    }

    public async Task<Unit> Handle(DeleteProductCommand request, CancellationToken cancellationToken)
    {
        var userId = _currentUser.GetUserId();
        var isAdmin = _currentUser.IsAdmin;
        var product = await _repository.GetByIdAsync(request.Id);

        if (product == null)
            throw new KeyNotFoundException($"Product with ID '{request.Id}' was not found.");

        // Authorization: Non-admin users can only delete their own products
        if (!isAdmin && product.CreatedByUserId != userId)
        {
            throw new UnauthorizedAccessException($"User {userId} is not authorized to delete product {request.Id}");
        }

        await _repository.DeleteAsync(request.Id);
        return Unit.Value;
    }
}
