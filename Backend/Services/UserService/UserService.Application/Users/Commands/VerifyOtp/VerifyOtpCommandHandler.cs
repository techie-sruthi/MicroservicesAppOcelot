using MediatR;
using Microsoft.EntityFrameworkCore;
using Shared.Kernel.Results;
using UserService.Application.Common.Interfaces;
using UserService.Application.Contracts;
using UserService.Application.Users.DTOs;

namespace UserService.Application.Users.Commands.VerifyOtp;

public class VerifyOtpCommandHandler : IRequestHandler<VerifyOtpCommand, Result<LoginResponse>>
{
    private readonly IUserDbContext _context;
    private readonly IJwtService _jwtService;
    private readonly IOtpService _otpService;

    public VerifyOtpCommandHandler(
        IUserDbContext context,
        IJwtService jwtService,
        IOtpService otpService)
    {
        _context = context;
        _jwtService = jwtService;
        _otpService = otpService;
    }

    public async Task<Result<LoginResponse>> Handle(VerifyOtpCommand request, CancellationToken cancellationToken)
    {
        if (!_otpService.ValidateOtp(request.Email, request.Otp))
            return Result<LoginResponse>.Failure("Invalid or expired OTP");

        var user = await _context.Users
            .FirstOrDefaultAsync(x => x.Email == request.Email, cancellationToken);

        if (user == null)
            return Result<LoginResponse>.Failure("User not found");

        _otpService.ClearOtp(request.Email);

        var accessToken = _jwtService.GenerateToken(user.Id, user.Email, user.Role);
        var refreshToken = _jwtService.GenerateRefreshToken();

        user.RefreshToken = refreshToken;
        user.RefreshTokenExpiryTime = DateTime.UtcNow.AddDays(7);

        await _context.SaveChangesAsync(cancellationToken);

        return Result<LoginResponse>.Success(new LoginResponse
        {
            AccessToken = accessToken,
            RefreshToken = refreshToken,
            ExpiresIn = 3600
        }, "OTP verified successfully.");
    }
}
