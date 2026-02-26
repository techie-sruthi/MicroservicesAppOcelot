using MediatR;
using ProductService.Application.Common.Interfaces;
using ProductService.Application.Common.Models;
using ProductService.Application.Products.DTOs;

namespace ProductService.Application.Products.Queries.GetAllProducts;

public class GetAllProductsQueryHandler : IRequestHandler<GetAllProductsQuery, PagedResult<ProductDto>>
{
    private readonly IProductRepository _repository;

    public GetAllProductsQueryHandler(IProductRepository repository)
    {
        _repository = repository;
    }

    public async Task<PagedResult<ProductDto>> Handle(GetAllProductsQuery request, CancellationToken cancellationToken)
    {
        var pagedResult = await _repository.GetAllPagedWithFiltersAsync(
            request.PageNumber, 
            request.PageSize,
            request.SearchTerm,
            request.MinPrice,
            request.MaxPrice,
            request.StartDate,
            request.SortField,
            request.SortOrder
        );

        return new PagedResult<ProductDto>
        {
            Items = pagedResult.Items.Select(p => new ProductDto
            {
                Id = p.Id,
                Name = p.Name,
                Description = p.Description,
                Price = p.Price,
                DateOfManufacture = p.DateOfManufacture,
                CreatedByUserId = p.CreatedByUserId,
                ImageUrl = p.ImageUrl
            }).ToList(),
            TotalCount = pagedResult.TotalCount,
            PageNumber = pagedResult.PageNumber,
            PageSize = pagedResult.PageSize
        };
    }
}
