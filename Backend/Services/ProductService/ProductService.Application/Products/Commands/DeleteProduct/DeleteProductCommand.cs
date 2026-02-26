using MediatR;

namespace ProductService.Application.Products.Commands.DeleteProduct;

public class DeleteProductCommand : IRequest<Unit>
{
    public string Id { get; set; } = default!;

    // Authorization context
    public int CurrentUserId { get; set; }
    public bool IsAdmin { get; set; }

    public DeleteProductCommand(string id)
    {
        Id = id;
    }
}
