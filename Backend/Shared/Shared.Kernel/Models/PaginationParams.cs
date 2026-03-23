namespace Shared.Kernel.Models;

public static class PaginationParams
{
    public const int MaxPageSize = 100;
    public const int DefaultPageNumber = 1;

    public static int ClampPageSize(int pageSize) => pageSize > MaxPageSize ? MaxPageSize : pageSize;
        
    public static int ClampPageNumber(int pageNumber) => 
        pageNumber < 1 ? DefaultPageNumber : pageNumber;
}
