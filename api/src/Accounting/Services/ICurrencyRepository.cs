using Accounting.Domain;

namespace Accounting.Services;

public interface ICurrencyRepository
{
    Task<Currency?> FindByCodeAsync(string code, CancellationToken cancellationToken);

    Task<IReadOnlyList<Currency>> FindManyByCodeAsync(IReadOnlyCollection<string> codes, CancellationToken cancellationToken);
}
