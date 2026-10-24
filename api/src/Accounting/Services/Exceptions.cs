namespace Accounting.Services;

public sealed class AccountingPeriodClosedException : Exception
{
    public long AccountingPeriodId { get; }

    public AccountingPeriodClosedException(long accountingPeriodId)
        : base($"Accounting period {accountingPeriodId} is closed.")
    {
        AccountingPeriodId = accountingPeriodId;
    }
}

public sealed class JournalEntryIdempotencyConflictException : Exception
{
    public string? SourceModule { get; }
    public string SourceReference { get; }

    public JournalEntryIdempotencyConflictException(string? sourceModule, string sourceReference)
        : base($"A journal entry with source '{sourceModule}:{sourceReference}' already exists with different content.")
    {
        SourceModule = sourceModule;
        SourceReference = sourceReference;
    }
}

public sealed class JournalEntrySequenceConflictException : Exception
{
    public long CompanyId { get; }
    public long VoucherTypeId { get; }
    public long SequenceNumber { get; }

    public JournalEntrySequenceConflictException(long companyId, long voucherTypeId, long sequenceNumber)
        : base($"Sequence number {sequenceNumber} is already used for voucher type {voucherTypeId} in company {companyId}.")
    {
        CompanyId = companyId;
        VoucherTypeId = voucherTypeId;
        SequenceNumber = sequenceNumber;
    }
}
