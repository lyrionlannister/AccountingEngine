namespace Accounting.Domain.Enums;

public enum AuditOperation
{
    JournalEntryPosted,
    JournalEntryReversed,
    PeriodClosed,
    PeriodReopenRequested,
    PeriodReopenResolved,
    AccountBalanceRecalculated
}
