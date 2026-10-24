namespace Accounting.DTOs;

public sealed record PostJournalEntryRequest(
    long CompanyId,
    long VoucherTypeId,
    long AccountingPeriodId,
    DateOnly EntryDate,
    string Description,
    string PostedBy,
    IReadOnlyList<JournalEntryLineRequest> Lines,
    string? SourceModule = null,
    string? SourceReference = null,
    long? SequenceNumber = null);

public sealed record JournalEntryLineRequest(
    long AccountId,
    long? CostCenterId,
    long? PartyId,
    long CurrencyId,
    decimal DebitAmount,
    decimal CreditAmount,
    string? Description = null);

public sealed record JournalEntryResult(
    long Id,
    Guid PublicId,
    long? SequenceNumber,
    string Status,
    DateTimeOffset? PostedAt);
