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

    // Falla de resolución de una referencia del body/ruta a su id interno (PublicId que no existe
    // en la empresa activa) — nunca llega a los Services, se traduce a 404 aquí mismo porque es
    // un problema de transporte, no una regla de negocio que Accounting.Services deba conocer.
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

            var lines = new List<JournalEntryLineRequest>(body.Lines.Count);
            foreach (var line in body.Lines)
                lines.Add(await ResolveLineAsync(companyId, line, cancellationToken));

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

    private async Task<JournalEntryLineRequest> ResolveLineAsync(long companyId, JournalEntryLineBody line, CancellationToken cancellationToken)
    {
        var account = await accounts.FindByPublicIdAsync(companyId, line.AccountPublicId, cancellationToken)
            ?? throw new ReferenceNotFoundException("Account");

        long? costCenterId = null;
        if (line.CostCenterPublicId is { } costCenterPublicId)
        {
            var costCenter = await costCenters.FindByPublicIdAsync(companyId, costCenterPublicId, cancellationToken)
                ?? throw new ReferenceNotFoundException("CostCenter");
            costCenterId = costCenter.Id;
        }

        long? partyId = null;
        if (line.PartyPublicId is { } partyPublicId)
        {
            var party = await parties.FindByPublicIdAsync(companyId, partyPublicId, cancellationToken)
                ?? throw new ReferenceNotFoundException("Party");
            partyId = party.Id;
        }

        var currency = await currencies.FindByCodeAsync(line.CurrencyCode, cancellationToken)
            ?? throw new ReferenceNotFoundException("Currency");

        return new JournalEntryLineRequest(account.Id, costCenterId, partyId, currency.Id, line.DebitAmount, line.CreditAmount, line.Description);
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
