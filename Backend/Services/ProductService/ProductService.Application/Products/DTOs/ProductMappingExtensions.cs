using ProductService.Domain.Entities;
using Shared.Kernel.Models;

namespace ProductService.Application.Products.DTOs;

public static class ProductMappingExtensions
{
    public static ProductDto ToDto(this Product product)
    {
        return new ProductDto
        {
            Id = product.Id,
            Name = product.Name,
            Description = product.Description,
            Price = product.Price,
            DateOfManufacture = product.DateOfManufacture,
            CreatedByUserId = product.CreatedByUserId,
            ImageUrl = product.ImageUrl
        };
    }

    public static PagedResult<ProductDto> ToDtoPage(this PagedResult<Product> pagedResult)
    {
        return new PagedResult<ProductDto>
        {
            Items = pagedResult.Items.Select(product => product.ToDto()).ToList(),
            TotalCount = pagedResult.TotalCount,
            PageNumber = pagedResult.PageNumber,
            PageSize = pagedResult.PageSize
        };
    }
}