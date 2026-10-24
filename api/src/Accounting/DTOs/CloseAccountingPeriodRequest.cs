namespace Accounting.DTOs;

public sealed record CloseAccountingPeriodRequest(long CompanyId, long AccountingPeriodId, string ClosedBy);

public sealed record AccountingPeriodResult(long Id, Guid PublicId, string Status, DateTimeOffset? ClosedAt, string? ClosedBy);
