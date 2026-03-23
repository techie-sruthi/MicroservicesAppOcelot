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
using Microsoft.Extensions.Logging;
namespace ProductService.API.Controllers;

public class ProductsController : BaseController
{
    [HttpPost("[action]")]
    public async Task<IActionResult> Create(CreateProductCommand command)
        => ToActionResult(await Mediator.Send(command));

    [HttpGet("[action]")]
    public async Task<IActionResult> GetAllProducts([FromQuery] GetAllProductsQuery query)
        => ToActionResult(await Mediator.Send(query));

    [HttpGet("[action]")]
    public async Task<IActionResult> GetMyProducts([FromQuery] GetProductsByUserIdQuery query)
        => ToActionResult(await Mediator.Send(query));

    [HttpGet("[action]/{id}")]
    public async Task<IActionResult> GetById(string id)
        => ToActionResult(await Mediator.Send(new GetProductByIdQuery(id)));

    [HttpPut("[action]/{id}")]
    public async Task<IActionResult> Update(string id, UpdateProductCommand command)
        => ToActionResult(await Mediator.Send(command));
    
    [HttpDelete("[action]/{id}")]
    public async Task<IActionResult> Delete(string id)
        => ToActionResult(await Mediator.Send(new DeleteProductCommand(id)));

    [HttpPost("[action]")]
    public async Task<IActionResult> UploadImage(IFormFile file)
        => ToActionResult(await Mediator.Send(new UploadImageCommand(file)));

    [HttpGet("[action]")]
    public async Task<IActionResult> CheckProductName([FromQuery] CheckProductNameQuery query)
        => ToActionResult(await Mediator.Send(query));
}
