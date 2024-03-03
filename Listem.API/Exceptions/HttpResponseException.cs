namespace Listem.API.Exceptions;

public class HttpResponseException(int statusCode, object? value = null) : Exception
{
    public int StatusCode { get; } = statusCode;

    public object? Value { get; } = value;
}
