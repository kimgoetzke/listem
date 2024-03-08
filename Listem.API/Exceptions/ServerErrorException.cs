using System.Net;

namespace Listem.API.Exceptions;

public class ServerErrorException(string message, object? value = null)
    : HttpResponseException(
        HttpStatusCode.InternalServerError,
        message,
        "Internal Server Error",
        value
    );
