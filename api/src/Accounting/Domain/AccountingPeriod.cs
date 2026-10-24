using Accounting.Domain.Enums;

namespace Accounting.Domain;

public sealed class AccountingPeriod
{
    public long Id { get; private set; }
    public Guid PublicId { get; private set; }
    public long CompanyId { get; private set; }
    public string Name { get; private set; } = string.Empty;
    public DateOnly StartDate { get; private set; }
    public DateOnly EndDate { get; private set; }
    public PeriodStatus Status { get; private set; }
    public DateTimeOffset? ClosedAt { get; private set; }
    public string? ClosedBy { get; private set; }

    private AccountingPeriod()
    {
    }

    public static AccountingPeriod Create(long companyId, string name, DateOnly startDate, DateOnly endDate)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Name is required.", nameof(name));
        if (endDate <= startDate)
            throw new ArgumentException("End date must be after start date.", nameof(endDate));

        return new AccountingPeriod
        {
            CompanyId = companyId,
            Name = name,
            StartDate = startDate,
            EndDate = endDate,
            Status = PeriodStatus.Open
        };
    }

    public bool Contains(DateOnly date) => date >= StartDate && date <= EndDate;

    public void Close(string closedBy, DateTimeOffset now)
    {
        if (Status != PeriodStatus.Open)
            throw new InvalidOperationException("Only an open period can be closed.");
        if (string.IsNullOrWhiteSpace(closedBy))
            throw new ArgumentException("ClosedBy is required.", nameof(closedBy));

        Status = PeriodStatus.Closed;
        ClosedAt = now;
        ClosedBy = closedBy;
    }

    public void Reopen()
    {
        if (Status != PeriodStatus.Closed)
            throw new InvalidOperationException("Only a closed period can be reopened.");

        Status = PeriodStatus.Open;
        ClosedAt = null;
        ClosedBy = null;
    }
}
