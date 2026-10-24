using Accounting.Domain;
using Accounting.Domain.Enums;
using Accounting.DTOs;
using Accounting.Repositories;
using Accounting.Services;
using Microsoft.EntityFrameworkCore;

namespace Accounting.IntegrationTests;

[Collection(PostgresCollection.Name)]
public sealed class AccountBalanceSnapshotReconciliationTests : IAsyncLifetime
{
    private readonly PostgresContainerFixture _fixture;
    private string _postgres = null!;

    private const long CompanyId = 1;

    public AccountBalanceSnapshotReconciliationTests(PostgresContainerFixture fixture) => _fixture = fixture;

    public async Task InitializeAsync() => _postgres = await _fixture.CreateDatabaseAsync();

    public Task DisposeAsync() => Task.CompletedTask;

    [Fact]
    public async Task RecalculateAsync_MatchesIndependentSumOfPostedJournalEntryLines()
    {
        await using var db = await TestDatabase.CreateAsync(_postgres);

        long periodId, cashAccountId, revenueAccountId, currencyId, voucherTypeId;
        using (var seedContext = db.NewContext())
        {
            var currency = Currency.Create("USD", "US Dollar", 2);
            seedContext.Currencies.Add(currency);

            var cash = Account.Create(CompanyId, "1000", "Cash", AccountNature.Debit, AccountType.Asset, null, true);
            var revenue = Account.Create(CompanyId, "4000", "Revenue", AccountNature.Credit, AccountType.Revenue, null, true);
            seedContext.Accounts.Add(cash);
            seedContext.Accounts.Add(revenue);

            var voucherType = VoucherType.Create(CompanyId, "GEN", "General", VoucherNumberingMode.Automatic);
            seedContext.VoucherTypes.Add(voucherType);

            var period = AccountingPeriod.Create(CompanyId, "2026-01", new DateOnly(2026, 1, 1), new DateOnly(2026, 1, 31));
            seedContext.AccountingPeriods.Add(period);

            await seedContext.SaveChangesAsync();

            periodId = period.Id;
            cashAccountId = cash.Id;
            revenueAccountId = revenue.Id;
            currencyId = currency.Id;
            voucherTypeId = voucherType.Id;
        }

        async Task PostAsync(decimal cashDebit, decimal cashCredit, string reference)
        {
            using var context = db.NewContext();
            var service = new PostJournalEntryService(
                new JournalEntryRepository(context),
                new AccountingPeriodRepository(context),
                new VoucherTypeRepository(context));

            var request = new PostJournalEntryRequest(
                CompanyId, voucherTypeId, periodId, new DateOnly(2026, 1, 15), "Recon test", "tester",
                new List<JournalEntryLineRequest>
                {
                    new(cashAccountId, null, null, currencyId, cashDebit, cashCredit),
                    new(revenueAccountId, null, null, currencyId, cashCredit, cashDebit)
                },
                SourceModule: "test",
                SourceReference: reference);

            await service.PostAsync(request, CancellationToken.None);
        }

        await PostAsync(cashDebit: 300m, cashCredit: 0m, reference: "RECON-1");
        await PostAsync(cashDebit: 150m, cashCredit: 0m, reference: "RECON-2");
        await PostAsync(cashDebit: 0m, cashCredit: 50m, reference: "RECON-3");

        using (var draftContext = db.NewContext())
        {
            var draft = JournalEntry.Create(CompanyId, voucherTypeId, periodId, new DateOnly(2026, 1, 20), "Never posted");
            draft.AddLine(cashAccountId, null, null, currencyId, 999m, 0m);
            draft.AddLine(revenueAccountId, null, null, currencyId, 0m, 999m);
            draftContext.JournalEntries.Add(draft);
            await draftContext.SaveChangesAsync();
        }

        using var recalcContext = db.NewContext();
        var snapshots = new AccountBalanceSnapshotRepository(recalcContext);
        var periods = new AccountingPeriodRepository(recalcContext);
        var recalcService = new RecalculateAccountBalanceSnapshotService(periods, snapshots);

        var result = await recalcService.RecalculateAsync(
            new RecalculateAccountBalanceSnapshotRequest(CompanyId, periodId, cashAccountId),
            CancellationToken.None);

        using var verifyContext = db.NewContext();
        var expected = await verifyContext.JournalEntries
            .Where(e => e.CompanyId == CompanyId && e.AccountingPeriodId == periodId && e.Status == JournalEntryStatus.Posted)
            .SelectMany(e => e.Lines)
            .Where(l => l.AccountId == cashAccountId)
            .GroupBy(_ => 1)
            .Select(g => new { Debit = g.Sum(l => l.DebitAmount), Credit = g.Sum(l => l.CreditAmount) })
            .SingleAsync();

        Assert.Equal(expected.Debit, result.DebitTotal);
        Assert.Equal(expected.Credit, result.CreditTotal);
        Assert.Equal(450m, result.DebitTotal);
        Assert.Equal(50m, result.CreditTotal);

        var persistedSnapshot = await verifyContext.AccountBalanceSnapshots
            .SingleAsync(s => s.AccountingPeriodId == periodId && s.AccountId == cashAccountId);
        Assert.Equal(result.DebitTotal, persistedSnapshot.DebitTotal);
        Assert.Equal(result.CreditTotal, persistedSnapshot.CreditTotal);
    }
}
