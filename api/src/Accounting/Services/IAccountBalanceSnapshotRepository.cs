using Accounting.Domain;

namespace Accounting.Services;

public interface IAccountBalanceSnapshotRepository
{
    // companyId explicito en la query, igual que el resto de los repositorios del modulo
    // (Account, CostCenter, Party, VoucherType, AccountingPeriod, JournalEntry,
    // PeriodReopenRequest) — segunda capa de defensa ademas del filtro global, no solo
    // conveniencia (hallazgo de auditoria: este era el unico repo sin ella).
    Task<AccountBalanceSnapshot?> FindByIdAsync(long companyId, long accountingPeriodId, long accountId, CancellationToken cancellationToken);

    Task<(decimal DebitTotal, decimal CreditTotal)> SumPostedLinesAsync(long companyId, long accountingPeriodId, long accountId, CancellationToken cancellationToken);

    Task AddAsync(AccountBalanceSnapshot snapshot, CancellationToken cancellationToken);

    Task UpdateAsync(AccountBalanceSnapshot snapshot, CancellationToken cancellationToken);
}
