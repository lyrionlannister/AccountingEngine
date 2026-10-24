using Accounting.Domain;

namespace Accounting.Services;

public interface IJournalEntryRepository
{
    Task<JournalEntry?> FindByIdAsync(long companyId, long id, CancellationToken cancellationToken);

    Task<JournalEntry?> FindBySourceAsync(long companyId, string? sourceModule, string sourceReference, CancellationToken cancellationToken);

    Task AddAsync(JournalEntry entry, CancellationToken cancellationToken);

    Task AddWithAutomaticSequenceAsync(JournalEntry entry, Action<long> onSequenceReserved, CancellationToken cancellationToken);
}
