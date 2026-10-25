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

        // Auditoría de seguridad (CLAUDE.md §14): sin esto, intentos de login fallidos y
        // conflictos de sesión no dejaban rastro — ni para detectar credential stuffing ni
        // para alertar sobre un posible secuestro de cuenta. Nunca se registra la contraseña.
        switch (exception)
        {
            case InvalidCredentialsException:
                logger.LogWarning(
                    "Intento de login fallido desde {RemoteIp}", httpContext.Connection.RemoteIpAddress);
                break;
            case SessionConflictException conflict:
                logger.LogWarning(
                    "Login rechazado por conflicto de sesión: usuario {UserId} ya tiene sesión activa, IP rechazada {RejectedIp}",
                    conflict.UserId, conflict.RejectedIp);
                break;
            default:
                logger.LogError(exception, "Excepción no mapeada en {Path}", httpContext.Request.Path);
                break;
        }

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
