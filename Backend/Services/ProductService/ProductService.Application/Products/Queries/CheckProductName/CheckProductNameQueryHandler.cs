using MediatR;
using ProductService.Application.Common.Interfaces;

namespace ProductService.Application.Products.Queries.CheckProductName;

public class CheckProductNameQueryHandler : IRequestHandler<CheckProductNameQuery, bool>
{
    private readonly IProductRepository _repository;

    public CheckProductNameQueryHandler(IProductRepository repository)
    {
        _repository = repository;
    }

    public async Task<bool> Handle(CheckProductNameQuery request, CancellationToken cancellationToken)
    {
        var allProducts = await _repository.GetAllAsync();

        var exists = allProducts.Any(p =>
            p.Name.Equals(request.Name, StringComparison.OrdinalIgnoreCase) &&
            (string.IsNullOrEmpty(request.ExcludeId) || p.Id != request.ExcludeId));

        return exists;
    }
}

