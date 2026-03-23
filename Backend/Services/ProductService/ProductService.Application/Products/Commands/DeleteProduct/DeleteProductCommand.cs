using MediatR;
using Shared.Kernel.Results;

namespace ProductService.Application.Products.Commands.DeleteProduct;

public class DeleteProductCommand : IRequest<Result>
{
    public string Id { get; set; }

    public int CurrentUserId { get; set; }
    public bool IsAdmin { get; set; }

    public DeleteProductCommand(string id)
    {
        Id = id;
    }
}
