using Microsoft.AspNetCore.Mvc;

namespace GestorGastos.Api.Middleware;

/// <summary>Builds RFC 7807 problem details matching the API's error contract.</summary>
public static class ProblemDetailsWriter
{
    public static async Task WriteAsync(
        HttpContext context,
        int status,
        string title,
        string detail,
        IDictionary<string, string[]>? errors = null,
        CancellationToken cancellationToken = default)
    {
        var problem = new ProblemDetails
        {
            Type = $"https://httpstatuses.io/{status}",
            Title = title,
            Status = status,
            Detail = detail,
        };

        if (errors is { Count: > 0 })
            problem.Extensions["errors"] = errors;

        context.Response.StatusCode = status;
        context.Response.ContentType = "application/problem+json";
        await context.Response.WriteAsJsonAsync(problem, cancellationToken);
    }
}
