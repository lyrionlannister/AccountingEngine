using Accounting.DTOs;
using Accounting.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Shared;

namespace Api.Controllers;

[ApiController]
[Authorize]
[Tags("Account Balance Snapshots")]
[Route("api/companies/{companyPublicId:guid}/accounting-periods/{periodPublicId:guid}/accounts/{accountPublicId:guid}/balance-snapshot")]
public sealed class AccountBalanceSnapshotsController(
    ICurrentCompanyContext currentCompany,
    IAccountingPeriodRepository accountingPeriods,
    IAccountRepository accounts,
    RecalculateAccountBalanceSnapshotService recalculateSnapshot) : ControllerBase
{
    [HttpPost("recalculation")]
    public async Task<ActionResult<AccountBalanceSnapshotResult>> Recalculate(
        Guid periodPublicId, Guid accountPublicId, CancellationToken cancellationToken)
    {
        var companyId = currentCompany.CompanyId;

        var period = await accountingPeriods.FindByPublicIdAsync(companyId, periodPublicId, cancellationToken);
        if (period is null)
            return NotFoundProblem("Período contable no encontrado");

        var account = await accounts.FindByPublicIdAsync(companyId, accountPublicId, cancellationToken);
        if (account is null)
            return NotFoundProblem("Cuenta no encontrada");

        var result = await recalculateSnapshot.RecalculateAsync(
            new RecalculateAccountBalanceSnapshotRequest(companyId, period.Id, account.Id), cancellationToken);
        return Ok(result);
    }

    private ObjectResult NotFoundProblem(string title) => new(new ProblemDetails
    {
        Status = StatusCodes.Status404NotFound,
        Title = title
    })
    {
        StatusCode = StatusCodes.Status404NotFound
    };
}
