using MediatR;
using ProductService.Application.Common.Interfaces;
using ProductService.Application.Common.Models;
using ProductService.Application.Products.DTOs;

namespace ProductService.Application.Products.Queries.GetProductsByUserId;

public class GetProductsByUserIdQueryHandler : IRequestHandler<GetProductsByUserIdQuery, PagedResult<ProductDto>>
{
    private readonly IProductRepository _repository;
    private readonly ICurrentUserService _currentUser;

    public GetProductsByUserIdQueryHandler(IProductRepository repository, ICurrentUserService currentUser)
    {
        _repository = repository;
        _currentUser = currentUser;
    }

    public async Task<PagedResult<ProductDto>> Handle(GetProductsByUserIdQuery request, CancellationToken cancellationToken)
    {

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
