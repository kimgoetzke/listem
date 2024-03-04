using System.Net;

namespace Listem.API.Exceptions;

public class ConflictException(string message, object? value = null)
    : HttpResponseException(HttpStatusCode.Conflict, message, "Conflict", value);
