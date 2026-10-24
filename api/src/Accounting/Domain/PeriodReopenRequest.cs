using Accounting.Domain.Enums;

namespace Accounting.Domain;

public sealed class PeriodReopenRequest
{
    public long Id { get; private set; }
    public Guid PublicId { get; private set; }
    public long CompanyId { get; private set; }
    public long AccountingPeriodId { get; private set; }
    public string Reason { get; private set; } = string.Empty;
    public string RequestedBy { get; private set; } = string.Empty;
    public DateTimeOffset RequestedAt { get; private set; }
    public PeriodReopenRequestStatus Status { get; private set; }
    public string? ResolvedBy { get; private set; }
    public DateTimeOffset? ResolvedAt { get; private set; }
    public string? ResolutionNotes { get; private set; }

    private PeriodReopenRequest()
    {
    }

    public static PeriodReopenRequest Create(long companyId, long accountingPeriodId, string reason, string requestedBy, DateTimeOffset requestedAt)
    {
        if (string.IsNullOrWhiteSpace(reason))
            throw new ArgumentException("Reason is required.", nameof(reason));
        if (string.IsNullOrWhiteSpace(requestedBy))
            throw new ArgumentException("RequestedBy is required.", nameof(requestedBy));

        return new PeriodReopenRequest
        {
            CompanyId = companyId,
            AccountingPeriodId = accountingPeriodId,
            Reason = reason,
            RequestedBy = requestedBy,
            RequestedAt = requestedAt,
            Status = PeriodReopenRequestStatus.Pending
        };
    }

    public void Approve(string resolvedBy, DateTimeOffset resolvedAt, string? notes = null)
    {
        Resolve(PeriodReopenRequestStatus.Approved, resolvedBy, resolvedAt, notes);
    }

    public void Reject(string resolvedBy, DateTimeOffset resolvedAt, string? notes = null)
    {
        Resolve(PeriodReopenRequestStatus.Rejected, resolvedBy, resolvedAt, notes);
    }

    private void Resolve(PeriodReopenRequestStatus status, string resolvedBy, DateTimeOffset resolvedAt, string? notes)
    {
        if (Status != PeriodReopenRequestStatus.Pending)
            throw new InvalidOperationException("Only a pending request can be resolved.");
        if (string.IsNullOrWhiteSpace(resolvedBy))
            throw new ArgumentException("ResolvedBy is required.", nameof(resolvedBy));

        Status = status;
        ResolvedBy = resolvedBy;
        ResolvedAt = resolvedAt;
        ResolutionNotes = notes;
    }
}
