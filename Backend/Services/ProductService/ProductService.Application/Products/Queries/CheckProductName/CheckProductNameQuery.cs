using MediatR;

namespace ProductService.Application.Products.Queries.CheckProductName;

public class CheckProductNameQuery : IRequest<bool>
{
    public string Name { get; set; } = default!;
    public string? ExcludeId { get; set; } 
    public int UserId { get; set; } 
}
