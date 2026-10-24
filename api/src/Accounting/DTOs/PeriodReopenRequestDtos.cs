namespace Accounting.DTOs;

public sealed record RequestPeriodReopenRequest(long CompanyId, long AccountingPeriodId, string Reason, string RequestedBy);

public sealed record ResolvePeriodReopenRequest(long CompanyId, long PeriodReopenRequestId, bool Approve, string ResolvedBy, string? Notes = null);

public sealed record PeriodReopenRequestResult(
    long Id,
    Guid PublicId,
    string Status,
    string RequestedBy,
    DateTimeOffset RequestedAt,
    string? ResolvedBy,
    DateTimeOffset? ResolvedAt,
    string? ResolutionNotes);
