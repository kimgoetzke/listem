namespace Listem.API.Middleware;

public class RequestMiddleware(RequestDelegate next, ILogger<RequestMiddleware> logger)
{
    public async Task InvokeAsync(HttpContext context, IRequestContext requestContext)
    {
        var start = TimeProvider.System.GetTimestamp();
        requestContext.Set(context.User);

        logger.LogInformation(
            "[BEFORE REQUEST] {RequestId}: {Method} {Path} by user {UserId} with email {UserEmail}",
            requestContext.RequestId,
            context.Request.Method,
            context.Request.Path,
            requestContext.UserId,
            requestContext.UserEmail
        );
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
