using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Backend.Contracts;
using Backend.Data;
using Backend.Infrastructure;
using Backend.Models;

namespace Backend.Controllers;

[ApiController]
[Route("api/auth")]
public sealed class AuthController(
    AppDbContext dbContext,
    JwtTokenService jwtTokenService,
    IOptions<JwtOptions> jwtOptions) : ControllerBase
{
    private readonly JwtOptions _jwtOptions = jwtOptions.Value;

    [HttpPost("register")]
    public async Task<ActionResult<RegisterResponse>> Register(
        [FromBody] RegisterRequest request,
        CancellationToken cancellationToken)
    {
        var normalizedLogin = request.Login.Trim();
        var normalizedFullName = request.FullName.Trim();

        var loginExists = await dbContext.Users
            .AnyAsync(x => x.Login == normalizedLogin, cancellationToken);

        if (loginExists)
        {
            return Conflict(new { message = "Пользователь с таким логином уже существует." });
        }

        var user = new User
        {
            UserId = Guid.NewGuid(),
            Login = normalizedLogin,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password),
            FullName = normalizedFullName,
            Role = UserRole.employee,
            CreatedAt = DateTimeOffset.UtcNow,
            Settings = new UserSettings
            {
                WorkTime = TimeSpan.FromHours(8.5),
                Theme = "light"
            }
        };

        dbContext.Users.Add(user);
        await dbContext.SaveChangesAsync(cancellationToken);

        return Ok(new RegisterResponse(
            user.UserId,
            user.Login,
            user.FullName,
            user.Role,
            user.CreatedAt));
    }

    [HttpPost("login")]
    public async Task<ActionResult<LoginResponse>> Login(
        [FromBody] LoginRequest request,
        CancellationToken cancellationToken)
    {
        var normalizedLogin = request.Login.Trim();

        var user = await dbContext.Users
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Login == normalizedLogin, cancellationToken);

        if (user is null || !BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash))
        {
            return Unauthorized(new { message = "Неверный логин или пароль." });
        }

        var accessToken = jwtTokenService.CreateAccessToken(user);
        var refreshToken = jwtTokenService.CreateRefreshToken();
        var refreshTokenHash = jwtTokenService.HashRefreshToken(refreshToken);

        dbContext.RefreshTokens.Add(new RefreshToken
        {
            TokenId = Guid.NewGuid(),
            UserId = user.UserId,
            TokenHash = refreshTokenHash,
            ExpiresAt = DateTimeOffset.UtcNow.AddDays(_jwtOptions.RefreshTokenDays),
            CreatedAt = DateTimeOffset.UtcNow
        });

        await dbContext.SaveChangesAsync(cancellationToken);

        return Ok(new LoginResponse(
            accessToken,
            refreshToken,
            "Bearer",
            _jwtOptions.AccessTokenMinutes * 60,
            new UserDto(
                user.UserId,
                user.Login,
                user.FullName,
                user.Role,
                user.CreatedAt)));
    }
}