using MediatR;
using ProductService.Application.Common.Interfaces;
using Shared.Kernel.Models;
using ProductService.Application.Products.DTOs;
using Shared.Kernel.Results;

namespace ProductService.Application.Products.Queries.GetAllProducts;

public class GetAllProductsQueryHandler
    : IRequestHandler<GetAllProductsQuery, Result<PagedResult<ProductDto>>>
{
    private readonly IProductRepository _repository;

    public GetAllProductsQueryHandler(IProductRepository repository)
    {
        _repository = repository;
    }

    public async Task<Result<PagedResult<ProductDto>>> Handle(
        GetAllProductsQuery request,
        CancellationToken cancellationToken)
    {
        request = request with
        {
            PageNumber = PaginationParams.ClampPageNumber(request.PageNumber),
            PageSize = PaginationParams.ClampPageSize(request.PageSize)
        };

        var pagedResult = await _repository.GetAllPagedWithFiltersAsync(request, cancellationToken);

        return Result<PagedResult<ProductDto>>.Success(
            pagedResult.ToDtoPage(),
            "Products fetched successfully.");
    }
}
