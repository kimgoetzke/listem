using System.Net;

namespace Listem.API.Exceptions;

public class HttpResponseException(
    HttpStatusCode statusCode,
    string message,
    string? title = null,
    object? value = null
) : Exception
{
    public HttpStatusCode StatusCode { get; } = statusCode;
    public string Title { get; } = title ?? statusCode.ToString();
    public new string Message { get; } = message;
    public object? Value { get; } = value;
}
