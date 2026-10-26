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

// Membership tiene rol (Admin/Accountant/Viewer) desde que existe, pero ningun caso de uso de
// Accounting lo verificaba — un Viewer podia contabilizar, cerrar periodos y aprobar su propia
// solicitud de reapertura (hallazgo de auditoria). RoleGuard centraliza el chequeo para no
// repetir la logica de comparacion en cada Service.
internal static class RoleGuard
{
    public static void Require(string currentRole, params string[] allowedRoles)
    {
        if (!allowedRoles.Contains(currentRole))
            throw new InsufficientRoleException(currentRole, allowedRoles);
    }
}

public sealed class InsufficientRoleException : Exception
{
    public string CurrentRole { get; }
    public IReadOnlyList<string> AllowedRoles { get; }

    public InsufficientRoleException(string currentRole, IReadOnlyList<string> allowedRoles)
        : base($"Role '{currentRole}' is not allowed; requires one of: {string.Join(", ", allowedRoles)}.")
    {
        CurrentRole = currentRole;
        AllowedRoles = allowedRoles;
    }
}

// Segregacion de funciones: quien resuelve una solicitud de reapertura no puede ser quien la
// pidio, sin importar su rol -- ni siquiera un Admin aprueba su propia solicitud.
public sealed class SelfApprovalNotAllowedException : Exception
{
    public SelfApprovalNotAllowedException()
        : base("A user cannot resolve their own period reopen request.")
    {
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
