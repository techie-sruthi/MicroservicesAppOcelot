namespace UserService.Application.Common.Interfaces;

public interface ICurrentUserService
{
    int GetUserId();
    bool IsAdmin { get; }
}