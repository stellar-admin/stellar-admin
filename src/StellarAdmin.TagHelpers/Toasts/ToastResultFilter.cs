using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.Extensions.DependencyInjection;

namespace StellarAdmin.TagHelpers;

// Decides where queued toasts go. This runs before the result executes, so the response has not
// started and MVC has not yet saved TempData:
// - Redirects keep toasts in TempData for the next response.
// - Navigations (or requests without Sec-Fetch-Mode) keep them in TempData for <sa-toaster>.
// - Everything else (fetch, XHR) drains them into the SA-Toasts header.
internal sealed class ToastResultFilter : IAlwaysRunResultFilter
{
    public void OnResultExecuted(ResultExecutedContext context) { }

    public void OnResultExecuting(ResultExecutingContext context)
    {
        if (context.Result is IKeepTempDataResult || IsNavigation(context.HttpContext.Request))
        {
            return;
        }

        var response = context.HttpContext.Response;
        if (response.HasStarted)
        {
            return;
        }

        var tempData = context
            .HttpContext.RequestServices.GetRequiredService<ITempDataDictionaryFactory>()
            .GetTempData(context.HttpContext);
        var header = ToastQueue.DrainToHeader(tempData, ToastQueue.MaxHeaderBytes);
        if (header is not null)
        {
            response.Headers[ToastQueue.HeaderName] = header;
        }
    }

    private static bool IsNavigation(Microsoft.AspNetCore.Http.HttpRequest request)
    {
        var mode = request.Headers["Sec-Fetch-Mode"].ToString();

        return mode.Length == 0
            || string.Equals(mode, "navigate", StringComparison.OrdinalIgnoreCase);
    }
}
