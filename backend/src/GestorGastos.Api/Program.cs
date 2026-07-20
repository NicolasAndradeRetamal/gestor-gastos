using System.Globalization;
using System.Text;
using System.Threading.RateLimiting;
using FluentValidation;
using GestorGastos.Api.Auth;
using GestorGastos.Api.Middleware;
using GestorGastos.Api.Validators;
using GestorGastos.Infrastructure;
using GestorGastos.Infrastructure.Persistence;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi;

var builder = WebApplication.CreateBuilder(args);

const string DevCorsPolicy = "DevClient";

builder.Services.AddControllers();

builder.Services.AddInfrastructure(builder.Configuration);
builder.Services.AddValidatorsFromAssemblyContaining<RegisterRequestValidator>();
builder.Services.AddScoped<SessionIssuer>();

// The platform (Render) terminates TLS and forwards the real client IP in
// X-Forwarded-For; trust it so rate limiting partitions by the actual client.
builder.Services.Configure<ForwardedHeadersOptions>(options =>
{
    options.ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto;
    options.KnownIPNetworks.Clear();
    options.KnownProxies.Clear();
});

builder.Services.AddRateLimiter(options =>
{
    options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
    options.OnRejected = async (context, token) =>
    {
        if (context.Lease.TryGetMetadata(MetadataName.RetryAfter, out var retryAfter))
            context.HttpContext.Response.Headers.RetryAfter =
                ((int)retryAfter.TotalSeconds).ToString(CultureInfo.InvariantCulture);

        await ProblemDetailsWriter.WriteAsync(
            context.HttpContext,
            StatusCodes.Status429TooManyRequests,
            "Demasiadas solicitudes",
            "Has realizado demasiados intentos. Inténtalo de nuevo más tarde.",
            cancellationToken: token);
    };

    AddFixedWindowPolicy(options, RateLimitPolicies.Register, permitLimit: 5, TimeSpan.FromHours(1));
    AddFixedWindowPolicy(options, RateLimitPolicies.Login, permitLimit: 10, TimeSpan.FromMinutes(5));
    AddFixedWindowPolicy(options, RateLimitPolicies.Refresh, permitLimit: 30, TimeSpan.FromMinutes(5));
    AddFixedWindowPolicy(options, RateLimitPolicies.TwoFactor, permitLimit: 10, TimeSpan.FromMinutes(5));
});

static void AddFixedWindowPolicy(
    Microsoft.AspNetCore.RateLimiting.RateLimiterOptions options, string name, int permitLimit, TimeSpan window) =>
    options.AddPolicy(name, httpContext =>
        RateLimitPartition.GetFixedWindowLimiter(
            partitionKey: httpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown",
            factory: _ => new FixedWindowRateLimiterOptions { PermitLimit = permitLimit, Window = window }));

var jwtSecret = builder.Configuration["Jwt:Secret"];
if (string.IsNullOrWhiteSpace(jwtSecret))
    throw new InvalidOperationException("Missing 'Jwt:Secret' configuration. Set it via environment variables or user-secrets.");

builder.Services
    .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidIssuer = builder.Configuration["Jwt:Issuer"],
            ValidateAudience = true,
            ValidAudience = builder.Configuration["Jwt:Audience"],
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSecret)),
            ValidateLifetime = true,
            ClockSkew = TimeSpan.FromSeconds(30),
        };
        options.Events = new JwtBearerEvents
        {
            // Replaces the default 401 body with the API's ProblemDetails contract.
            OnChallenge = async context =>
            {
                context.HandleResponse();
                await ProblemDetailsWriter.WriteAsync(
                    context.HttpContext,
                    StatusCodes.Status401Unauthorized,
                    "Unauthorized",
                    "El token falta, es inválido o expiró.");
            },
        };
    });

builder.Services.AddAuthorizationBuilder()
    .SetDefaultPolicy(new AuthorizationPolicyBuilder().RequireAuthenticatedUser().Build());

builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
builder.Services.AddProblemDetails();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo { Title = "Gestor de Gastos API", Version = "v1" });

    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "Ingresa el token JWT (sin el prefijo 'Bearer').",
    });
    options.AddSecurityRequirement(_ => new OpenApiSecurityRequirement
    {
        { new OpenApiSecuritySchemeReference("Bearer", null, null), [] },
    });
});

builder.Services.AddCors(options =>
{
    options.AddPolicy(DevCorsPolicy, policy =>
        policy.WithOrigins("http://localhost:5173")
            .AllowAnyHeader()
            .AllowAnyMethod());
});

var app = builder.Build();

app.UseForwardedHeaders();

app.UseExceptionHandler();

app.UseSwagger();
app.UseSwaggerUI();

if (app.Environment.IsDevelopment())
{
    app.UseCors(DevCorsPolicy);
}

app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

app.UseRateLimiter();

app.MapControllers();
app.MapGet("/health", () => Results.Ok(new { status = "ok" })).AllowAnonymous();

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    db.Database.Migrate();
}

app.Run();

/// <summary>Marker for WebApplicationFactory in integration tests.</summary>
public partial class Program;
