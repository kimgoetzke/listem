using System.Text;

namespace Listem.API.Middleware;

public class RequestMiddleware(RequestDelegate next, ILogger<RequestMiddleware> logger)
{
    public async Task InvokeAsync(HttpContext context, IRequestContext requestContext)
    {
        var start = TimeProvider.System.GetTimestamp();
        if (context.Request.Path.StartsWithSegments("/api"))
        {
            requestContext.Set(context.User);
        }

        logger.LogInformation(
            "[BEFORE REQUEST] {RequestId}: {Method} {Path} by user {UserId} with email {UserEmail}",
            requestContext.RequestId,
            context.Request.Method,
            context.Request.Path,
            requestContext.UserId == "" ? "(null)" : requestContext.UserId,
            requestContext.UserEmail == "" ? "(null)" : requestContext.UserEmail
        );

        if (
            Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") == "Development"
            && context.Request.Method == HttpMethods.Post
        )
        {
            context.Request.EnableBuffering();
            using (var r = new StreamReader(context.Request.Body, Encoding.UTF8, true, 1024, true))
            {
                var body = await r.ReadToEndAsync();
                logger.LogInformation("Request body (logged in development only):\n{Body}", body);
            }
            context.Request.Body.Position = 0;
        }

        try
        {
            await next(context);
        }
        finally
        {
            var duration = TimeProvider.System.GetElapsedTime(start);
            logger.LogInformation(
                "[AFTER REQUEST] {RequestId}: {Method} {Path} responded {StatusCode} and took {Duration} ms",
                requestContext.RequestId,
                context.Request.Method,
                context.Request.Path,
                context.Response.StatusCode,
                (int)duration.TotalMilliseconds
            );
        }
    }
}

public static class RequestLoggerMiddlewareExtensions
{
    public static void UseRequestMiddleware(this IApplicationBuilder builder)
    {
        builder.UseMiddleware<RequestMiddleware>();
    }
}
