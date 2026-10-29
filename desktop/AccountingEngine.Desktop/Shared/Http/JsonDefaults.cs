using System.Text.Json;

namespace AccountingEngine.Desktop.Shared.Http;

public static class JsonDefaults
{
    public static readonly JsonSerializerOptions Options = new(JsonSerializerDefaults.Web);
}
