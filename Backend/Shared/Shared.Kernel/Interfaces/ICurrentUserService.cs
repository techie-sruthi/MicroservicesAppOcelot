namespace Shared.Kernel.Interfaces;

public interface ICurrentUserService
{
    int GetUserId();
    bool IsAdmin { get; }
}
