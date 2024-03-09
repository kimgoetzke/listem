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
            logger.LogInformation("Handling {ExceptionType}: {Message}", ex.GetType(), ex.Message);
            await ProcessException(context, ex.Message, ex.StatusCode, ex.Title);
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "An unhandled exception occurred");
            await ProcessException(context, ex.Message);
        }
    }

    private static async Task ProcessException(
        HttpContext context,
        string detail,
        HttpStatusCode? statusCode = null,
        string? exceptionTitle = null
    )
    {
        var status = (int)(statusCode ?? HttpStatusCode.InternalServerError);
        var title = exceptionTitle ?? "Internal Server Error";
        var details = new ProblemDetails
        {
            Type = "https://httpstatuses.com/" + status,
            Status = status,
            Title = title,
            Detail = detail
        };
        context.Response.ContentType = "application/problem+json";
        context.Response.StatusCode = status;
        var json = JsonSerializer.Serialize(details);
        await context.Response.WriteAsync(json);
    }
}

public static class HttpExceptionHandlerMiddlewareExtensions
{
    public static void UseHttpExceptionHandler(this IApplicationBuilder builder)
    {
        builder.UseMiddleware<HttpExceptionHandlerMiddleware>();
    }
}
