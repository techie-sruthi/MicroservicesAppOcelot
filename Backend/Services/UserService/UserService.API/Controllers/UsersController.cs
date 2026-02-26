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

    [HttpPost("register")]
    public async Task<IActionResult> Register(RegisterUserCommand command)
    {
        var userId = await _mediator.Send(command);
        return Ok(userId);
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login(LoginUserCommand command)
    {
        var response = await _mediator.Send(command);
        return Ok(response);
    }

    

    // ===== OTP FEATURE ENABLED =====
    [HttpPost("verify-otp")]
    public async Task<IActionResult> VerifyOtp(VerifyOtpCommand command)
    {
            var response = await _mediator.Send(command);
            return Ok(response);   
    }


    [HttpPost("refresh")]
    public async Task<IActionResult> Refresh([FromBody] RefreshTokenCommand command)
    {
            var response = await _mediator.Send(command);
            return Ok(response);
    }

    [HttpPost]
    public async Task<IActionResult> Create(CreateUserCommand command)
        => Ok(await _mediator.Send(command));

    [HttpGet]
    public async Task<IActionResult> GetAll( [FromQuery] GetAllUsersQuery query)
    {        
        return Ok(await _mediator.Send(query));
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
        => Ok(await _mediator.Send(new GetUserByIdQuery(id)));

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, UpdateUserCommand command)
    {
        if (id != command.Id) return BadRequest();
        await _mediator.Send(command);
        return NoContent();
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        await _mediator.Send(new DeleteUserCommand(id));
        return NoContent();
    }

    [HttpGet("check-email")]
    public async Task<IActionResult> CheckEmail([FromQuery] string email)
    {
        var exists = await _mediator.Send(new CheckEmailQuery { Email = email });
        return Ok(new { exists });
    }

    [HttpPost("forgot-password")]
    public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordRequest request)
    {
        await _mediator.Send(new ForgotPasswordCommand(request.Email));
        return Ok(new { message = "If your email exists, you will receive a password reset link." });
    }

    [HttpPost("reset-password")]
    public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordRequest request)
    {
        await _mediator.Send(new ResetPasswordCommand(request.Token, request.NewPassword));
        return Ok(new { message = "Password has been reset successfully." });
    }
}

public record ForgotPasswordRequest(string Email);
public record ResetPasswordRequest(string Token, string NewPassword);
