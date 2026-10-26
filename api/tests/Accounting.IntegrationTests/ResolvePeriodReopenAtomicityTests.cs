using Accounting.Domain;
using Accounting.Domain.Enums;
using Accounting.DTOs;
using Accounting.Repositories;
using Accounting.Services;
using Microsoft.EntityFrameworkCore;

namespace Accounting.IntegrationTests;

[Collection(PostgresCollection.Name)]
public sealed class ResolvePeriodReopenAtomicityTests : IAsyncLifetime
{
    private readonly PostgresContainerFixture _fixture;
    private string _postgres = null!;

    public ResolvePeriodReopenAtomicityTests(PostgresContainerFixture fixture) => _fixture = fixture;

    public async Task InitializeAsync() => _postgres = await _fixture.CreateDatabaseAsync();

    public Task DisposeAsync() => Task.CompletedTask;

    [Fact]
    public async Task ResolveAsync_Approve_WhenSaveFails_LeavesPeriodAndRequestAndAuditUnapplied()
    {
        await using var database = await TestDatabase.CreateAsync(_postgres);

        const long companyId = 1;
        long periodId, reopenRequestId;

        await using (var seedContext = database.NewContext())
        {
            var period = AccountingPeriod.Create(companyId, "2026-01", new DateOnly(2026, 1, 1), new DateOnly(2026, 1, 31));
            period.Close("closer", DateTimeOffset.UtcNow);
            seedContext.AccountingPeriods.Add(period);
            await seedContext.SaveChangesAsync();
            periodId = period.Id;

            var reopenRequest = PeriodReopenRequest.Create(companyId, periodId, "Corrección", "requester-1", DateTimeOffset.UtcNow);
            seedContext.PeriodReopenRequests.Add(reopenRequest);
            await seedContext.SaveChangesAsync();
            reopenRequestId = reopenRequest.Id;
        }

        await using var context = database.NewContext();
        var service = new ResolvePeriodReopenService(
            new PeriodReopenRequestRepository(context), new AccountingPeriodRepository(context), new AuditTrailWriter(context),
            new FixedCurrentCompanyContext(companyId));

        using var alreadyCancelled = new CancellationTokenSource();
        alreadyCancelled.Cancel();

        await Assert.ThrowsAnyAsync<OperationCanceledException>(() => service.ResolveAsync(
            new ResolvePeriodReopenRequest(companyId, reopenRequestId, Approve: true, ResolvedBy: "resolver-1"),
            alreadyCancelled.Token));

        await using var verifyContext = database.NewContext();
        var persistedPeriod = await verifyContext.AccountingPeriods.AsNoTracking().SingleAsync(p => p.Id == periodId);
        var persistedRequest = await verifyContext.PeriodReopenRequests.AsNoTracking().SingleAsync(r => r.Id == reopenRequestId);
        var auditCount = await verifyContext.AuditTrailEntries.CountAsync(a => a.CompanyId == companyId);

        Assert.Equal(PeriodStatus.Closed, persistedPeriod.Status);
        Assert.Equal(PeriodReopenRequestStatus.Pending, persistedRequest.Status);
        Assert.Equal(0, auditCount);
    }
}
