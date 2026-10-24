using Accounting.Domain;
using Accounting.Services;

namespace Accounting.Tests;

internal sealed class FakeJournalEntryRepository : IJournalEntryRepository
{
    public List<JournalEntry> Entries { get; } = new();

    public Task<JournalEntry?> FindByIdAsync(long companyId, long id, CancellationToken cancellationToken) =>
        Task.FromResult(Entries.FirstOrDefault(e => e.CompanyId == companyId && e.Id == id));

    public Task<JournalEntry?> FindBySourceAsync(long companyId, string? sourceModule, string sourceReference, CancellationToken cancellationToken) =>
        Task.FromResult(Entries.FirstOrDefault(e =>
            e.CompanyId == companyId && e.SourceModule == sourceModule && e.SourceReference == sourceReference));

    public Task AddAsync(JournalEntry entry, CancellationToken cancellationToken)
    {
        Entries.Add(entry);
        return Task.CompletedTask;
    }

    public Task AddWithAutomaticSequenceAsync(JournalEntry entry, Action<long> onSequenceReserved, CancellationToken cancellationToken)
    {
        var next = Entries
            .Where(e => e.CompanyId == entry.CompanyId && e.VoucherTypeId == entry.VoucherTypeId)
            .Select(e => e.SequenceNumber ?? 0)
            .DefaultIfEmpty(0)
            .Max() + 1;

        onSequenceReserved(next);
        Entries.Add(entry);
        return Task.CompletedTask;
    }
}

internal sealed class FakeAccountingPeriodRepository : IAccountingPeriodRepository
{
    public List<AccountingPeriod> Periods { get; } = new();

    public Task<AccountingPeriod?> FindByIdAsync(long companyId, long id, CancellationToken cancellationToken) =>
        Task.FromResult(Periods.FirstOrDefault(p => p.CompanyId == companyId && p.Id == id));

    public Task UpdateAsync(AccountingPeriod period, CancellationToken cancellationToken) => Task.CompletedTask;
}

internal sealed class FakeVoucherTypeRepository : IVoucherTypeRepository
{
    public List<VoucherType> VoucherTypes { get; } = new();

    public Task<VoucherType?> FindByIdAsync(long companyId, long id, CancellationToken cancellationToken) =>
        Task.FromResult(VoucherTypes.FirstOrDefault(v => v.CompanyId == companyId && v.Id == id));
}

internal sealed class FakeAccountBalanceSnapshotRepository : IAccountBalanceSnapshotRepository
{
    public List<AccountBalanceSnapshot> Snapshots { get; } = new();
    public (decimal DebitTotal, decimal CreditTotal) NextSum { get; set; }

    public Task<AccountBalanceSnapshot?> FindByIdAsync(long accountingPeriodId, long accountId, CancellationToken cancellationToken) =>
        Task.FromResult(Snapshots.FirstOrDefault(s => s.AccountingPeriodId == accountingPeriodId && s.AccountId == accountId));

    public Task<(decimal DebitTotal, decimal CreditTotal)> SumPostedLinesAsync(long companyId, long accountingPeriodId, long accountId, CancellationToken cancellationToken) =>
        Task.FromResult(NextSum);

    public Task AddAsync(AccountBalanceSnapshot snapshot, CancellationToken cancellationToken)
    {
        Snapshots.Add(snapshot);
        return Task.CompletedTask;
    }

    public Task UpdateAsync(AccountBalanceSnapshot snapshot, CancellationToken cancellationToken) => Task.CompletedTask;
}
