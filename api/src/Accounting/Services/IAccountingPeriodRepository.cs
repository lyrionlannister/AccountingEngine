using Accounting.Domain;

namespace Accounting.Services;

public interface IAccountingPeriodRepository
{
    Task<AccountingPeriod?> FindByIdAsync(long companyId, long id, CancellationToken cancellationToken);

    Task<AccountingPeriod?> FindByPublicIdAsync(long companyId, Guid publicId, CancellationToken cancellationToken);

    Task UpdateAsync(AccountingPeriod period, CancellationToken cancellationToken);

    // Toma el mismo lock de fila (FOR UPDATE) que JournalEntryRepository.AddAsync usa antes de
    // contabilizar — sin esto, un Close concurrente con un Post en vuelo no estaba serializado
    // contra ese Post (hallazgo de auditoria, api/CLAUDE.md §5.4). closeAction corre DENTRO del
    // lock, antes del commit, para que la decision de cerrar (incluyendo cualquier chequeo
    // adicional) sea atomica con la escritura.
    Task<AccountingPeriod> CloseAsync(long companyId, long accountingPeriodId, Func<AccountingPeriod, CancellationToken, Task> closeAction, CancellationToken cancellationToken);
}
