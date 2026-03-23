using Microsoft.AspNetCore.Mvc;
using Shared.Kernel.Responses;
using Shared.Kernel.Results;

namespace Shared.Kernel.Controllers;

public abstract class ResultBaseController : ControllerBase
{
    protected IActionResult ToActionResult<T>(Result<T> result)
    {
        if (result.IsSuccess)
            return Ok(ApiResponse<T>.Ok(result.Value, result.Message));

        return BadRequest(ApiResponse<T>.Fail(result.Error!));
    }

    protected IActionResult ToActionResult(Result result)
    {
        if (result.IsSuccess)
            return Ok(ApiResponse<object>.Ok(null, result.Message));

        return BadRequest(ApiResponse<object>.Fail(result.Error!));
    }
}
