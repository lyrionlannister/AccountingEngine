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
[Tags("Period Reopen Requests")]
[Route("api/companies/{companyPublicId:guid}/period-reopen-requests")]
public sealed class PeriodReopenRequestsController(
    ICurrentCompanyContext currentCompany,
    IPeriodReopenRequestRepository reopenRequests,
    ResolvePeriodReopenService resolveReopen) : ControllerBase
{
    public sealed record ResolveReopenBody(bool Approve, string? Notes = null);

    [HttpPost("{publicId:guid}/resolution")]
    public async Task<ActionResult<PeriodReopenRequestResult>> Resolve(Guid publicId, [FromBody] ResolveReopenBody body, CancellationToken cancellationToken)
    {
        var companyId = currentCompany.CompanyId;

        var request = await reopenRequests.FindByPublicIdAsync(companyId, publicId, cancellationToken);
        if (request is null)
            return NotFoundProblem();

        var userId = User.FindFirstValue(JwtRegisteredClaimNames.Sub)!;
        var result = await resolveReopen.ResolveAsync(
            new ResolvePeriodReopenRequest(companyId, request.Id, body.Approve, userId, body.Notes), cancellationToken);
        return Ok(result);
    }

    private ObjectResult NotFoundProblem() => new(new ProblemDetails
    {
        Status = StatusCodes.Status404NotFound,
        Title = "Solicitud de reapertura no encontrada"
    })
    {
        StatusCode = StatusCodes.Status404NotFound
    };
}
