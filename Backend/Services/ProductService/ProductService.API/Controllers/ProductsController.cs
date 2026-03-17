using Microsoft.AspNetCore.Mvc;
using ProductService.Application.Products.Commands.CreateProduct;
using ProductService.Application.Products.Commands.UpdateProduct;
using ProductService.Application.Products.Queries.GetAllProducts;
using ProductService.Application.Products.Commands.DeleteProduct;
using ProductService.Application.Products.Queries.GetProductById;
using ProductService.Application.Products.Queries.GetProductsByUserId;
using ProductService.Application.Products.Queries.CheckProductName;
using ProductService.Application.Products.Commands.UploadImage;
using ProductService.API.Helpers;

namespace ProductService.API.Controllers;

public class ProductsController : BaseController
{
    [HttpPost("[action]")]
    public async Task<IActionResult> Create(CreateProductCommand command)
        => Ok(new { id = await Mediator.Send(command) });

    [HttpGet("[action]")]
    public async Task<IActionResult> GetAllProducts([FromQuery] GetAllProductsQuery query)
        => Ok(await Mediator.Send(query));

    [HttpGet("[action]")]
    public async Task<IActionResult> GetMyProducts([FromQuery] GetProductsByUserIdQuery query)
        => Ok(await Mediator.Send(query));

    [HttpGet("[action]/{id}")]
    public async Task<IActionResult> GetById(string id)
        => Ok(await Mediator.Send(new GetProductByIdQuery(id)));

    [HttpPut("[action]/{id}")]
    public async Task<IActionResult> Update(string id, UpdateProductCommand command)
    { command.Id = id; return await SendNoContent(command); }

    [HttpDelete("[action]/{id}")]
    public async Task<IActionResult> Delete(string id)
        => await SendNoContent(new DeleteProductCommand(id) { CurrentUserId = User.GetUserId(), IsAdmin = User.IsAdmin() });

    [HttpPost("[action]")]
    public async Task<IActionResult> UploadImage(IFormFile file)
        => Ok(new { imageUrl = await Mediator.Send(new UploadImageCommand(file)) });

    [HttpGet("[action]")]
    public async Task<IActionResult> CheckProductName([FromQuery] CheckProductNameQuery query)
        => Ok(new { exists = await Mediator.Send(query) });
}
