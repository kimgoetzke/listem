using System.Net;

namespace Listem.API.Exceptions;

public class HttpResponseException(HttpStatusCode statusCode, string message, string? value = null)
    : Exception
{
    public HttpStatusCode StatusCode { get; } = statusCode;
    public new string Message { get; } = message;
    public object? Value { get; } = value;
}
