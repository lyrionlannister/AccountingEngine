using Api.Contracts;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;

namespace Api.ExceptionHandling;

public sealed class ApiContractExceptionHandler(ILogger<ApiContractExceptionHandler> logger) : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(HttpContext httpContext, Exception exception, CancellationToken cancellationToken)
    {
        var (status, title, detail) = exception switch
        {
            InvalidPaginationException ex => (
                StatusCodes.Status400BadRequest,
                "Paginación inválida",
                (string?)ex.Message),
            _ => ((int?)null, (string?)null, (string?)null)
        };

        if (status is null)
            return false;

        logger.LogWarning(exception, "Excepción de contrato de API mapeada en {Path}", httpContext.Request.Path);

        httpContext.Response.StatusCode = status.Value;
        await httpContext.Response.WriteAsJsonAsync(new ProblemDetails
        {
            Status = status,
            Title = title,
            Detail = detail
        }, cancellationToken);

        return true;
    }
}
