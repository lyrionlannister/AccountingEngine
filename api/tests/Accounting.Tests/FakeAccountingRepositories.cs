using Accounting.Domain;
using Accounting.Domain.Enums;
using Accounting.Services;

namespace Accounting.Tests;

internal sealed class FakeJournalEntryRepository : IJournalEntryRepository
{
    public List<JournalEntry> Entries { get; } = new();

    public Task<JournalEntry?> FindByIdAsync(long companyId, long id, CancellationToken cancellationToken) =>
        Task.FromResult(Entries.FirstOrDefault(e => e.CompanyId == companyId && e.Id == id));

    public Task<JournalEntry?> FindByPublicIdAsync(long companyId, Guid publicId, CancellationToken cancellationToken) =>
        Task.FromResult(Entries.FirstOrDefault(e => e.CompanyId == companyId && e.PublicId == publicId));

    public Task<JournalEntry?> FindBySourceAsync(long companyId, string? sourceModule, string sourceReference, CancellationToken cancellationToken) =>
        Task.FromResult(Entries.FirstOrDefault(e =>
            e.CompanyId == companyId && e.SourceModule == sourceModule && e.SourceReference == sourceReference));

    public Task<bool> HasDraftEntriesAsync(long companyId, long accountingPeriodId, CancellationToken cancellationToken) =>
        Task.FromResult(Entries.Any(e =>
            e.CompanyId == companyId && e.AccountingPeriodId == accountingPeriodId && e.Status == JournalEntryStatus.Draft));

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

    public Task<AccountingPeriod?> FindByPublicIdAsync(long companyId, Guid publicId, CancellationToken cancellationToken) =>
        Task.FromResult(Periods.FirstOrDefault(p => p.CompanyId == companyId && p.PublicId == publicId));

    public Task UpdateAsync(AccountingPeriod period, CancellationToken cancellationToken) => Task.CompletedTask;
}

internal sealed class FakeVoucherTypeRepository : IVoucherTypeRepository
{
    public List<VoucherType> VoucherTypes { get; } = new();

    public Task<VoucherType?> FindByIdAsync(long companyId, long id, CancellationToken cancellationToken) =>
        Task.FromResult(VoucherTypes.FirstOrDefault(v => v.CompanyId == companyId && v.Id == id));

    public Task<VoucherType?> FindByPublicIdAsync(long companyId, Guid publicId, CancellationToken cancellationToken) =>
        Task.FromResult(VoucherTypes.FirstOrDefault(v => v.CompanyId == companyId && v.PublicId == publicId));
}

internal sealed class FakeAccountRepository : IAccountRepository
{
    public List<Account> Accounts { get; } = new();

    public Task<IReadOnlyList<Account>> FindManyByIdAsync(long companyId, IReadOnlyCollection<long> ids, CancellationToken cancellationToken) =>
        Task.FromResult<IReadOnlyList<Account>>(
            Accounts.Where(a => a.CompanyId == companyId && ids.Contains(a.Id)).ToList());

    public Task<Account?> FindByPublicIdAsync(long companyId, Guid publicId, CancellationToken cancellationToken) =>
        Task.FromResult(Accounts.FirstOrDefault(a => a.CompanyId == companyId && a.PublicId == publicId));
}

internal sealed class FakeCostCenterRepository : ICostCenterRepository
{
    public List<CostCenter> CostCenters { get; } = new();

    public Task<IReadOnlyList<CostCenter>> FindManyByIdAsync(long companyId, IReadOnlyCollection<long> ids, CancellationToken cancellationToken) =>
        Task.FromResult<IReadOnlyList<CostCenter>>(
            CostCenters.Where(c => c.CompanyId == companyId && ids.Contains(c.Id)).ToList());

    public Task<CostCenter?> FindByPublicIdAsync(long companyId, Guid publicId, CancellationToken cancellationToken) =>
        Task.FromResult(CostCenters.FirstOrDefault(c => c.CompanyId == companyId && c.PublicId == publicId));
}

internal sealed class FakePartyRepository : IPartyRepository
{
    public List<Party> Parties { get; } = new();

    public Task<IReadOnlyList<Party>> FindManyByIdAsync(long companyId, IReadOnlyCollection<long> ids, CancellationToken cancellationToken) =>
        Task.FromResult<IReadOnlyList<Party>>(
            Parties.Where(p => p.CompanyId == companyId && ids.Contains(p.Id)).ToList());

    public Task<Party?> FindByPublicIdAsync(long companyId, Guid publicId, CancellationToken cancellationToken) =>
        Task.FromResult(Parties.FirstOrDefault(p => p.CompanyId == companyId && p.PublicId == publicId));
}

internal sealed class FakeCurrencyRepository : ICurrencyRepository
{
    public List<Currency> Currencies { get; } = new();

    public Task<Currency?> FindByCodeAsync(string code, CancellationToken cancellationToken) =>
        Task.FromResult(Currencies.FirstOrDefault(c => c.Code == code));
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
