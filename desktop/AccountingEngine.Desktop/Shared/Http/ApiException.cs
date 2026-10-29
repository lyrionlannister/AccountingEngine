using System;
using System.Collections.Generic;

namespace AccountingEngine.Desktop.Shared.Http;

public sealed class ApiException : Exception
{
    public int StatusCode { get; }
    public IReadOnlyDictionary<string, string[]> FieldErrors { get; }

    public ApiException(int statusCode, string message, IReadOnlyDictionary<string, string[]>? fieldErrors = null)
        : base(message)
    {
        StatusCode = statusCode;
        FieldErrors = fieldErrors ?? new Dictionary<string, string[]>();
    }
}
