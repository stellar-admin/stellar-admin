using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using StellarAdmin.UI.Toasts;

namespace StellarAdmin.UI.IntegrationTests.Pages;

[IgnoreAntiforgeryToken]
public sealed class BookingsModel(IToastNotifier notifier) : PageModel
{
    public IActionResult OnPostRedirect()
    {
        notifier.Success("Booking saved");

        return RedirectToPage();
    }

    public IActionResult OnPostSave()
    {
        notifier.Success("Booking saved");

        return new OkResult();
    }
}
