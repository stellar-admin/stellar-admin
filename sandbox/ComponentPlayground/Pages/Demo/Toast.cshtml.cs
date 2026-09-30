using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using StellarAdmin.TagHelpers;

namespace ComponentPlayground.Pages.Demo;

public class ToastPage(IToastNotifier notifier) : PageModel
{
    public void OnGet() { }

    // fetch: an info toast with a link action and a longer duration.
    public IActionResult OnPostArchive()
    {
        notifier.Add(
            new Toast
            {
                Title = "Itinerary archived",
                Description = "3 bookings moved to the archive.",
                Type = ToastType.Info,
                Duration = TimeSpan.FromSeconds(8),
                Action = ToastAction.Link("Undo", Url.Page(null, "Restore")!),
            }
        );

        return new NoContentResult();
    }

    // The Undo link navigates here, so the toast comes back on the redirected page.
    public IActionResult OnGetRestore()
    {
        notifier.Success("Itinerary restored", "3 bookings are back in your trips.");

        return RedirectToPage();
    }

    // fetch: several toasts in one response header.
    public IActionResult OnPostCheckIn()
    {
        notifier.Success("Checked in", "Boarding pass sent for Amara Okafor.");
        notifier.Success("Checked in", "Boarding pass sent for Tomás Reyes.");
        notifier.Warning("Seat not assigned", "Priya Nair will be seated at the gate.");

        return new NoContentResult();
    }

    // Plain form post: the redirect carries the toast to the next full page load.
    public IActionResult OnPostRedirect()
    {
        notifier.Success("Itinerary sent", "We emailed the Kyoto itinerary to 4 travellers.");

        return RedirectToPage();
    }

    // htmx post that redirects: fetch follows the redirect, and the toast arrives in the
    // header of the Status response.
    public IActionResult OnPostRedirectHtmx()
    {
        notifier.Info("Reminder scheduled", "Travellers get a reminder 2 days before departure.");

        return RedirectToPage(new { handler = "Status" });
    }

    public IActionResult OnGetStatus() =>
        Content($"Reminder scheduled at {DateTime.Now:HH:mm:ss}.");

    // htmx form post that returns a fragment along with a toast.
    public IActionResult OnPostSave(string? traveller)
    {
        if (string.IsNullOrWhiteSpace(traveller))
        {
            notifier.Error("Booking not saved", "Enter the traveller's name.");

            return Content("Not saved.");
        }

        notifier.Success("Booking saved", $"{traveller.Trim()}'s trip to Lisbon is confirmed.");

        return Content($"Saved {traveller.Trim()} at {DateTime.Now:HH:mm:ss}.");
    }
}
