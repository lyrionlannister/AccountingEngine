using Accounting.Domain;
using Accounting.DTOs;
using Accounting.Services;

namespace Accounting.Tests;

public class ResolvePeriodReopenServiceTests
{
    private const long CompanyId = 1;

    private static (ResolvePeriodReopenService Service, PeriodReopenRequest ReopenRequest) CreateService(string role = "Admin")
    {
        var period = AccountingPeriod.Create(CompanyId, "2026-01", new DateOnly(2026, 1, 1), new DateOnly(2026, 1, 31));
        period.Close("closer", DateTimeOffset.UtcNow);
        var periods = new FakeAccountingPeriodRepository();
        periods.Periods.Add(period);

        var reopenRequest = PeriodReopenRequest.Create(CompanyId, 0, "Corrección", "requester-1", DateTimeOffset.UtcNow);
        var reopenRequests = new FakePeriodReopenRequestRepository();
        reopenRequests.Requests.Add(reopenRequest);

        var service = new ResolvePeriodReopenService(reopenRequests, periods, new FakeAuditTrailWriter(), new FakeCurrentCompanyContext(CompanyId, role));
        return (service, reopenRequest);
    }

    [Fact]
    public async Task ResolveAsync_AsAccountant_ThrowsInsufficientRole()
    {
        var (service, reopenRequest) = CreateService(role: "Accountant");

        await Assert.ThrowsAsync<InsufficientRoleException>(() => service.ResolveAsync(
            new ResolvePeriodReopenRequest(CompanyId, reopenRequest.Id, Approve: true, "resolver-1"), CancellationToken.None));
    }

    [Fact]
    public async Task ResolveAsync_AsViewer_ThrowsInsufficientRole()
    {
        var (service, reopenRequest) = CreateService(role: "Viewer");

        await Assert.ThrowsAsync<InsufficientRoleException>(() => service.ResolveAsync(
            new ResolvePeriodReopenRequest(CompanyId, reopenRequest.Id, Approve: true, "resolver-1"), CancellationToken.None));
    }

    [Fact]
    public async Task ResolveAsync_BySameUserWhoRequested_ThrowsSelfApprovalNotAllowed()
    {
        var (service, reopenRequest) = CreateService();

        await Assert.ThrowsAsync<SelfApprovalNotAllowedException>(() => service.ResolveAsync(
            new ResolvePeriodReopenRequest(CompanyId, reopenRequest.Id, Approve: true, reopenRequest.RequestedBy), CancellationToken.None));
    }

    [Fact]
    public async Task ResolveAsync_AsAdminByDifferentUser_Succeeds()
    {
        var (service, reopenRequest) = CreateService();

        var result = await service.ResolveAsync(
            new ResolvePeriodReopenRequest(CompanyId, reopenRequest.Id, Approve: true, "resolver-1"), CancellationToken.None);

        Assert.Equal("Approved", result.Status);
    }
}
