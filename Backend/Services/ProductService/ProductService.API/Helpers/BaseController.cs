using MediatR;
using Microsoft.AspNetCore.Mvc;
using Shared.Kernel.Controllers;

namespace ProductService.API.Helpers;

[ApiController]
[Route("api/[controller]")]
public abstract class BaseController : ResultBaseController
{
    private IMediator? _mediator;

    protected IMediator Mediator =>
        _mediator ??= HttpContext.RequestServices.GetRequiredService<IMediator>();
}