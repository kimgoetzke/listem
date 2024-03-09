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
    public async Task InvokeAsync(HttpContext httpContext, IRequestContext requestContext)
    {
        try
        {
            await next(httpContext);
        }
        catch (HttpResponseException ex)
        {
            logger.LogInformation(
                "Handling {ExceptionType} with message '{Message}' for {RequestId}",
                ex.GetType(),
                ex.Message,
                requestContext.RequestId
            );
            await ProcessException(httpContext, ex.Message, ex.StatusCode, ex.Title);
        }
        catch (Exception ex)
        {
            logger.LogWarning(
                ex,
                "An unhandled exception occurred while processing {RequestId}",
                requestContext.RequestId
            );
            await ProcessException(httpContext, ex.Message);
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
