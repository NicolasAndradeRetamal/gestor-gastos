using FluentValidation;
using GestorGastos.Api.Auth;
using GestorGastos.Api.Dtos.Auth;
using GestorGastos.Api.Mapping;
using GestorGastos.Domain.Common;
using GestorGastos.Domain.Entities;
using GestorGastos.Infrastructure.Auth;
using GestorGastos.Infrastructure.Persistence;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;

namespace GestorGastos.Api.Controllers;

[ApiController]
[Route("api/auth")]
[Authorize]
public class AuthController(
    AppDbContext db,
    IPasswordHasher passwordHasher,
    IJwtTokenGenerator jwtTokenGenerator,
    IRefreshTokenService refreshTokenService,
    SessionIssuer sessionIssuer,
    IValidator<RegisterRequest> registerValidator,
    IValidator<LoginRequest> loginValidator,
    IValidator<RefreshRequest> refreshValidator,
    IValidator<LogoutRequest> logoutValidator) : ControllerBase
{
    private const string InvalidCredentials = "Correo o contraseña incorrectos.";

    [HttpPost("register")]
    [AllowAnonymous]
    [EnableRateLimiting(RateLimitPolicies.Register)]
    public async Task<ActionResult<AuthResponse>> Register(RegisterRequest request, CancellationToken ct)
    {
        await registerValidator.ValidateAndThrowAsync(request, ct);

        var email = request.Email.Trim().ToLowerInvariant();
        var emailInUse = await db.Users.AnyAsync(u => u.Email == email, ct);
        if (emailInUse)
            throw new ConflictException("Ya existe una cuenta con este correo.");

        var user = new User
        {
            Email = email,
            PasswordHash = passwordHasher.Hash(request.Password),
            DisplayName = request.DisplayName.Trim(),
        };

        db.Users.Add(user);
        await db.SaveChangesAsync(ct);

        var response = await sessionIssuer.IssueAsync(user, HttpContext.GetClientIp(), ct);
        return CreatedAtAction(nameof(Me), null, response);
    }

    [HttpPost("login")]
    [AllowAnonymous]
    [EnableRateLimiting(RateLimitPolicies.Login)]
    public async Task<ActionResult> Login(LoginRequest request, CancellationToken ct)
    {
        await loginValidator.ValidateAndThrowAsync(request, ct);

        var email = request.Email.Trim().ToLowerInvariant();
        var user = await db.Users.FirstOrDefaultAsync(u => u.Email == email, ct);
        var now = DateTimeOffset.UtcNow;

        // Locked accounts are rejected without checking the password; the message
        // stays generic so it never reveals that the account exists or is locked.
        if (user is null || user.IsLockedOut(now))
            throw new InvalidCredentialsException(InvalidCredentials);

        if (!passwordHasher.Verify(request.Password, user.PasswordHash))
        {
            user.RegisterFailedLogin(now);
            await db.SaveChangesAsync(ct);
            throw new InvalidCredentialsException(InvalidCredentials);
        }

        user.RegisterSuccessfulLogin();
        await db.SaveChangesAsync(ct);

        if (user.TwoFactorEnabled)
        {
            var challenge = jwtTokenGenerator.GenerateTwoFactorChallenge(user);
            return Ok(new TwoFactorChallengeResponse(true, challenge.Value));
        }

        var response = await sessionIssuer.IssueAsync(user, HttpContext.GetClientIp(), ct);
        return Ok(response);
    }

    [HttpPost("refresh")]
    [AllowAnonymous]
    [EnableRateLimiting(RateLimitPolicies.Refresh)]
    public async Task<ActionResult<RefreshResponse>> Refresh(RefreshRequest request, CancellationToken ct)
    {
        await refreshValidator.ValidateAndThrowAsync(request, ct);

        var rotation = await refreshTokenService.RotateAsync(request.RefreshToken, HttpContext.GetClientIp(), ct);
        var access = jwtTokenGenerator.Generate(rotation.User);

        return Ok(new RefreshResponse(access.Value, access.ExpiresAt, rotation.Refresh.Token, rotation.Refresh.ExpiresAt));
    }

    [HttpPost("logout")]
    [AllowAnonymous]
    [EnableRateLimiting(RateLimitPolicies.Refresh)]
    public async Task<IActionResult> Logout(LogoutRequest request, CancellationToken ct)
    {
        await logoutValidator.ValidateAndThrowAsync(request, ct);

        await refreshTokenService.RevokeFamilyAsync(request.RefreshToken, ct);
        return NoContent();
    }

    [HttpGet("me")]
    public async Task<ActionResult<UserDto>> Me(CancellationToken ct)
    {
        var userId = User.GetId();
        var user = await db.Users.AsNoTracking().FirstOrDefaultAsync(u => u.Id == userId, ct)
            ?? throw new InvalidCredentialsException("El usuario del token ya no existe.");

        return Ok(user.ToDto());
    }
}
