using Accounting.Domain;
using Accounting.Domain.Enums;

namespace Accounting.Tests;

public class PeriodReopenRequestTests
{
    [Fact]
    public void Create_SetsPendingStatus()
    {
        var request = PeriodReopenRequest.Create(1, 1, "Corrección de error", "user-1", DateTimeOffset.UtcNow);

        Assert.Equal(PeriodReopenRequestStatus.Pending, request.Status);
        Assert.Null(request.ResolvedBy);
        Assert.Null(request.ResolvedAt);
    }

    [Fact]
    public void Approve_WhenPending_SetsApprovedAndAuditInfo()
    {
        var request = PeriodReopenRequest.Create(1, 1, "Corrección de error", "user-1", DateTimeOffset.UtcNow);
        var now = DateTimeOffset.UtcNow;

        request.Approve("admin-1", now, "Autorizado por contraloría");

        Assert.Equal(PeriodReopenRequestStatus.Approved, request.Status);
        Assert.Equal("admin-1", request.ResolvedBy);
        Assert.Equal(now, request.ResolvedAt);
        Assert.Equal("Autorizado por contraloría", request.ResolutionNotes);
    }

    [Fact]
    public void Reject_WhenPending_SetsRejected()
    {
        var request = PeriodReopenRequest.Create(1, 1, "Corrección de error", "user-1", DateTimeOffset.UtcNow);

        request.Reject("admin-1", DateTimeOffset.UtcNow);

        Assert.Equal(PeriodReopenRequestStatus.Rejected, request.Status);
    }

    [Fact]
    public void Approve_WhenAlreadyResolved_Throws()
    {
        var request = PeriodReopenRequest.Create(1, 1, "Corrección de error", "user-1", DateTimeOffset.UtcNow);
        request.Approve("admin-1", DateTimeOffset.UtcNow);

        Assert.Throws<InvalidOperationException>(() => request.Reject("admin-1", DateTimeOffset.UtcNow));
    }
}
