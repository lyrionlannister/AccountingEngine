using Accounting.Domain;

namespace Accounting.Services;

// Currency es un catálogo global (sin CompanyId, ver Currency.cs) — a diferencia del resto de
// repos de este módulo, esta búsqueda no filtra por empresa. Primer consumidor: Api, para
// resolver el código ISO 4217 del body de un endpoint al id interno antes de llamar a un Service.
public interface ICurrencyRepository
{
    Task<Currency?> FindByCodeAsync(string code, CancellationToken cancellationToken);
}
