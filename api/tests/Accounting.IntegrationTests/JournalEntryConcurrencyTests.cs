using Accounting.Domain;
using Accounting.Domain.Enums;
using Accounting.DTOs;
using Accounting.Repositories;
using Accounting.Services;

namespace Accounting.IntegrationTests;

[Collection(PostgresCollection.Name)]
public sealed class JournalEntryConcurrencyTests : IAsyncLifetime
{
    private readonly PostgresContainerFixture _fixture;
    private string _postgres = null!;

    private const long CompanyId = 1;

    public JournalEntryConcurrencyTests(PostgresContainerFixture fixture) => _fixture = fixture;

    public async Task InitializeAsync() => _postgres = await _fixture.CreateDatabaseAsync();

    public Task DisposeAsync() => Task.CompletedTask;

    private sealed record Fixture(long VoucherTypeId, long PeriodId, long DebitAccountId, long CreditAccountId, long CurrencyId);

    private static async Task<Fixture> SeedAsync(TestDatabase db, VoucherNumberingMode numberingMode = VoucherNumberingMode.Automatic)
    {
        using var context = db.NewContext();

        var currency = Currency.Create("USD", "US Dollar", 2);
        context.Currencies.Add(currency);

        var debitAccount = Account.Create(CompanyId, "1000", "Cash", AccountNature.Debit, AccountType.Asset, null, true);
        var creditAccount = Account.Create(CompanyId, "4000", "Revenue", AccountNature.Credit, AccountType.Revenue, null, true);
        context.Accounts.Add(debitAccount);
        context.Accounts.Add(creditAccount);

        var voucherType = VoucherType.Create(CompanyId, "GEN", "General", numberingMode);
        context.VoucherTypes.Add(voucherType);

        var period = AccountingPeriod.Create(CompanyId, "2026-01", new DateOnly(2026, 1, 1), new DateOnly(2026, 1, 31));
        context.AccountingPeriods.Add(period);

        await context.SaveChangesAsync();

        return new Fixture(voucherType.Id, period.Id, debitAccount.Id, creditAccount.Id, currency.Id);
    }

    private static PostJournalEntryRequest BuildRequest(Fixture fixture, int index) => new(
        CompanyId,
        fixture.VoucherTypeId,
        fixture.PeriodId,
        new DateOnly(2026, 1, 15),
        $"Concurrent entry {index}",
        "tester",
        new List<JournalEntryLineRequest>
        {
            new(fixture.DebitAccountId, null, null, fixture.CurrencyId, 100m, 0m),
            new(fixture.CreditAccountId, null, null, fixture.CurrencyId, 0m, 100m)
        },
        SourceModule: "test",
        SourceReference: $"CONC-{index}");

    private static async Task<bool> TryPostAsync(TestDatabase db, PostJournalEntryRequest request, List<long?> sequenceNumbers)
    {
        using var context = db.NewContext();
        var service = new PostJournalEntryService(
            new JournalEntryRepository(context),
            new AccountingPeriodRepository(context),
            new VoucherTypeRepository(context),
            new AccountRepository(context),
            new CostCenterRepository(context),
            new PartyRepository(context), new AuditTrailWriter(context));

        try
        {
            var result = await service.PostAsync(request, CancellationToken.None);
            lock (sequenceNumbers)
                sequenceNumbers.Add(result.SequenceNumber);
            return true;
        }
        catch (AccountingPeriodClosedException)
        {
            return false;
        }
    }

    [Fact]
    public async Task PostAsync_WithConcurrentAutomaticPosts_AssignsSequentialSequenceNumbersWithoutCollisionOrGap()
    {
        await using var db = await TestDatabase.CreateAsync(_postgres);
        var fixture = await SeedAsync(db);

        const int concurrentPosts = 8;
        var sequenceNumbers = new List<long?>();

        var tasks = Enumerable.Range(0, concurrentPosts)
            .Select(i => TryPostAsync(db, BuildRequest(fixture, i), sequenceNumbers))
            .ToArray();

        var results = await Task.WhenAll(tasks);

        Assert.All(results, Assert.True);
        Assert.Equal(concurrentPosts, sequenceNumbers.Count);
        Assert.Equal(
            Enumerable.Range(1, concurrentPosts).Select(n => (long?)n).OrderBy(n => n),
            sequenceNumbers.OrderBy(n => n));
    }

    [Fact]
    public async Task PostAsync_RacingAgainstClosePeriod_NeverPostsIntoAnAlreadyClosedPeriod()
    {
        await using var db = await TestDatabase.CreateAsync(_postgres);
        var fixture = await SeedAsync(db);

        const int concurrentPosts = 6;
        var sequenceNumbers = new List<long?>();

        var postTasks = Enumerable.Range(0, concurrentPosts)
            .Select(i => TryPostAsync(db, BuildRequest(fixture, i), sequenceNumbers))
            .ToArray();

        var closeTask = Task.Run(async () =>
        {
            using var context = db.NewContext();
            var service = new CloseAccountingPeriodService(new AccountingPeriodRepository(context), new JournalEntryRepository(context));
            await service.CloseAsync(new CloseAccountingPeriodRequest(CompanyId, fixture.PeriodId, "closer"), CancellationToken.None);
        });

        await Task.WhenAll(postTasks.Cast<Task>().Append(closeTask));

        var successfulPosts = postTasks.Count(t => t.Result);

        using var verifyContext = db.NewContext();
        var period = verifyContext.AccountingPeriods.Single(p => p.Id == fixture.PeriodId);
        Assert.Equal(PeriodStatus.Closed, period.Status);

        var postedEntryCount = verifyContext.JournalEntries.Count(e => e.AccountingPeriodId == fixture.PeriodId);
        Assert.Equal(successfulPosts, postedEntryCount);
        Assert.Equal(successfulPosts, sequenceNumbers.Count);
        Assert.Equal(
            Enumerable.Range(1, successfulPosts).Select(n => (long?)n).OrderBy(n => n),
            sequenceNumbers.OrderBy(n => n));
    }
}
