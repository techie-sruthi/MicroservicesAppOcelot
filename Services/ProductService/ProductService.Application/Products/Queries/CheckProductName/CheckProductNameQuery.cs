using MediatR;

namespace ProductService.Application.Products.Queries.CheckProductName;

public class CheckProductNameQuery : IRequest<bool>
{
    public string Name { get; set; } = default!;
    public string? ExcludeId { get; set; } // For edit mode - exclude current product
    public int UserId { get; set; } // For logging/auditing purposes
}
