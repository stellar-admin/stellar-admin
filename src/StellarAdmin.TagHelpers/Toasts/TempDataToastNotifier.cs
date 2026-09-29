using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.ViewFeatures;

namespace StellarAdmin.TagHelpers;

internal sealed class TempDataToastNotifier(
    ITempDataDictionaryFactory tempDataFactory,
    IHttpContextAccessor httpContextAccessor
) : IToastNotifier
{
    public void Add(Toast toast)
    {
        ArgumentNullException.ThrowIfNull(toast);
        ArgumentException.ThrowIfNullOrWhiteSpace(toast.Title, nameof(toast));
        if (toast.Duration < TimeSpan.Zero)
        {
            throw new ArgumentOutOfRangeException(
                nameof(toast),
                toast.Duration,
                "The toast duration cannot be negative."
            );
        }

        var httpContext =
            httpContextAccessor.HttpContext
            ?? throw new InvalidOperationException("Toasts can only be added during a request.");

        ToastQueue.Enqueue(tempDataFactory.GetTempData(httpContext), toast);
    }
}
