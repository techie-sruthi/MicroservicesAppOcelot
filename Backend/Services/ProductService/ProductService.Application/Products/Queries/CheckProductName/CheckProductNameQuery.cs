using MediatR;
using Shared.Kernel.Results;

namespace ProductService.Application.Products.Queries.CheckProductName;

public class CheckProductNameQuery : IRequest<Result<bool>>
{
    public string Name { get; set; } = default!;
    public string? ExcludeId { get; set; } 
    public int UserId { get; set; } 
}
