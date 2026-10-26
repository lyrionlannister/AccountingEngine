using Accounting.Domain;
using Accounting.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Npgsql;

namespace Accounting.IntegrationTests;

[Collection(PostgresCollection.Name)]
public sealed class AuditTrailFixupTests : IAsyncLifetime
{
    private readonly PostgresContainerFixture _fixture;
    private string _postgres = null!;

    public AuditTrailFixupTests(PostgresContainerFixture fixture) => _fixture = fixture;

    public async Task InitializeAsync() => _postgres = await _fixture.CreateDatabaseAsync();

    public Task DisposeAsync() => Task.CompletedTask;

    [Fact]
    public async Task SaveChanges_WithNewJournalEntryAndAuditEntryInSameCall_ResolvesForeignKeyWithoutSecondRoundTrip()
    {
        await using var database = await TestDatabase.CreateAsync(_postgres);
        await using var context = database.NewContext();

        const long companyId = 1;

        var currency = Currency.Create("USD", "US Dollar", 2);
        context.Currencies.Add(currency);

        var debitAccount = Account.Create(companyId, "1000", "Cash", AccountNature.Debit, AccountType.Asset, null, true);
        var creditAccount = Account.Create(companyId, "4000", "Revenue", AccountNature.Credit, AccountType.Revenue, null, true);
        context.Accounts.Add(debitAccount);
        context.Accounts.Add(creditAccount);

        var voucherType = VoucherType.Create(companyId, "GEN", "General", VoucherNumberingMode.Manual);
        context.VoucherTypes.Add(voucherType);

        var period = AccountingPeriod.Create(companyId, "2026-01", new DateOnly(2026, 1, 1), new DateOnly(2026, 1, 31));
        context.AccountingPeriods.Add(period);

        await context.SaveChangesAsync();

        var entry = JournalEntry.Create(companyId, voucherType.Id, period.Id, new DateOnly(2026, 1, 15), "Fixup test");
        entry.AddLine(debitAccount.Id, null, null, currency.Id, 100m, 0m);
        entry.AddLine(creditAccount.Id, null, null, currency.Id, 0m, 100m);
        entry.AssignSequenceNumber(1);
        entry.Post(DateTimeOffset.UtcNow, "tester");

        var auditEntry = AuditTrailEntry.ForJournalEntry(entry, AuditOperation.JournalEntryPosted, companyId, "tester", DateTimeOffset.UtcNow);

        context.JournalEntries.Add(entry);
        context.AuditTrailEntries.Add(auditEntry);

        await context.SaveChangesAsync();

        Assert.True(entry.Id > 0);
        Assert.Equal(entry.Id, auditEntry.JournalEntryId);

        var persisted = await context.AuditTrailEntries.AsNoTracking().SingleAsync(a => a.CompanyId == companyId);
        Assert.Equal(entry.Id, persisted.JournalEntryId);
        Assert.Equal(AuditOperation.JournalEntryPosted, persisted.Operation);
    }

    [Fact]
    public async Task SaveChanges_WithZeroOrMultipleReferences_FailsOnDatabaseCheckConstraint()
    {
        await using var database = await TestDatabase.CreateAsync(_postgres);
        await using var context = database.NewContext();

        const long companyId = 1;
        var period = AccountingPeriod.Create(companyId, "2026-01", new DateOnly(2026, 1, 1), new DateOnly(2026, 1, 31));
        context.AccountingPeriods.Add(period);
        await context.SaveChangesAsync();

        var validEntry = AuditTrailEntry.ForAccountingPeriod(period, AuditOperation.PeriodClosed, companyId, "tester", DateTimeOffset.UtcNow);
        context.AuditTrailEntries.Add(validEntry);
        await context.SaveChangesAsync();

        await Assert.ThrowsAsync<PostgresException>(async () =>
        {
            await context.Database.ExecuteSqlInterpolatedAsync(
                $"INSERT INTO auditoria_contable (empresa_id, operacion, actor, ocurrido_en) VALUES ({companyId}, 'PeriodClosed', 'tester', now())");
        });
    }
}
