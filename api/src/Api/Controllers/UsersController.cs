using System.ComponentModel.DataAnnotations;
using Companies.DTOs;
using Companies.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace Api.Controllers;

[ApiController]
[Route("api/users")]
public sealed class UsersController : ControllerBase
{
    private readonly RegisterTenantWithOwnerService _register;

    public UsersController(RegisterTenantWithOwnerService register) => _register = register;

    public sealed record RegisterBody(
        [Required(ErrorMessage = "El correo es obligatorio.")]
        [EmailAddress(ErrorMessage = "El correo no tiene un formato válido.")]
        string Email,
        [Required(ErrorMessage = "La contraseña es obligatoria.")] string Password,
        [Required(ErrorMessage = "El nombre de la empresa matriz (tenant) es obligatorio.")] string TenantName,
        [Required(ErrorMessage = "El nombre de la empresa es obligatorio.")] string CompanyName,
        [Required(ErrorMessage = "El código de país es obligatorio.")]
        [RegularExpression("^[A-Z]{2}$", ErrorMessage = "El código de país debe tener 2 letras mayúsculas (ISO 3166-1 alpha-2).")]
        string CountryCode);

    public sealed record RegisterResponse(Guid OwnerPublicId, Guid TenantPublicId, Guid CompanyPublicId, Guid MembershipPublicId);

    [HttpPost("register")]
    [AllowAnonymous]
    [EnableRateLimiting("register")]
    public async Task<ActionResult<RegisterResponse>> Register([FromBody] RegisterBody body, CancellationToken cancellationToken)
    {
        var result = await _register.RegisterAsync(
            new RegisterTenantWithOwnerRequest(body.Email, body.Password, body.TenantName, body.CompanyName, body.CountryCode),
            cancellationToken);

        return StatusCode(StatusCodes.Status201Created, new RegisterResponse(
            result.OwnerPublicId, result.TenantPublicId, result.CompanyPublicId, result.MembershipPublicId));
    }
}
