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
using UserService.Application.Users.Queries.GetUsersByIds;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using UserService.API.Helpers;

namespace UserService.API.Controllers;

public class UsersController : BaseController
{
    [HttpPost("[action]")]
    public async Task<IActionResult> Register(RegisterUserCommand command)
        => ToActionResult(await Mediator.Send(command));

    [HttpPost("[action]")]
    public async Task<IActionResult> Login(LoginUserCommand command)
        => ToActionResult(await Mediator.Send(command));

    [HttpPost("[action]")]
    public async Task<IActionResult> VerifyOtp(VerifyOtpCommand command)
        => ToActionResult(await Mediator.Send(command));

    [HttpPost("[action]")]
    public async Task<IActionResult> Refresh([FromBody] RefreshTokenCommand command)
        => ToActionResult(await Mediator.Send(command));

    [HttpPost("[action]")]
    public async Task<IActionResult> Logout([FromBody] LogoutCommand command)
        => ToActionResult(await Mediator.Send(command));

    [HttpPost("[action]")]
    public async Task<IActionResult> Create(CreateUserCommand command)
        => ToActionResult(await Mediator.Send(command));

    [HttpGet("[action]")]
    public async Task<IActionResult> GetAll([FromQuery] GetAllUsersQuery query)
        => ToActionResult(await Mediator.Send(query));

    [HttpGet("[action]/{id}")]
    public async Task<IActionResult> GetById(int id)
        => ToActionResult(await Mediator.Send(new GetUserByIdQuery(id)));

    [HttpPut("[action]/{id}")]
    public async Task<IActionResult> Update(int id, UpdateUserCommand command)
        => ToActionResult(await Mediator.Send(command with { RouteId = id }));

    [HttpDelete("[action]/{id}")]
    public async Task<IActionResult> Delete(int id)
        => ToActionResult(await Mediator.Send(new DeleteUserCommand(id)));

    [HttpGet("[action]")]
    public async Task<IActionResult> CheckEmail([FromQuery] string email)
        => ToActionResult(await Mediator.Send(new CheckEmailQuery { Email = email }));

    [HttpPost("[action]")]
    public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordRequest request)
        => ToActionResult(await Mediator.Send(new ForgotPasswordCommand(request.Email)));

    [HttpPost("[action]")]
    public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordRequest request)
        => ToActionResult(await Mediator.Send(new ResetPasswordCommand(request.Token, request.NewPassword)));

    [HttpGet("[action]")]
    public async Task<IActionResult> GetUsersByIds([FromQuery] string ids)
        => ToActionResult(await Mediator.Send(new GetUserByIdsQuery { Ids = ids }));

    [HttpPost("[action]")]
    public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordRequest request)
        => ToActionResult(await Mediator.Send(new ChangePasswordCommand(request.CurrentPassword, request.NewPassword)));
}

public record ForgotPasswordRequest(string Email);
public record ResetPasswordRequest(string Token, string NewPassword);
public record ChangePasswordRequest(string CurrentPassword, string NewPassword);
