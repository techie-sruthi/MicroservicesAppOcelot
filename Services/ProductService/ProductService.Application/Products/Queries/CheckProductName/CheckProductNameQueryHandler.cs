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
        // Get ALL products (global check across all users)
        var allProducts = await _repository.GetAllAsync();

        // Check if name exists globally (case-insensitive), excluding current product if editing
        var exists = allProducts.Any(p => 
            p.Name.Equals(request.Name, StringComparison.OrdinalIgnoreCase) &&
            (string.IsNullOrEmpty(request.ExcludeId) || p.Id != request.ExcludeId));

        Console.WriteLine($"[CheckProductName] Name: '{request.Name}', Exists: {exists}, ExcludeId: {request.ExcludeId}");

        return exists; // Returns true if duplicate exists globally
    }
}
