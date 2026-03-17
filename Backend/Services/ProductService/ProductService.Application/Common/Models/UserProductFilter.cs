namespace ProductService.Application.Common.Models;

public record UserProductFilter(
    int UserId,
    int PageNumber = 1,
    int PageSize = 10,
    string? SearchTerm = null,
    decimal? MinPrice = null,
    decimal? MaxPrice = null,
    DateTime? StartDate = null,
    string? SortField = null,
    string? SortOrder = null);
