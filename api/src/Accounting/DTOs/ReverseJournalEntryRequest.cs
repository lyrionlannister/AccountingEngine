namespace Accounting.DTOs;

public sealed record ReverseJournalEntryRequest(
    long CompanyId,
    long JournalEntryId,
    DateOnly EntryDate,
    string Description,
    string PostedBy,
    long? SequenceNumber = null);
