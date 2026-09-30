using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace StellarAdmin.TagHelpers.IntegrationTests.Pages;

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
