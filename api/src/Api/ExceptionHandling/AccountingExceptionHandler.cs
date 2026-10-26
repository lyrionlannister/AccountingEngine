using Accounting.Services;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;

namespace Api.ExceptionHandling;

// Solo mapea las excepciones reales que Accounting.Services lanza hoy (mismo criterio que
// AuthExceptionHandler) — registrado ANTES de AuthExceptionHandler en Program.cs: si éste
// corriera primero, su catch-all (que siempre devuelve true) nunca dejaría llegar la excepción
// aquí. Lo que este handler no reconoce, devuelve false y cae al catch-all genérico de Auth.
//
// AccountingPeriodClosedException/HasDraftEntriesException y la violación de balance de
// JournalEntry.Post() son, ambas, InvalidOperationException sin tipo dedicado para el caso de
// "período no encontrado" en otros Services — por eso NO se mapea InvalidOperationException en
// general (conflaría casos distintos bajo un único código HTTP); solo se mapean los tipos
// concretos listados abajo.
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
