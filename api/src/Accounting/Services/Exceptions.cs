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

// Decisión documentada (tasks/todo.md pedía que quedara explícita, no implícita): cerrar un
// periodo con asientos en Draft los deja huérfanos para siempre (nunca se pueden Post() en un
// periodo cerrado). En vez de ignorarlo silenciosamente, se bloquea el cierre — el caller debe
// contabilizar o descartar (Void) esos borradores primero.
public sealed class AccountingPeriodHasDraftEntriesException : Exception
{
    public long AccountingPeriodId { get; }

    public AccountingPeriodHasDraftEntriesException(long accountingPeriodId)
        : base($"Accounting period {accountingPeriodId} has draft journal entries; post or void them before closing.")
    {
        AccountingPeriodId = accountingPeriodId;
    }
}

// Una FK por sí sola solo garantiza que la fila existe, no que es de la misma empresa — ver
// hallazgo de tenant-isolation. Un solo tipo de excepción cubre Account/CostCenter/Party
// porque las tres son el mismo caso (una referencia de línea que no pertenece a la empresa del
// asiento), no tres fallos distintos.
public sealed class JournalEntryLineReferenceNotFoundException : Exception
{
    public string ReferenceKind { get; }
    public long CompanyId { get; }
    public long ReferenceId { get; }

    public JournalEntryLineReferenceNotFoundException(string referenceKind, long companyId, long referenceId)
        : base($"{referenceKind} {referenceId} was not found for company {companyId}.")
    {
        ReferenceKind = referenceKind;
        CompanyId = companyId;
        ReferenceId = referenceId;
    }
}

public sealed class AccountDoesNotAllowPostingException : Exception
{
    public long AccountId { get; }

    public AccountDoesNotAllowPostingException(long accountId)
        : base($"Account {accountId} does not allow posting.")
    {
        AccountId = accountId;
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

public sealed class JournalEntryAlreadyReversedException : Exception
{
    public long JournalEntryId { get; }

    public JournalEntryAlreadyReversedException(long journalEntryId)
        : base($"Journal entry {journalEntryId} was already reversed.")
    {
        JournalEntryId = journalEntryId;
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
