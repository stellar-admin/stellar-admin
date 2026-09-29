using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace StellarAdmin.TagHelpers.IntegrationTests.Pages;

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
