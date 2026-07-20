using FluentValidation;
using GestorGastos.Api.Auth;
using GestorGastos.Api.Dtos.Auth;
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
[Route("api/auth/2fa")]
[Authorize]
[EnableRateLimiting(RateLimitPolicies.TwoFactor)]
public class TwoFactorController(
    AppDbContext db,
    IPasswordHasher passwordHasher,
    ITotpService totp,
    ITotpSecretProtector secretProtector,
    IRecoveryCodeService recoveryCodes,
    SessionIssuer sessionIssuer,
    IJwtTokenGenerator jwtTokenGenerator,
    IValidator<TwoFactorEnableRequest> enableValidator,
    IValidator<TwoFactorDisableRequest> disableValidator,
    IValidator<TwoFactorVerifyRequest> verifyValidator) : ControllerBase
{
    private const int RecoveryCodeCount = 10;

    [HttpPost("setup")]
    public async Task<ActionResult<TwoFactorSetupResponse>> Setup(CancellationToken ct)
    {
        var user = await LoadCurrentUserAsync(ct);
        if (user.TwoFactorEnabled)
            throw new ConflictException("La verificación en dos pasos ya está activa.");

        var secret = totp.GenerateSecret();
        user.TwoFactorSecret = secretProtector.Protect(secret);
        await db.SaveChangesAsync(ct);

        return Ok(new TwoFactorSetupResponse(secret, totp.BuildOtpauthUri(user.Email, secret)));
    }

    [HttpPost("enable")]
    public async Task<ActionResult<TwoFactorEnableResponse>> Enable(TwoFactorEnableRequest request, CancellationToken ct)
    {
        await enableValidator.ValidateAndThrowAsync(request, ct);

        var user = await LoadCurrentUserAsync(ct);
        if (user.TwoFactorEnabled)
            throw new ConflictException("La verificación en dos pasos ya está activa.");
        if (user.TwoFactorSecret is null)
            throw new NotFoundException("No hay una configuración de verificación en dos pasos pendiente.");

        var secret = secretProtector.Unprotect(user.TwoFactorSecret);
        if (!totp.VerifyCode(secret, request.Code))
            throw new InvalidCredentialsException("El código no es válido.");

        var generated = recoveryCodes.Generate(RecoveryCodeCount);
        db.TwoFactorRecoveryCodes.AddRange(generated.Select(c => new TwoFactorRecoveryCode
        {
            UserId = user.Id,
            CodeHash = c.Hash,
        }));

        user.TwoFactorEnabled = true;
        user.TwoFactorEnabledAt = DateTimeOffset.UtcNow;
        await db.SaveChangesAsync(ct);

        return Ok(new TwoFactorEnableResponse(generated.Select(c => c.PlainText).ToList()));
    }

    [HttpPost("disable")]
    public async Task<IActionResult> Disable(TwoFactorDisableRequest request, CancellationToken ct)
    {
        await disableValidator.ValidateAndThrowAsync(request, ct);

        var user = await LoadCurrentUserAsync(ct);
        if (!user.TwoFactorEnabled || user.TwoFactorSecret is null)
            throw new ConflictException("La verificación en dos pasos no está activa.");

        var passwordValid = passwordHasher.Verify(request.Password, user.PasswordHash);
        var codeValid = await IsSecondFactorValidAsync(user, request.Code, consume: false, ct);
        if (!passwordValid || !codeValid)
            throw new InvalidCredentialsException("La contraseña o el código no son correctos.");

        var codes = await db.TwoFactorRecoveryCodes.Where(c => c.UserId == user.Id).ToListAsync(ct);
        foreach (var code in codes)
            code.Active = false;

        user.TwoFactorEnabled = false;
        user.TwoFactorSecret = null;
        user.TwoFactorEnabledAt = null;
        await db.SaveChangesAsync(ct);

        return NoContent();
    }

    [HttpPost("verify")]
    [AllowAnonymous]
    public async Task<ActionResult<AuthResponse>> Verify(TwoFactorVerifyRequest request, CancellationToken ct)
    {
        await verifyValidator.ValidateAndThrowAsync(request, ct);

        var userId = jwtTokenGenerator.ValidateTwoFactorChallenge(request.TwoFactorToken);
        if (userId is null)
            throw new InvalidCredentialsException("El desafío expiró o no es válido. Inicia sesión de nuevo.");

        var user = await db.Users.FirstOrDefaultAsync(u => u.Id == userId.Value, ct);
        if (user is null || !user.TwoFactorEnabled || user.TwoFactorSecret is null)
            throw new InvalidCredentialsException("El desafío expiró o no es válido. Inicia sesión de nuevo.");

        if (!await IsSecondFactorValidAsync(user, request.Code, consume: true, ct))
            throw new InvalidCredentialsException("El código no es válido.");

        await db.SaveChangesAsync(ct);

        var response = await sessionIssuer.IssueAsync(user, HttpContext.GetClientIp(), ct);
        return Ok(response);
    }

    private async Task<User> LoadCurrentUserAsync(CancellationToken ct)
    {
        var userId = User.GetId();
        return await db.Users.FirstOrDefaultAsync(u => u.Id == userId, ct)
            ?? throw new InvalidCredentialsException("El usuario del token ya no existe.");
    }

    /// <summary>Validates a TOTP code or, failing that, an unused recovery code (consumed on success).</summary>
    private async Task<bool> IsSecondFactorValidAsync(User user, string code, bool consume, CancellationToken ct)
    {
        var secret = secretProtector.Unprotect(user.TwoFactorSecret!);
        if (totp.VerifyCode(secret, code))
            return true;

        var hash = recoveryCodes.HashInput(code);
        var match = await db.TwoFactorRecoveryCodes
            .FirstOrDefaultAsync(c => c.UserId == user.Id && c.CodeHash == hash && c.UsedAt == null, ct);
        if (match is null)
            return false;

        if (consume)
            match.UsedAt = DateTimeOffset.UtcNow;

        return true;
    }
}
