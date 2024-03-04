using System.Net;

namespace Listem.API.Exceptions;

public class NotFoundException(string message, object? value = null)
    : HttpResponseException(HttpStatusCode.NotFound, message, "Not Found", value);
