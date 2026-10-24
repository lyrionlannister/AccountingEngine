using Accounting.Domain;
using Accounting.DTOs;

namespace Accounting.Services;

public sealed class RecalculateAccountBalanceSnapshotService
{
    private readonly IAccountingPeriodRepository _periods;
    private readonly IAccountBalanceSnapshotRepository _snapshots;

    public RecalculateAccountBalanceSnapshotService(IAccountingPeriodRepository periods, IAccountBalanceSnapshotRepository snapshots)
    {
        _periods = periods;
        _snapshots = snapshots;
    }

    public async Task<AccountBalanceSnapshotResult> RecalculateAsync(RecalculateAccountBalanceSnapshotRequest request, CancellationToken cancellationToken)
    {
        _ = await _periods.FindByIdAsync(request.CompanyId, request.AccountingPeriodId, cancellationToken)
            ?? throw new InvalidOperationException($"Accounting period {request.AccountingPeriodId} not found.");

        var (debitTotal, creditTotal) = await _snapshots.SumPostedLinesAsync(
            request.CompanyId, request.AccountingPeriodId, request.AccountId, cancellationToken);

        var now = DateTimeOffset.UtcNow;
        var snapshot = await _snapshots.FindByIdAsync(request.AccountingPeriodId, request.AccountId, cancellationToken);
        if (snapshot is null)
        {
            snapshot = AccountBalanceSnapshot.Create(request.AccountingPeriodId, request.AccountId, debitTotal, creditTotal, now);
            await _snapshots.AddAsync(snapshot, cancellationToken);
        }
        else
        {
            snapshot.Recalculate(debitTotal, creditTotal, now);
            await _snapshots.UpdateAsync(snapshot, cancellationToken);
        }

        return new AccountBalanceSnapshotResult(snapshot.AccountingPeriodId, snapshot.AccountId, snapshot.DebitTotal, snapshot.CreditTotal, snapshot.CalculatedAt);
    }
}
