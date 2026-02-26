namespace ProductService.Application.Common.Interfaces;

public interface ICurrentUserService
{
    int UserId { get; }
    bool IsAdmin { get; }
}