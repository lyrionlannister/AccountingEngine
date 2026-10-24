using Accounting.Domain;
using Accounting.Domain.Enums;
using Accounting.Persistence;
using Accounting.Services;
using Microsoft.EntityFrameworkCore;
using Npgsql;

namespace Accounting.Repositories;

public sealed class JournalEntryRepository : IJournalEntryRepository
{
    private readonly AppDbContext _context;

    public JournalEntryRepository(AppDbContext context) => _context = context;

    public Task<JournalEntry?> FindByIdAsync(long companyId, long id, CancellationToken cancellationToken) =>
        _context.JournalEntries
            .Include(e => e.Lines)
            .FirstOrDefaultAsync(e => e.CompanyId == companyId && e.Id == id, cancellationToken);

    public Task<JournalEntry?> FindBySourceAsync(long companyId, string? sourceModule, string sourceReference, CancellationToken cancellationToken) =>
        _context.JournalEntries
            .Include(e => e.Lines)
            .FirstOrDefaultAsync(e => e.CompanyId == companyId && e.SourceModule == sourceModule && e.SourceReference == sourceReference, cancellationToken);

    public async Task AddAsync(JournalEntry entry, CancellationToken cancellationToken)
    {
        await using var transaction = await _context.Database.BeginTransactionAsync(cancellationToken);

        await LockAndRequirePeriodOpenAsync(entry.CompanyId, entry.AccountingPeriodId, cancellationToken);

        _context.JournalEntries.Add(entry);
        await SaveChangesTranslatingConflictsAsync(entry, cancellationToken);
        await transaction.CommitAsync(cancellationToken);
    }

    public async Task AddWithAutomaticSequenceAsync(JournalEntry entry, Action<long> onSequenceReserved, CancellationToken cancellationToken)
    {
        await using var transaction = await _context.Database.BeginTransactionAsync(cancellationToken);

        await LockAndRequirePeriodOpenAsync(entry.CompanyId, entry.AccountingPeriodId, cancellationToken);

        await _context.VoucherTypes
            .FromSqlInterpolated($"SELECT * FROM tipos_comprobante WHERE id = {entry.VoucherTypeId} AND empresa_id = {entry.CompanyId} FOR UPDATE")
            .SingleOrDefaultAsync(cancellationToken);

        var maxSequence = await _context.JournalEntries
            .Where(e => e.CompanyId == entry.CompanyId && e.VoucherTypeId == entry.VoucherTypeId)
            .Select(e => e.SequenceNumber)
            .MaxAsync(cancellationToken);

        onSequenceReserved((maxSequence ?? 0) + 1);

        _context.JournalEntries.Add(entry);
        await SaveChangesTranslatingConflictsAsync(entry, cancellationToken);
        await transaction.CommitAsync(cancellationToken);
    }

    private async Task SaveChangesTranslatingConflictsAsync(JournalEntry entry, CancellationToken cancellationToken)
    {
        try
        {
            await _context.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateException ex) when (ex.InnerException is PostgresException
        {
            SqlState: PostgresErrorCodes.UniqueViolation,
            ConstraintName: "ix_asientos_contables_idempotencia_origen"
        })
        {
            throw new JournalEntryIdempotencyConflictException(entry.SourceModule, entry.SourceReference!);
        }
        catch (DbUpdateException ex) when (ex.InnerException is PostgresException
        {
            SqlState: PostgresErrorCodes.UniqueViolation,
            ConstraintName: "ix_asientos_contables_empresa_comprobante_secuencia"
        })
        {
            throw new JournalEntrySequenceConflictException(entry.CompanyId, entry.VoucherTypeId, entry.SequenceNumber!.Value);
        }
    }

    private async Task LockAndRequirePeriodOpenAsync(long companyId, long accountingPeriodId, CancellationToken cancellationToken)
    {
        var period = await _context.AccountingPeriods
            .FromSqlInterpolated($"SELECT * FROM periodos_contables WHERE id = {accountingPeriodId} AND empresa_id = {companyId} FOR UPDATE")
            .SingleOrDefaultAsync(cancellationToken)
            ?? throw new InvalidOperationException($"Accounting period {accountingPeriodId} not found.");

        if (period.Status != PeriodStatus.Open)
            throw new AccountingPeriodClosedException(accountingPeriodId);
    }
}
