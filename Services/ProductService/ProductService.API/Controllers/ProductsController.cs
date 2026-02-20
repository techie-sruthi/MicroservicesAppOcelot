using MediatR;
using Microsoft.AspNetCore.Mvc;
using ProductService.Application.Products.Commands.CreateProduct;
using ProductService.Application.Products.Commands.UpdateProduct;
using ProductService.Application.Products.Queries.GetAllProducts;
using ProductService.Application.Products.Commands.DeleteProduct;
using ProductService.Application.Products.Queries.GetProductById;
using ProductService.Application.Products.Queries.GetProductsByUserId;
using ProductService.Application.Products.Queries.CheckProductName;
using Microsoft.AspNetCore.Authorization;
using ProductService.API.Helpers;
using ProductService.API.Validators;
using ProductService.Application.Common.Interfaces;

namespace ProductService.API.Controllers;

[Authorize]
[ApiController]
[Route("api/products")]
public class ProductsController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly IFileStorageService _fileStorageService;

    public ProductsController(
        IMediator mediator,
        IFileStorageService fileStorageService)
    {
        _mediator = mediator;
        _fileStorageService = fileStorageService;
    }

    [HttpPost]
    public async Task<IActionResult> Create(CreateProductCommand command)
    {
        command.CreatedByUserId = User.GetUserId();
        var productId = await _mediator.Send(command);
        return Ok(new { id = productId });
    }

    [HttpGet]
    public async Task<IActionResult> GetProducts([FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 10)
    {
        // Let the handler decide what to return based on user role
        var query = User.IsAdmin()
            ? new GetAllProductsQuery(pageNumber, pageSize)
            : (object)new GetProductsByUserIdQuery(User.GetUserId(), pageNumber, pageSize);

        return Ok(await _mediator.Send(query));
    }

    [HttpGet("all")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> GetAllProducts(
        [FromQuery] int pageNumber = 1, 
        [FromQuery] int pageSize = 10,
        [FromQuery] string? searchTerm = null,
        [FromQuery] decimal? minPrice = null,
        [FromQuery] decimal? maxPrice = null,
        [FromQuery] DateTime? startDate = null,
        [FromQuery] string? sortField = null,
        [FromQuery] string? sortOrder = null)
    {
        var query = new GetAllProductsQuery(
            pageNumber, 
            pageSize, 
            searchTerm, 
            minPrice, 
            maxPrice, 
            startDate,
            sortField,
            sortOrder);

        return Ok(await _mediator.Send(query));
    }

    [HttpGet("my-products")]
    public async Task<IActionResult> GetMyProducts(
        [FromQuery] int pageNumber = 1, 
        [FromQuery] int pageSize = 10,
        [FromQuery] string? searchTerm = null,
        [FromQuery] decimal? minPrice = null,
        [FromQuery] decimal? maxPrice = null,
        [FromQuery] DateTime? startDate = null,
        [FromQuery] string? sortField = null,
        [FromQuery] string? sortOrder = null)
    {
        var userId = User.GetUserId();
        var query = new GetProductsByUserIdQuery(
            userId, 
            pageNumber, 
            pageSize,
            searchTerm,
            minPrice,
            maxPrice,
            startDate,
            sortField,
            sortOrder);

        return Ok(await _mediator.Send(query));
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(string id)
    {
        var query = new GetProductByIdQuery(id)
        {
            CurrentUserId = User.GetUserId(),
            IsAdmin = User.IsAdmin()
        };

        var product = await _mediator.Send(query);
        return Ok(product);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(string id, UpdateProductCommand command)
    {
        if (id != command.Id)
            return BadRequest("Product ID mismatch");

        command.CurrentUserId = User.GetUserId();
        command.IsAdmin = User.IsAdmin();

        await _mediator.Send(command);
        return NoContent();
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(string id)
    {
        var command = new DeleteProductCommand(id)
        {
            CurrentUserId = User.GetUserId(),
            IsAdmin = User.IsAdmin()
        };

        await _mediator.Send(command);
        return NoContent();
    }

    [HttpPost("upload-image")]
    public async Task<IActionResult> UploadImage(IFormFile file)
    {
        FileValidator.ValidateImageFile(file);

        using var stream = file.OpenReadStream();
        var imageUrl = await _fileStorageService.UploadFileAsync(stream, file.FileName, file.ContentType);

        return Ok(new { imageUrl });
    }

    [HttpGet("check-name")]
    public async Task<IActionResult> CheckProductName([FromQuery] string name, [FromQuery] string? excludeId = null)
    {
        var query = new CheckProductNameQuery
        {
            Name = name,
            ExcludeId = excludeId,
            UserId = User.GetUserId()
        };

        var exists = await _mediator.Send(query);
        return Ok(new { exists });
    }
}
