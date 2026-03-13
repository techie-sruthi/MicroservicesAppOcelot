using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace UserService.API.Helpers;

[ApiController]
[Route("api/[controller]")]
public abstract class BaseController : ControllerBase
{
    private IMediator? _mediator;

    protected IMediator Mediator =>
        _mediator ??= HttpContext.RequestServices.GetRequiredService<IMediator>();

    protected async Task<IActionResult> SendNoContent<TResponse>(IRequest<TResponse> request, CancellationToken ct = default)
    {
        await Mediator.Send(request, ct);
        return NoContent();
    }

    protected async Task<IActionResult> SendNoContent(IRequest request, CancellationToken ct = default)
    {
        await Mediator.Send(request, ct);
        return NoContent();
    }

    protected async Task<IActionResult> SendOk<TResponse>(IRequest<TResponse> request, object response, CancellationToken ct = default)
    {
        await Mediator.Send(request, ct);
        return Ok(response);
    }

    protected async Task<IActionResult> SendOk(IRequest request, object response, CancellationToken ct = default)
    {
        await Mediator.Send(request, ct);
        return Ok(response);
    }
}
