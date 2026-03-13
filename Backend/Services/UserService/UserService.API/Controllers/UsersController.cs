using UserService.Application.Users.Commands.CreateUser;
using UserService.Application.Users.Commands.LoginUser;
using UserService.Application.Users.Commands.RegisterUser;
using UserService.Application.Users.Commands.UpdateUser;
using UserService.Application.Users.Commands.RefreshToken;
using UserService.Application.Users.Commands.VerifyOtp;
using UserService.Application.Users.Commands.ForgotPassword;
using UserService.Application.Users.Commands.ResetPassword;
using UserService.Application.Users.Commands.ChangePassword;
using UserService.Application.Users.Queries.GetAllUsers;
using UserService.Application.Users.Commands.DeleteUser;
using UserService.Application.Users.Commands.Logout;
using UserService.Application.Users.Queries.GetUserById;
using UserService.Application.Users.Queries.CheckEmail;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using UserService.API.Helpers;

namespace UserService.API.Controllers;

public class UsersController : BaseController
{
    [HttpPost("[action]")]
    public async Task<IActionResult> Register(RegisterUserCommand command)
        => Ok(await Mediator.Send(command));

    [HttpPost("[action]")]
    public async Task<IActionResult> Login(LoginUserCommand command)
        => Ok(await Mediator.Send(command));

    [HttpPost("[action]")]
    public async Task<IActionResult> VerifyOtp(VerifyOtpCommand command)
        => Ok(await Mediator.Send(command));

    [HttpPost("[action]")]
    public async Task<IActionResult> Refresh([FromBody] RefreshTokenCommand command)
        => Ok(await Mediator.Send(command));

    [HttpPost("[action]")]
    public async Task<IActionResult> Logout([FromBody] LogoutCommand command)
        => await SendNoContent(command);

    [HttpPost("[action]")]
    public async Task<IActionResult> Create(CreateUserCommand command)
        => Ok(await Mediator.Send(command));

    [HttpGet("[action]")]
    public async Task<IActionResult> GetAll([FromQuery] GetAllUsersQuery query)
        => Ok(await Mediator.Send(query));

    [HttpGet("[action]/{id}")]
    public async Task<IActionResult> GetById(int id)
        => Ok(await Mediator.Send(new GetUserByIdQuery(id)));

    [HttpPut("[action]/{id}")]
    public async Task<IActionResult> Update(int id, UpdateUserCommand command)
        => Ok(await Mediator.Send(command with { RouteId = id }));

    [HttpDelete("[action]/{id}")]
    public async Task<IActionResult> Delete(int id)
        => await SendNoContent(new DeleteUserCommand(id));

    [HttpGet("[action]")]
    public async Task<IActionResult> CheckEmail([FromQuery] string email)
        => Ok(new { exists = await Mediator.Send(new CheckEmailQuery { Email = email }) });

    [HttpPost("[action]")]
    public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordRequest request)
        => await SendOk(new ForgotPasswordCommand(request.Email), new { message = "If your email exists, you will receive a password reset link." });

    [HttpPost("[action]")]
    public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordRequest request)
        => await SendOk(new ResetPasswordCommand(request.Token, request.NewPassword), new { message = "Password has been reset successfully." });

    [HttpGet("[action]")]
    public async Task<IActionResult> GetUsersByIds([FromQuery] string ids)
        => Ok(await Mediator.Send(new GetUserByIdsQuery { Ids = ids }));

    [HttpPost("[action]")]
    public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordRequest request)
        => await SendOk(new ChangePasswordCommand(request.CurrentPassword, request.NewPassword), new { message = "Password changed successfully." });
}

public record ForgotPasswordRequest(string Email);
public record ResetPasswordRequest(string Token, string NewPassword);
public record ChangePasswordRequest(string CurrentPassword, string NewPassword);
