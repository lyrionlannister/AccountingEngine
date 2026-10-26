using Accounting.Domain;
using Accounting.Domain.Enums;
using Accounting.DTOs;
using Accounting.Services;

namespace Accounting.Tests;

public class PostJournalEntryServiceTests
{
    private const long CompanyId = 1;

    private static PostJournalEntryRequest BuildRequest(
        string? sourceReference = "REF-1",
        decimal debit = 100m,
        string description = "Entry") => new(
        CompanyId,
        VoucherTypeId: 0,
        AccountingPeriodId: 0,
        EntryDate: new DateOnly(2026, 1, 15),
        description,
        PostedBy: "tester",
        Lines: new List<JournalEntryLineRequest>
        {
            // Mismo AccountId en ambas líneas a propósito: a los fakes de este archivo no les
            // interesa distinguir cuentas, solo el período/idempotencia — una sola cuenta
            // sembrada (Id por defecto 0) alcanza y es válida para partida doble.
            new(AccountId: 0, null, null, CurrencyId: 1, debit, 0m),
            new(AccountId: 0, null, null, CurrencyId: 1, 0m, debit)
        },
        SourceModule: null,
        SourceReference: sourceReference);

    private static (PostJournalEntryService Service, FakeJournalEntryRepository Entries, FakeAuditTrailWriter AuditTrail) CreateService(
        PeriodStatus periodStatus, VoucherNumberingMode numberingMode, bool seedAccount = true, bool accountAllowsPosting = true, string role = "Admin")
    {
        var entries = new FakeJournalEntryRepository();
        var periods = new FakeAccountingPeriodRepository();
        var voucherTypes = new FakeVoucherTypeRepository();
        var accounts = new FakeAccountRepository();
        var auditTrail = new FakeAuditTrailWriter();

        var period = AccountingPeriod.Create(CompanyId, "2026-01", new DateOnly(2026, 1, 1), new DateOnly(2026, 1, 31));
        if (periodStatus == PeriodStatus.Closed)
            period.Close("closer", DateTimeOffset.UtcNow);
        periods.Periods.Add(period);

        voucherTypes.VoucherTypes.Add(VoucherType.Create(CompanyId, "GEN", "General", numberingMode));
        if (seedAccount)
            accounts.Accounts.Add(Account.Create(CompanyId, "1000", "Cash", AccountNature.Debit, AccountType.Asset, null, accountAllowsPosting));

        return (new PostJournalEntryService(
            entries, periods, voucherTypes, accounts, new FakeCostCenterRepository(), new FakePartyRepository(), auditTrail,
            new FakeCurrentCompanyContext(CompanyId, role)), entries, auditTrail);
    }

    [Fact]
    public async Task PostAsync_WhenPeriodClosed_ThrowsAccountingPeriodClosedException()
    {
        var (service, _, _) = CreateService(PeriodStatus.Closed, VoucherNumberingMode.Automatic);

        await Assert.ThrowsAsync<AccountingPeriodClosedException>(
            () => service.PostAsync(BuildRequest(), CancellationToken.None));
    }

    [Fact]
    public async Task PostAsync_RetriedWithSameContent_DoesNotCreateASecondEntry()
    {
        var (service, entries, auditTrail) = CreateService(PeriodStatus.Open, VoucherNumberingMode.Automatic);

        var first = await service.PostAsync(BuildRequest(), CancellationToken.None);
        var second = await service.PostAsync(BuildRequest(), CancellationToken.None);

        Assert.Single(entries.Entries);
        Assert.Equal(first.SequenceNumber, second.SequenceNumber);
        Assert.Equal("Posted", second.Status);
    }

    [Fact]
    public async Task PostAsync_RetriedWithDifferentContent_ThrowsIdempotencyConflict()
    {
        var (service, _, _) = CreateService(PeriodStatus.Open, VoucherNumberingMode.Automatic);

        await service.PostAsync(BuildRequest(debit: 100m), CancellationToken.None);

        await Assert.ThrowsAsync<JournalEntryIdempotencyConflictException>(
            () => service.PostAsync(BuildRequest(debit: 200m), CancellationToken.None));
    }

    [Fact]
    public async Task PostAsync_WithAccountNotBelongingToCompany_ThrowsReferenceNotFound()
    {
        // Antes de este check, una FK por sí sola solo probaba que la cuenta existía en algún
        // lado, no que fuera de la misma empresa que el asiento (hallazgo de tenant-isolation).
        var (service, _, _) = CreateService(PeriodStatus.Open, VoucherNumberingMode.Automatic, seedAccount: false);

        await Assert.ThrowsAsync<JournalEntryLineReferenceNotFoundException>(
            () => service.PostAsync(BuildRequest(), CancellationToken.None));
    }

    [Fact]
    public async Task PostAsync_WithAccountThatDoesNotAllowPosting_ThrowsAccountDoesNotAllowPosting()
    {
        var (service, _, _) = CreateService(PeriodStatus.Open, VoucherNumberingMode.Automatic, accountAllowsPosting: false);

        await Assert.ThrowsAsync<AccountDoesNotAllowPostingException>(
            () => service.PostAsync(BuildRequest(), CancellationToken.None));
    }

    [Fact]
    public async Task PostAsync_OnSuccess_RecordsJournalEntryPostedInAuditTrail()
    {
        var (service, _, auditTrail) = CreateService(PeriodStatus.Open, VoucherNumberingMode.Automatic);

        var result = await service.PostAsync(BuildRequest(), CancellationToken.None);

        var record = Assert.Single(auditTrail.Records);
        Assert.Equal(AuditOperation.JournalEntryPosted, record.Operation);
        Assert.Equal(CompanyId, record.CompanyId);
        Assert.Equal("tester", record.Actor);
        Assert.IsType<JournalEntry>(record.AuditedEntity);
        Assert.Equal(result.PublicId, ((JournalEntry)record.AuditedEntity).PublicId);
    }

    [Fact]
    public async Task PostAsync_AsViewer_ThrowsInsufficientRole()
    {
        var (service, _, _) = CreateService(PeriodStatus.Open, VoucherNumberingMode.Automatic, role: "Viewer");

        await Assert.ThrowsAsync<InsufficientRoleException>(() => service.PostAsync(BuildRequest(), CancellationToken.None));
    }
}
