using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using StellarAdmin.UI.Toasts;

namespace StellarAdmin.UI.IntegrationTests.Pages;

[IgnoreAntiforgeryToken]
public sealed class ToasterModel(IToastNotifier notifier) : PageModel
{
    public void OnGet() { }

    public IActionResult OnPostRedirect()
    {
        notifier.Success("Booking saved");

        return RedirectToPage();
    }
}
