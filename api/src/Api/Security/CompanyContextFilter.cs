using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Companies.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace Api.Security;

// Filtro global (Program.cs: AddControllers(o => o.Filters.Add<CompanyContextFilter>())) — una ruta
// con {companyPublicId} nunca puede olvidar aplicarlo, a diferencia de un atributo que haya que
// decorar en cada Controller nuevo. Corre después de [Authorize] (filtros de MVC se ejecutan ya
// autenticado), por eso puede leer el claim "sub" sin volver a validar el token.
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
            // Mismo resultado que "la empresa no existe" (arriba) — nunca se confirma a quien
            // llama que el companyPublicId es real si no tiene membership activa en ella.
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
