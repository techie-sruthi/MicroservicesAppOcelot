using MediatR;
using ProductService.Application.Common.Models;
using ProductService.Application.Products.DTOs;

namespace ProductService.Application.Products.Queries.GetAllProducts;

public record GetAllProductsQuery(
    int PageNumber = 1, 
    int PageSize = 10,
    string? SearchTerm = null,
    decimal? MinPrice = null,
    decimal? MaxPrice = null,
    DateTime? StartDate = null,
    string? SortField = null,
    string? SortOrder = null
) : IRequest<PagedResult<ProductDto>>;
