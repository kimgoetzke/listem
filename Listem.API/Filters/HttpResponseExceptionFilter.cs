using Listem.API.Exceptions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace Listem.API.Filters;

public class HttpResponseExceptionFilter : IActionFilter, IOrderedFilter
{
    public int Order => int.MaxValue - 10;

    public void OnActionExecuting(ActionExecutingContext context) { }

    public void OnActionExecuted(ActionExecutedContext context)
    {
        if (context.Exception is not HttpResponseException httpResponseException)
            return;

        context.Result = new ObjectResult(httpResponseException.Value)
        {
            StatusCode = httpResponseException.StatusCode
        };

        context.ExceptionHandled = true;
    }
}
