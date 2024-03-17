using System.Net;

namespace Listem.API.Exceptions;

public class BadRequestException(string message, object? value = null)
    : HttpResponseException(HttpStatusCode.BadRequest, message, "Bad Request", value);
