using Accounting.Services;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;

namespace Api.ExceptionHandling;

public sealed class AccountingExceptionHandler(ILogger<AccountingExceptionHandler> logger) : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(HttpContext httpContext, Exception exception, CancellationToken cancellationToken)
    {
        var (status, title, detail) = exception switch
        {
            AccountingPeriodClosedException => (
                StatusCodes.Status409Conflict,
                "Período contable cerrado",
                "No se puede contabilizar ni revertir un asiento en un período contable cerrado."),
            AccountingPeriodHasDraftEntriesException => (
                StatusCodes.Status409Conflict,
                "El período tiene asientos en borrador",
                "El período contable tiene asientos sin contabilizar o descartar; resuélvelos antes de cerrarlo."),
            JournalEntryLineReferenceNotFoundException => (
                StatusCodes.Status404NotFound,
                "Referencia no encontrada",
                (string?)"Una de las referencias del asiento no existe en la empresa activa."),
            AccountDoesNotAllowPostingException => (
                StatusCodes.Status422UnprocessableEntity,
                "La cuenta no admite contabilización",
                "La cuenta de esta línea no está habilitada para recibir movimientos."),
            JournalEntryIdempotencyConflictException => (
                StatusCodes.Status409Conflict,
                "Conflicto de idempotencia",
                "Ya existe un asiento distinto registrado con la misma referencia de origen."),
            JournalEntrySequenceConflictException => (
                StatusCodes.Status409Conflict,
                "Conflicto de numeración",
                "El número de secuencia ya está en uso para este tipo de comprobante."),
            JournalEntryAlreadyReversedException => (
                StatusCodes.Status409Conflict,
                "El asiento ya fue revertido",
                "Este asiento contable ya tiene una reversión registrada; no se puede revertir dos veces."),
            InsufficientRoleException => (
                StatusCodes.Status403Forbidden,
                "Rol insuficiente",
                "Tu rol en esta empresa no tiene permiso para realizar esta operación."),
            SelfApprovalNotAllowedException => (
                StatusCodes.Status403Forbidden,
                "No puedes resolver tu propia solicitud",
                "Un usuario no puede aprobar ni rechazar una solicitud de reapertura que él mismo creó."),
            _ => ((int?)null, (string?)null, (string?)null)
        };

        if (status is null)
            return false;

        logger.LogWarning(exception, "Excepción de Accounting mapeada en {Path}", httpContext.Request.Path);

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
