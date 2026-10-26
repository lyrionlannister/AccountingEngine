using Accounting.Domain;
using Accounting.Domain.Enums;
using Accounting.DTOs;
using Shared;

namespace Accounting.Services;

public sealed class PostJournalEntryService
{
    private readonly IJournalEntryRepository _journalEntries;
    private readonly IAccountingPeriodRepository _periods;
    private readonly IVoucherTypeRepository _voucherTypes;
    private readonly IAccountRepository _accounts;
    private readonly ICostCenterRepository _costCenters;
    private readonly IPartyRepository _parties;
    private readonly IAuditTrailWriter _auditTrail;
    private readonly ICurrentCompanyContext _currentCompany;

    public PostJournalEntryService(
        IJournalEntryRepository journalEntries,
        IAccountingPeriodRepository periods,
        IVoucherTypeRepository voucherTypes,
        IAccountRepository accounts,
        ICostCenterRepository costCenters,
        IPartyRepository parties,
        IAuditTrailWriter auditTrail,
        ICurrentCompanyContext currentCompany)
    {
        _journalEntries = journalEntries;
        _periods = periods;
        _voucherTypes = voucherTypes;
        _accounts = accounts;
        _costCenters = costCenters;
        _parties = parties;
        _auditTrail = auditTrail;
        _currentCompany = currentCompany;
    }

    public async Task<JournalEntryResult> PostAsync(PostJournalEntryRequest request, CancellationToken cancellationToken)
    {
        RoleGuard.Require(_currentCompany.Role, "Admin", "Accountant");

        if (!string.IsNullOrWhiteSpace(request.SourceReference))
        {
            var existing = await _journalEntries.FindBySourceAsync(
                request.CompanyId, request.SourceModule, request.SourceReference, cancellationToken);

            if (existing is not null)
            {
                if (!MatchesRequest(existing, request))
                    throw new JournalEntryIdempotencyConflictException(request.SourceModule, request.SourceReference);

                return ToResult(existing);
            }
        }

        var period = await _periods.FindByIdAsync(request.CompanyId, request.AccountingPeriodId, cancellationToken)
            ?? throw new InvalidOperationException($"Accounting period {request.AccountingPeriodId} not found.");
        if (period.Status != PeriodStatus.Open)
            throw new AccountingPeriodClosedException(period.Id);
        if (!period.Contains(request.EntryDate))
            throw new ArgumentException("Entry date is outside the accounting period.", nameof(request));

        await EnsureLineReferencesBelongToCompanyAsync(request.CompanyId, request.Lines, cancellationToken);

        var entry = JournalEntry.Create(
            request.CompanyId, request.VoucherTypeId, request.AccountingPeriodId, request.EntryDate,
            request.Description, request.SourceModule, request.SourceReference);

        foreach (var line in request.Lines)
            entry.AddLine(line.AccountId, line.CostCenterId, line.PartyId, line.CurrencyId, line.DebitAmount, line.CreditAmount, line.Description);

        // Antes de persistir: el fixup del FK a esta fila (todavía sin Id) depende de que
        // ambas entidades viajen trackeadas en el mismo SaveChangesAsync (ver AuditTrailEntry).
        _auditTrail.Record(AuditOperation.JournalEntryPosted, request.CompanyId, request.PostedBy, entry, DateTimeOffset.UtcNow);

        await JournalEntryPoster.PostAsync(_journalEntries, _voucherTypes, entry, request.SequenceNumber, request.PostedBy, cancellationToken);

        return ToResult(entry);
    }

    // La FK de base de datos solo garantiza que la fila existe, no que es de la misma empresa
    // que el asiento (hallazgo de tenant-isolation) — sin esto, un caller podía contabilizar
    // contra una cuenta/centro de costo/tercero de otra empresa. Se agrupa por id distinto en
    // vez de consultar por línea (antipatterns-architecture: queries en loop).
    private async Task EnsureLineReferencesBelongToCompanyAsync(long companyId, IReadOnlyList<JournalEntryLineRequest> lines, CancellationToken cancellationToken)
    {
        var accountIds = lines.Select(l => l.AccountId).Distinct().ToList();
        var accounts = await _accounts.FindManyByIdAsync(companyId, accountIds, cancellationToken);
        foreach (var accountId in accountIds)
        {
            var account = accounts.FirstOrDefault(a => a.Id == accountId)
                ?? throw new JournalEntryLineReferenceNotFoundException(nameof(Account), companyId, accountId);
            if (!account.AllowsPosting)
                throw new AccountDoesNotAllowPostingException(account.Id);
        }

        var costCenterIds = lines.Where(l => l.CostCenterId is not null).Select(l => l.CostCenterId!.Value).Distinct().ToList();
        if (costCenterIds.Count > 0)
        {
            var costCenters = await _costCenters.FindManyByIdAsync(companyId, costCenterIds, cancellationToken);
            var missing = costCenterIds.FirstOrDefault(id => costCenters.All(c => c.Id != id));
            if (missing != default)
                throw new JournalEntryLineReferenceNotFoundException(nameof(CostCenter), companyId, missing);
        }

        var partyIds = lines.Where(l => l.PartyId is not null).Select(l => l.PartyId!.Value).Distinct().ToList();
        if (partyIds.Count > 0)
        {
            var parties = await _parties.FindManyByIdAsync(companyId, partyIds, cancellationToken);
            var missing = partyIds.FirstOrDefault(id => parties.All(p => p.Id != id));
            if (missing != default)
                throw new JournalEntryLineReferenceNotFoundException(nameof(Party), companyId, missing);
        }
    }

    private static bool MatchesRequest(JournalEntry existing, PostJournalEntryRequest request)
    {
        if (existing.EntryDate != request.EntryDate || existing.Description != request.Description)
            return false;
        if (existing.Lines.Count != request.Lines.Count)
            return false;

        for (var i = 0; i < existing.Lines.Count; i++)
        {
            var existingLine = existing.Lines[i];
            var requestLine = request.Lines[i];
            if (existingLine.AccountId != requestLine.AccountId
                || existingLine.CostCenterId != requestLine.CostCenterId
                || existingLine.PartyId != requestLine.PartyId
                || existingLine.CurrencyId != requestLine.CurrencyId
                || existingLine.DebitAmount != requestLine.DebitAmount
                || existingLine.CreditAmount != requestLine.CreditAmount)
                return false;
        }

        return true;
    }

    internal static JournalEntryResult ToResult(JournalEntry entry) =>
        new(entry.Id, entry.PublicId, entry.SequenceNumber, entry.Status.ToString(), entry.PostedAt);
}
