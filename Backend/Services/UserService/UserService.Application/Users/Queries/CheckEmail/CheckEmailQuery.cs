using MediatR;
using Shared.Kernel.Results;

namespace UserService.Application.Users.Queries.CheckEmail;

public class CheckEmailQuery : IRequest<Result<bool>>
{
    public string Email { get; set; } = default!;
}
