using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;

namespace AccountingEngine.Desktop.Shared.Http;

internal sealed record ProblemPayload(
    [property: JsonPropertyName("title")] string? Title,
    [property: JsonPropertyName("detail")] string? Detail,
    [property: JsonPropertyName("errors")] Dictionary<string, string[]>? Errors);

public static class HttpResponseExtensions
{
    public static async Task EnsureApiSuccessAsync(this HttpResponseMessage response, CancellationToken cancellationToken)
    {
        if (response.IsSuccessStatusCode) return;

        ProblemPayload? problem = null;
        try
        {
            problem = await response.Content.ReadFromJsonAsync<ProblemPayload>(JsonDefaults.Options, cancellationToken);
        }
        catch (JsonException)
        {
            // cuerpo vacío o no-JSON: se usa el mensaje genérico de abajo
        }

        throw new ApiException(
            (int)response.StatusCode,
            problem?.Detail ?? problem?.Title ?? "Ocurrió un error inesperado. Intenta de nuevo.",
            problem?.Errors);
    }
}
