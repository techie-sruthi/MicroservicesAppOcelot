using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using UserService.Application.Common.Interfaces;
using UserService.Application.Users.DTOs;
using UserService.Application.Contracts;

namespace UserService.Application.Users.Commands.LoginUser;

public class LoginUserCommandHandler
    : IRequestHandler<LoginUserCommand, object>
{
    private readonly IUserDbContext _context;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IJwtService _jwtService;
    private readonly IOtpService _otpService;
    private readonly IEmailService _emailService;
    private readonly ILogger<LoginUserCommandHandler> _logger;


    public LoginUserCommandHandler(
        IUserDbContext context,
        IPasswordHasher passwordHasher,
        IJwtService jwtService,
        IOtpService otpService,
        IEmailService emailService,
        ILogger<LoginUserCommandHandler> logger)

    {
        _context = context;
        _passwordHasher = passwordHasher;
        _jwtService = jwtService;
        _otpService = otpService;
        _emailService = emailService;
        _logger = logger;
       
    }


    public async Task<object> Handle(LoginUserCommand request, CancellationToken cancellationToken)
    {
        var user = await _context.Users
            .FirstOrDefaultAsync(x => x.Email == request.Email, cancellationToken);

        if (user == null)
            throw new UnauthorizedAccessException("User not found with this email address");

        if (!_passwordHasher.Verify(request.Password, user.PasswordHash))
            throw new UnauthorizedAccessException("Invalid password. Please try again");

        var otp = _otpService.GenerateOtp();
        _otpService.StoreOtp(user.Email, otp);
        await _emailService.SendOtpEmailAsync(user.Email, otp);

        _logger.LogDebug("Generated OTP for {Email}: {Otp}", user.Email, otp);

        return new
        {
            email = user.Email,
            message = "OTP sent to your email. Please verify to complete login.",
            otpRequired = true,
            expirySeconds = _otpService.GetExpirySeconds()
        };
    }
}
