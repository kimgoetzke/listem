using System.Net;
using System.Text.Json;
using Listem.API.Exceptions;
using Microsoft.AspNetCore.Mvc;

namespace Listem.API.Middleware;

public class HttpExceptionHandlerMiddleware(
    RequestDelegate next,
    ILogger<HttpExceptionHandlerMiddleware> logger
)
{
    public async Task InvokeAsync(HttpContext context, IRequestContext requestContext)
    {
        try
        {
            await next(context);
        }
        catch (HttpResponseException ex)
        {
            var jsonError = ExceptionToJson(context, ex.Message, ex.StatusCode, ex.Title);
            await context.Response.WriteAsync(jsonError);
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "An unhandled exception occurred");
            var jsonError = ExceptionToJson(context, ex.Message);
            await context.Response.WriteAsync(jsonError);
        }
    }

    private string ExceptionToJson(
        HttpContext context,
        string detail,
        HttpStatusCode? statusCode = null,
        string? exceptionTitle = null
    )
    {
        context.Response.ContentType = "application/problem+json";
        context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
        var status = (int)(statusCode ?? HttpStatusCode.InternalServerError);
        var title = exceptionTitle ?? "Internal Server Error";
        var details = new ProblemDetails
        {
            Type = "https://httpstatuses.com/" + status,
            Status = status,
            Title = title,
            Detail = detail
        };
        return JsonSerializer.Serialize(details);
    }
}

public static class HttpExceptionHandlerMiddlewareExtensions
{
    public static void UseHttpExceptionHandler(this IApplicationBuilder builder)
    {
        builder.UseMiddleware<HttpExceptionHandlerMiddleware>();
    }
}
