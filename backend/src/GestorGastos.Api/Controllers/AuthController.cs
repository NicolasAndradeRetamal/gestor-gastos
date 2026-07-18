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
using Microsoft.EntityFrameworkCore;

namespace GestorGastos.Api.Controllers;

[ApiController]
[Route("api/auth")]
[Authorize]
public class AuthController(
    AppDbContext db,
    IPasswordHasher passwordHasher,
    IJwtTokenGenerator jwtTokenGenerator,
    IValidator<RegisterRequest> registerValidator,
    IValidator<LoginRequest> loginValidator) : ControllerBase
{
    [HttpPost("register")]
    [AllowAnonymous]
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

        var token = jwtTokenGenerator.Generate(user);
        var response = new AuthResponse(token.Value, token.ExpiresAt, user.ToDto());

        return CreatedAtAction(nameof(Me), null, response);
    }

    [HttpPost("login")]
    [AllowAnonymous]
    public async Task<ActionResult<AuthResponse>> Login(LoginRequest request, CancellationToken ct)
    {
        await loginValidator.ValidateAndThrowAsync(request, ct);

        var email = request.Email.Trim().ToLowerInvariant();
        var user = await db.Users.FirstOrDefaultAsync(u => u.Email == email, ct);

        if (user is null || !passwordHasher.Verify(request.Password, user.PasswordHash))
            throw new InvalidCredentialsException("Correo o contraseña incorrectos.");

        var token = jwtTokenGenerator.Generate(user);
        return Ok(new AuthResponse(token.Value, token.ExpiresAt, user.ToDto()));
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
