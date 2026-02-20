using MediatR;

namespace UserService.Application.Users.Queries.CheckEmail;

public class CheckEmailQuery : IRequest<bool>
{
    public string Email { get; set; } = default!;
}
