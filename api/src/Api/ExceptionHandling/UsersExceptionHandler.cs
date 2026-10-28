using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Users.Services;

namespace Api.ExceptionHandling;

public sealed class UsersExceptionHandler(ILogger<UsersExceptionHandler> logger) : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(HttpContext httpContext, Exception exception, CancellationToken cancellationToken)
    {
        var (status, title, detail) = exception switch
        {
            DuplicateEmailException => (
                (int?)StatusCodes.Status409Conflict,
                "Correo ya registrado",
                (string?)"Ya existe una cuenta registrada con ese correo."),
            ArgumentException => (
                (int?)StatusCodes.Status400BadRequest,
                "Datos inválidos",
                (string?)"Uno de los campos enviados no cumple con el formato requerido."),
            _ => ((int?)null, (string?)null, (string?)null)
        };

        if (status is null)
            return false;

        logger.LogWarning(exception, "Excepción de Users mapeada en {Path}", httpContext.Request.Path);

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
