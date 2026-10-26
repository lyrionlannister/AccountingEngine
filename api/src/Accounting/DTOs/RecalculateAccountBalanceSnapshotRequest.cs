namespace Accounting.DTOs;

public sealed record RecalculateAccountBalanceSnapshotRequest(long CompanyId, long AccountingPeriodId, long AccountId, string TriggeredBy);

public sealed record AccountBalanceSnapshotResult(long AccountingPeriodId, long AccountId, decimal DebitTotal, decimal CreditTotal, DateTimeOffset CalculatedAt);
