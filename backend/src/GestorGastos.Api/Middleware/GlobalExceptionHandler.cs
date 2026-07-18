using FluentValidation;
using GestorGastos.Domain.Common;
using Microsoft.AspNetCore.Diagnostics;

namespace GestorGastos.Api.Middleware;

/// <summary>Translates unhandled exceptions into the API's ProblemDetails error contract.</summary>
public class GlobalExceptionHandler(ILogger<GlobalExceptionHandler> logger) : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(HttpContext httpContext, Exception exception, CancellationToken cancellationToken)
    {
        switch (exception)
        {
            case ValidationException validationException:
                var errors = validationException.Errors
                    .GroupBy(e => ToCamelCase(e.PropertyName))
                    .ToDictionary(g => g.Key, g => g.Select(e => e.ErrorMessage).ToArray());
                await ProblemDetailsWriter.WriteAsync(
                    httpContext, StatusCodes.Status400BadRequest, "Validation failed",
                    "One or more fields are invalid.", errors, cancellationToken);
                return true;

            case NotFoundException notFound:
                await ProblemDetailsWriter.WriteAsync(
                    httpContext, StatusCodes.Status404NotFound, "Not Found", notFound.Message, cancellationToken: cancellationToken);
                return true;

            case ConflictException conflict:
                await ProblemDetailsWriter.WriteAsync(
                    httpContext, StatusCodes.Status409Conflict, "Conflict", conflict.Message, cancellationToken: cancellationToken);
                return true;

            case InvalidCredentialsException invalidCredentials:
                await ProblemDetailsWriter.WriteAsync(
                    httpContext, StatusCodes.Status401Unauthorized, "Unauthorized", invalidCredentials.Message, cancellationToken: cancellationToken);
                return true;

            default:
                logger.LogError(exception, "Unhandled exception processing {Method} {Path}", httpContext.Request.Method, httpContext.Request.Path);
                await ProblemDetailsWriter.WriteAsync(
                    httpContext, StatusCodes.Status500InternalServerError, "Internal Server Error",
                    "An unexpected error occurred.", cancellationToken: cancellationToken);
                return true;
        }
    }

    private static string ToCamelCase(string propertyName) =>
        string.IsNullOrEmpty(propertyName) ? propertyName : char.ToLowerInvariant(propertyName[0]) + propertyName[1..];
}
