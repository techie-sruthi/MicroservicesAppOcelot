using MediatR;
using ProductService.Application.Common.Interfaces;

namespace ProductService.Application.Products.Commands.DeleteProduct;

public class DeleteProductCommandHandler : IRequestHandler<DeleteProductCommand, Unit>
{
    private readonly IProductRepository _repository;

    public DeleteProductCommandHandler(IProductRepository repository)
    {
        _repository = repository;
    }

    public async Task<Unit> Handle(DeleteProductCommand request, CancellationToken cancellationToken)
    {
        var product = await _repository.GetByIdAsync(request.Id);

        if (product == null)
            throw new Exception("Product not found");

        // Authorization: Non-admin users can only delete their own products
        if (!request.IsAdmin && product.CreatedByUserId != request.CurrentUserId)
        {
            throw new UnauthorizedAccessException($"User {request.CurrentUserId} is not authorized to delete product {request.Id}");
        }

        await _repository.DeleteAsync(request.Id);
        return Unit.Value;
    }
}
