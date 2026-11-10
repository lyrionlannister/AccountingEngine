namespace Api.Contracts;

public static class PaginationRequest
{
    public const int DefaultPageSize = 50;
    public const int MaxPageSize = 100;

    public static (int Page, int PageSize) Validate(int? page, int? pageSize)
    {
        var resolvedPage = page ?? 1;
        var resolvedPageSize = pageSize ?? DefaultPageSize;

        if (resolvedPage < 1)
            throw new InvalidPaginationException(
                $"El parámetro 'page' debe ser mayor o igual a 1 (se recibió {resolvedPage}).");

        if (resolvedPageSize < 1)
            throw new InvalidPaginationException(
                $"El parámetro 'pageSize' debe ser mayor o igual a 1 (se recibió {resolvedPageSize}).");

        if (resolvedPageSize > MaxPageSize)
            throw new InvalidPaginationException(
                $"El parámetro 'pageSize' no puede ser mayor a {MaxPageSize} (se recibió {resolvedPageSize}).");

        return (resolvedPage, resolvedPageSize);
    }
}

public sealed class InvalidPaginationException(string message) : Exception(message);
