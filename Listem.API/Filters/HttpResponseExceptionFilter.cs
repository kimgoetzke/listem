using Listem.API.Exceptions;
using Listem.API.Utilities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.Infrastructure;

// ReSharper disable ClassNeverInstantiated.Global

namespace Listem.API.Filters;

public class HttpResponseExceptionFilter(ProblemDetailsFactory factory)
    : IActionFilter,
        IOrderedFilter
{
    public int Order => int.MaxValue - 10;

    public void OnActionExecuting(ActionExecutingContext context) { }

    public void OnActionExecuted(ActionExecutedContext context)
    {
        if (context.Exception is not HttpResponseException httpResponseException)
            return;

        Logger.Log(httpResponseException.Message);
        var problemDetails = factory.CreateProblemDetails(
            context.HttpContext,
            (int)httpResponseException.StatusCode,
            httpResponseException.Title,
            null,
            httpResponseException.Message
        );
        context.Result = new ObjectResult(problemDetails)
        {
            StatusCode = (int)httpResponseException.StatusCode,
            ContentTypes = { "application/json" }
        };
        context.ExceptionHandled = true;
    }
}
