using MediatR;
using ProductService.Application.Common.Interfaces;
using ProductService.Application.Common.Models;
using ProductService.Application.Products.DTOs;
using Shared.Kernel.Models;
using Shared.Kernel.Results;

namespace ProductService.Application.Products.Queries.GetProductsByUserId;

public class GetProductsByUserIdQueryHandler : IRequestHandler<GetProductsByUserIdQuery, Result<PagedResult<ProductDto>>>
{
    private readonly IProductRepository _repository;
    private readonly ICurrentUserService _currentUser;

    public GetProductsByUserIdQueryHandler(IProductRepository repository, ICurrentUserService currentUser)
    {
        _repository = repository;
        _currentUser = currentUser;
    }

    public async Task<Result<PagedResult<ProductDto>>> Handle(GetProductsByUserIdQuery request, CancellationToken cancellationToken)
    {
        request = request with
        {
            PageNumber = PaginationParams.ClampPageNumber(request.PageNumber),
            PageSize = PaginationParams.ClampPageSize(request.PageSize)
        };

        var userId = _currentUser.GetUserId();

        var filter = new UserProductFilter(
            UserId: userId,
            PageNumber: request.PageNumber,
            PageSize: request.PageSize,
            SearchTerm: request.SearchTerm,
            MinPrice: request.MinPrice,
            MaxPrice: request.MaxPrice,
            StartDate: request.StartDate,
            SortField: request.SortField,
            SortOrder: request.SortOrder
        );

        var pagedResult = await _repository.GetByUserIdPagedAsync(filter);

        return Result<PagedResult<ProductDto>>.Success(
            pagedResult.ToDtoPage(),
            "Products fetched successfully.");
    }
}
