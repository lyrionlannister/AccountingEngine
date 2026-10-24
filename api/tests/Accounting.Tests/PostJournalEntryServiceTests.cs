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
            new(AccountId: 1, null, null, CurrencyId: 1, debit, 0m),
            new(AccountId: 2, null, null, CurrencyId: 1, 0m, debit)
        },
        SourceModule: null,
        SourceReference: sourceReference);

    private static (PostJournalEntryService Service, FakeJournalEntryRepository Entries) CreateService(PeriodStatus periodStatus, VoucherNumberingMode numberingMode)
    {
        var entries = new FakeJournalEntryRepository();
        var periods = new FakeAccountingPeriodRepository();
        var voucherTypes = new FakeVoucherTypeRepository();

        var period = AccountingPeriod.Create(CompanyId, "2026-01", new DateOnly(2026, 1, 1), new DateOnly(2026, 1, 31));
        if (periodStatus == PeriodStatus.Closed)
            period.Close("closer", DateTimeOffset.UtcNow);
        periods.Periods.Add(period);

        voucherTypes.VoucherTypes.Add(VoucherType.Create(CompanyId, "GEN", "General", numberingMode));

        return (new PostJournalEntryService(entries, periods, voucherTypes), entries);
    }

    [Fact]
    public async Task PostAsync_WhenPeriodClosed_ThrowsAccountingPeriodClosedException()
    {
        var (service, _) = CreateService(PeriodStatus.Closed, VoucherNumberingMode.Automatic);

        await Assert.ThrowsAsync<AccountingPeriodClosedException>(
            () => service.PostAsync(BuildRequest(), CancellationToken.None));
    }

    [Fact]
    public async Task PostAsync_RetriedWithSameContent_DoesNotCreateASecondEntry()
    {
        var (service, entries) = CreateService(PeriodStatus.Open, VoucherNumberingMode.Automatic);

        var first = await service.PostAsync(BuildRequest(), CancellationToken.None);
        var second = await service.PostAsync(BuildRequest(), CancellationToken.None);

        Assert.Single(entries.Entries);
        Assert.Equal(first.SequenceNumber, second.SequenceNumber);
        Assert.Equal("Posted", second.Status);
    }

    [Fact]
    public async Task PostAsync_RetriedWithDifferentContent_ThrowsIdempotencyConflict()
    {
        var (service, _) = CreateService(PeriodStatus.Open, VoucherNumberingMode.Automatic);

        await service.PostAsync(BuildRequest(debit: 100m), CancellationToken.None);

        await Assert.ThrowsAsync<JournalEntryIdempotencyConflictException>(
            () => service.PostAsync(BuildRequest(debit: 200m), CancellationToken.None));
    }
}
