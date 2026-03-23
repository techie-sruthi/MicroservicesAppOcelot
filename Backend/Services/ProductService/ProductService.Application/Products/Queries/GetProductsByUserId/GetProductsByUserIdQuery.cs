using MediatR;
using Shared.Kernel.Models;
using ProductService.Application.Products.DTOs;
using Shared.Kernel.Results;

namespace ProductService.Application.Products.Queries.GetProductsByUserId;

public record GetProductsByUserIdQuery(
    int UserId, 
    int PageNumber = 1, 
    int PageSize = 10,
    string? SearchTerm = null,
    decimal? MinPrice = null,
    decimal? MaxPrice = null,
    DateTime? StartDate = null,
    string? SortField = null,
    string? SortOrder = null
) : IRequest<Result<PagedResult<ProductDto>>>;
