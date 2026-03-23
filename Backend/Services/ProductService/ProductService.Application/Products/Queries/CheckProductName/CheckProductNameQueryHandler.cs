using MediatR;
using ProductService.Application.Common.Interfaces;
using Shared.Kernel.Results;

namespace ProductService.Application.Products.Queries.CheckProductName;

public class CheckProductNameQueryHandler : IRequestHandler<CheckProductNameQuery, Result<bool>>
{
    private readonly IProductRepository _repository;

    public CheckProductNameQueryHandler(IProductRepository repository)
    {
        _repository = repository;
    }

    public async Task<Result<bool>> Handle(CheckProductNameQuery request, CancellationToken cancellationToken)
    {
        var allProducts = await _repository.GetAllAsync();

        var exists = allProducts.Any(p =>
            p.Name.Equals(request.Name, StringComparison.OrdinalIgnoreCase) &&
            (string.IsNullOrEmpty(request.ExcludeId) || p.Id != request.ExcludeId));

        return Result<bool>.Success(exists, exists ? "Product name exists." : "Product name is available.");
    }
}

