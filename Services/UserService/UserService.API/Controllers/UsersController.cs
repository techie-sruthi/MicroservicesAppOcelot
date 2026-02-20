using UserService.Application.Users.Commands.CreateUser;
using UserService.Application.Users.Commands.LoginUser;
using UserService.Application.Users.Commands.RegisterUser;
using UserService.Application.Users.Commands.UpdateUser;
using UserService.Application.Users.Commands.RefreshToken;
using UserService.Application.Users.Commands.VerifyOtp;
using UserService.Application.Users.Commands.ForgotPassword;
using UserService.Application.Users.Commands.ResetPassword;
using UserService.Application.Users.Queries.GetAllUsers;
using UserService.Application.Users.Commands.DeleteUser;
using UserService.Application.Users.Queries.GetUserById;
using UserService.Application.Users.Queries.CheckEmail;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using UserService.API.Helpers;

namespace UserService.API.Controllers;

[Authorize]
[ApiController]
[Route("api/users")]
public class UsersController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly ILogger<UsersController> _logger;

    public UsersController(IMediator mediator, ILogger<UsersController> logger)
    {
        _mediator = mediator;
        _logger = logger;
    }

    [AllowAnonymous]
    [HttpPost("register")]
    public async Task<IActionResult> Register(RegisterUserCommand command)
    {
        var userId = await _mediator.Send(command);
        return Ok(userId);
    }

    [AllowAnonymous]
    [HttpPost("login")]
    public async Task<IActionResult> Login(LoginUserCommand command)
    {
        try
        {
            var response = await _mediator.Send(command);
            return Ok(response);
        }
        catch (UnauthorizedAccessException ex)
        {
            return Unauthorized(new { error = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogWarning($"Login error: {ex.Message}");
            return BadRequest(new { error = "Login failed. Please try again." });
        }
    }

    // ===== OTP FEATURE COMMENTED OUT =====
    // [AllowAnonymous]
    // [HttpPost("verify-otp")]
    // public async Task<IActionResult> VerifyOtp(VerifyOtpCommand command)
    // {
    //     try
    //     {
    //         var response = await _mediator.Send(command);
    //         return Ok(response);
    //     }
    //     catch (UnauthorizedAccessException ex)
    //     {
    //         return Unauthorized(new { error = ex.Message });
    //     }
    // }
    // =====================================

    [AllowAnonymous]
    [HttpPost("refresh")]
    public async Task<IActionResult> Refresh([FromBody] RefreshTokenCommand command)
    {
        try
        {
            var response = await _mediator.Send(command);
            return Ok(response);
        }
        catch (UnauthorizedAccessException ex)
        {
            _logger.LogWarning($"Refresh token failed: {ex.Message}");
            return Unauthorized(new { error = ex.Message });
        }
    }

    [Authorize]
    [HttpPost]
    public async Task<IActionResult> Create(CreateUserCommand command)
        => Ok(await _mediator.Send(command));

    [HttpGet]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> GetAll(
        [FromQuery] int pageNumber = 1, 
        [FromQuery] int pageSize = 10,
        [FromQuery] string? searchTerm = null,
        [FromQuery] string? roleFilter = null,
        [FromQuery] string? sortField = null,
        [FromQuery] string? sortOrder = null)
    {
        var query = new GetAllUsersQuery(
            pageNumber, 
            pageSize,
            searchTerm,
            roleFilter,
            sortField,
            sortOrder);
            
        return Ok(await _mediator.Send(query));
    }

    [Authorize]
    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
        => Ok(await _mediator.Send(new GetUserByIdQuery(id)));

    [Authorize]
    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, UpdateUserCommand command)
    {
        if (id != command.Id) return BadRequest();
        await _mediator.Send(command);
        return NoContent();
    }

    [Authorize]
    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        await _mediator.Send(new DeleteUserCommand(id));
        return NoContent();
    }

    [AllowAnonymous]
    [HttpGet("check-email")]
    public async Task<IActionResult> CheckEmail([FromQuery] string email)
    {
        var exists = await _mediator.Send(new CheckEmailQuery { Email = email });
        return Ok(new { exists });
    }

    [AllowAnonymous]
    [HttpPost("forgot-password")]
    public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordRequest request)
    {
        await _mediator.Send(new ForgotPasswordCommand(request.Email));
        return Ok(new { message = "If your email exists, you will receive a password reset link." });
    }

    [AllowAnonymous]
    [HttpPost("reset-password")]
    public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordRequest request)
    {
        try
        {
            await _mediator.Send(new ResetPasswordCommand(request.Token, request.NewPassword));
            return Ok(new { message = "Password has been reset successfully." });
        }
        catch (UnauthorizedAccessException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }
}

public record ForgotPasswordRequest(string Email);
public record ResetPasswordRequest(string Token, string NewPassword);
