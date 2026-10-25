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
[Tags("Accounting Periods")]
[Route("api/companies/{companyPublicId:guid}/accounting-periods")]
public sealed class AccountingPeriodsController(
    ICurrentCompanyContext currentCompany,
    IAccountingPeriodRepository accountingPeriods,
    CloseAccountingPeriodService closePeriod,
    RequestPeriodReopenService requestReopen) : ControllerBase
{
    public sealed record RequestReopenBody(
        [Required(ErrorMessage = "El motivo es obligatorio.")] string Reason);

    [HttpPost("{publicId:guid}/closure")]
    public async Task<ActionResult<AccountingPeriodResult>> Close(Guid publicId, CancellationToken cancellationToken)
    {
        var companyId = currentCompany.CompanyId;

        var period = await accountingPeriods.FindByPublicIdAsync(companyId, publicId, cancellationToken);
        if (period is null)
            return NotFoundProblem();

        var result = await closePeriod.CloseAsync(
            new CloseAccountingPeriodRequest(companyId, period.Id, ActorFromToken()), cancellationToken);
        return Ok(result);
    }

    [HttpPost("{publicId:guid}/reopen-requests")]
    public async Task<ActionResult<PeriodReopenRequestResult>> RequestReopen(Guid publicId, [FromBody] RequestReopenBody body, CancellationToken cancellationToken)
    {
        var companyId = currentCompany.CompanyId;

        var period = await accountingPeriods.FindByPublicIdAsync(companyId, publicId, cancellationToken);
        if (period is null)
            return NotFoundProblem();

        var result = await requestReopen.RequestAsync(
            new RequestPeriodReopenRequest(companyId, period.Id, body.Reason, ActorFromToken()), cancellationToken);
        return Created(string.Empty, result);
    }

    private string ActorFromToken() => User.FindFirstValue(JwtRegisteredClaimNames.Sub)!;

    private ObjectResult NotFoundProblem() => new(new ProblemDetails
    {
        Status = StatusCodes.Status404NotFound,
        Title = "Período contable no encontrado"
    })
    {
        StatusCode = StatusCodes.Status404NotFound
    };
}
