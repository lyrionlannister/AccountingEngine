using Auth.Services;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;

namespace Api.ExceptionHandling;

// Primer IExceptionHandler del proyecto — Auth es el primer módulo con Controllers reales.
// Cubre a propósito solo las excepciones que Auth lanza hoy (CLAUDE.md §11: no revelar
// información sensible); el catch-all nunca expone el tipo ni el mensaje interno de C#.
public sealed class AuthExceptionHandler(ILogger<AuthExceptionHandler> logger) : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(HttpContext httpContext, Exception exception, CancellationToken cancellationToken)
    {
        var (status, title, detail) = exception switch
        {
            InvalidCredentialsException => (StatusCodes.Status401Unauthorized, "Credenciales inválidas", (string?)null),
            SessionConflictException => (
                StatusCodes.Status409Conflict,
                "Ya existe una sesión activa",
                "Ya existe una sesión activa. Cierra sesión en tu otro dispositivo para iniciar aquí."),
            _ => (StatusCodes.Status500InternalServerError, "Error interno", (string?)null)
        };

        if (status == StatusCodes.Status500InternalServerError)
            logger.LogError(exception, "Excepción no mapeada en {Path}", httpContext.Request.Path);

        httpContext.Response.StatusCode = status;
        await httpContext.Response.WriteAsJsonAsync(new ProblemDetails
        {
            Status = status,
            Title = title,
            Detail = detail
        }, cancellationToken);

        return true;
    }
}
