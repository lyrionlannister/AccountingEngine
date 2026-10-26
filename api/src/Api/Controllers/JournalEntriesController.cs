using System.ComponentModel.DataAnnotations;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Accounting.DTOs;
using Accounting.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Shared;

namespace Api.Controllers;

[ApiController]
[Authorize]
[Tags("Journal Entries")]
[Route("api/companies/{companyPublicId:guid}/journal-entries")]
public sealed class JournalEntriesController(
    ICurrentCompanyContext currentCompany,
    PostJournalEntryService postJournalEntry,
    ReverseJournalEntryService reverseJournalEntry,
    IJournalEntryRepository journalEntries,
    IVoucherTypeRepository voucherTypes,
    IAccountRepository accounts,
    ICostCenterRepository costCenters,
    IPartyRepository parties,
    ICurrencyRepository currencies,
    IAccountingPeriodRepository accountingPeriods) : ControllerBase
{
    public sealed record JournalEntryLineBody(
        Guid AccountPublicId,
        Guid? CostCenterPublicId,
        Guid? PartyPublicId,
        [Required(ErrorMessage = "El código de moneda es obligatorio.")] string CurrencyCode,
        decimal DebitAmount,
        decimal CreditAmount,
        string? Description = null);

    public sealed record PostJournalEntryBody(
        Guid VoucherTypePublicId,
        Guid AccountingPeriodPublicId,
        DateOnly EntryDate,
        [Required(ErrorMessage = "La descripción es obligatoria.")] string Description,
        [Required(ErrorMessage = "El asiento debe tener al menos una línea.")]
        [MinLength(1, ErrorMessage = "El asiento debe tener al menos una línea.")]
        IReadOnlyList<JournalEntryLineBody> Lines,
        string? SourceModule = null,
        string? SourceReference = null,
        long? SequenceNumber = null);

    public sealed record ReverseJournalEntryBody(
        DateOnly EntryDate,
        [Required(ErrorMessage = "La descripción es obligatoria.")] string Description);

    private sealed class ReferenceNotFoundException(string referenceKind) : Exception
    {
        public string ReferenceKind { get; } = referenceKind;
    }

    [HttpPost]
    public async Task<ActionResult<JournalEntryResult>> Post([FromBody] PostJournalEntryBody body, CancellationToken cancellationToken)
    {
        var companyId = currentCompany.CompanyId;

        try
        {
            var voucherType = await voucherTypes.FindByPublicIdAsync(companyId, body.VoucherTypePublicId, cancellationToken)
                ?? throw new ReferenceNotFoundException("VoucherType");

            var period = await accountingPeriods.FindByPublicIdAsync(companyId, body.AccountingPeriodPublicId, cancellationToken)
                ?? throw new ReferenceNotFoundException("AccountingPeriod");

            var lines = await ResolveLinesAsync(companyId, body.Lines, cancellationToken);

            var request = new PostJournalEntryRequest(
                companyId,
                voucherType.Id,
                period.Id,
                body.EntryDate,
                body.Description,
                PostedByFromToken(),
                lines,
                body.SourceModule,
                body.SourceReference,
                body.SequenceNumber);

            var result = await postJournalEntry.PostAsync(request, cancellationToken);
            return Created(string.Empty, result);
        }
        catch (ReferenceNotFoundException ex)
        {
            return NotFoundProblem(ex.ReferenceKind);
        }
    }

    [HttpPost("{publicId:guid}/reversal")]
    public async Task<ActionResult<JournalEntryResult>> Reverse(Guid publicId, [FromBody] ReverseJournalEntryBody body, CancellationToken cancellationToken)
    {
        var companyId = currentCompany.CompanyId;

        var original = await journalEntries.FindByPublicIdAsync(companyId, publicId, cancellationToken);
        if (original is null)
            return NotFoundProblem(nameof(JournalEntryResult));

        var request = new ReverseJournalEntryRequest(companyId, original.Id, body.EntryDate, body.Description, PostedByFromToken());
        var result = await reverseJournalEntry.ReverseAsync(request, cancellationToken);
        return Created(string.Empty, result);
    }

    private async Task<IReadOnlyList<JournalEntryLineRequest>> ResolveLinesAsync(
        long companyId, IReadOnlyList<JournalEntryLineBody> lines, CancellationToken cancellationToken)
    {
        var accountIds = lines.Select(l => l.AccountPublicId).Distinct().ToList();
        var costCenterIds = lines.Where(l => l.CostCenterPublicId is not null).Select(l => l.CostCenterPublicId!.Value).Distinct().ToList();
        var partyIds = lines.Where(l => l.PartyPublicId is not null).Select(l => l.PartyPublicId!.Value).Distinct().ToList();
        var currencyCodes = lines.Select(l => l.CurrencyCode).Distinct().ToList();

        var accountsById = (await accounts.FindManyByPublicIdAsync(companyId, accountIds, cancellationToken)).ToDictionary(a => a.PublicId);
        var costCentersById = costCenterIds.Count == 0
            ? []
            : (await costCenters.FindManyByPublicIdAsync(companyId, costCenterIds, cancellationToken)).ToDictionary(c => c.PublicId);
        var partiesById = partyIds.Count == 0
            ? []
            : (await parties.FindManyByPublicIdAsync(companyId, partyIds, cancellationToken)).ToDictionary(p => p.PublicId);
        var currenciesByCode = (await currencies.FindManyByCodeAsync(currencyCodes, cancellationToken)).ToDictionary(c => c.Code);

        var resolved = new List<JournalEntryLineRequest>(lines.Count);
        foreach (var line in lines)
        {
            if (!accountsById.TryGetValue(line.AccountPublicId, out var account))
                throw new ReferenceNotFoundException("Account");

            long? costCenterId = null;
            if (line.CostCenterPublicId is { } costCenterPublicId)
            {
                if (!costCentersById.TryGetValue(costCenterPublicId, out var costCenter))
                    throw new ReferenceNotFoundException("CostCenter");
                costCenterId = costCenter.Id;
            }

            long? partyId = null;
            if (line.PartyPublicId is { } partyPublicId)
            {
                if (!partiesById.TryGetValue(partyPublicId, out var party))
                    throw new ReferenceNotFoundException("Party");
                partyId = party.Id;
            }

            if (!currenciesByCode.TryGetValue(line.CurrencyCode, out var currency))
                throw new ReferenceNotFoundException("Currency");

            resolved.Add(new JournalEntryLineRequest(account.Id, costCenterId, partyId, currency.Id, line.DebitAmount, line.CreditAmount, line.Description));
        }

        return resolved;
    }

    private string PostedByFromToken() => User.FindFirstValue(JwtRegisteredClaimNames.Sub)!;

    private ObjectResult NotFoundProblem(string referenceKind) => new(new ProblemDetails
    {
        Status = StatusCodes.Status404NotFound,
        Title = "Referencia no encontrada",
        Detail = $"No se encontró \"{referenceKind}\" en la empresa activa."
    })
    {
        StatusCode = StatusCodes.Status404NotFound
    };
}
