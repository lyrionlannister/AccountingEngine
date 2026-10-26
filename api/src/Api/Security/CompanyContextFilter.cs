using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Companies.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace Api.Security;

public sealed class CompanyContextFilter(ICompanyRepository companies, CurrentCompanyContext currentCompany) : IAsyncActionFilter
{
    public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
    {
        if (!context.RouteData.Values.TryGetValue("companyPublicId", out var raw)
            || !Guid.TryParse(raw as string, out var companyPublicId))
        {
            await next();
            return;
        }

        var company = await companies.FindByPublicIdAsync(companyPublicId, context.HttpContext.RequestAborted);
        if (company is null)
        {
            context.Result = NotFound(context);
            return;
        }

        var userId = long.Parse(context.HttpContext.User.FindFirstValue(JwtRegisteredClaimNames.Sub)!);

        try
        {
            await currentCompany.ResolveAsync(userId, company.Id, context.HttpContext.RequestAborted);
        }
        catch (CompanyMembershipNotFoundException)
        {
            context.Result = NotFound(context);
            return;
        }

        await next();
    }

    private static ObjectResult NotFound(ActionExecutingContext context) => new(new ProblemDetails
    {
        Status = StatusCodes.Status404NotFound,
        Title = "Empresa no encontrada"
    })
    {
        StatusCode = StatusCodes.Status404NotFound
    };
}
