using Microsoft.AspNetCore.Mvc;

namespace StellarAdmin.TagHelpers.IntegrationTests.Fixtures;

[Route("toasts")]
public sealed class ToastsController(IToastNotifier notifier) : Controller
{
    [HttpPost("detailed")]
    public IActionResult Detailed()
    {
        notifier.Add(
            new Toast
            {
                Title = "Itinerary archived",
                Description = "3 bookings moved to archive.",
                Type = ToastType.Info,
                Duration = TimeSpan.FromSeconds(8),
                Action = ToastAction.Link("Undo", "/itineraries/42/restore"),
            }
        );

        return Ok();
    }

    [HttpPost("many")]
    public IActionResult Many(int count, int descriptionLength)
    {
        for (var i = 1; i <= count; i++)
        {
            notifier.Info($"Toast {i}", new string('x', descriptionLength));
        }

        return Ok();
    }

    [HttpGet("none")]
    public IActionResult NoToasts() => Ok();

    [HttpPost("redirect")]
    public IActionResult SaveAndRedirect()
    {
        notifier.Success("Booking saved");

        return RedirectToAction(nameof(NoToasts));
    }

    [HttpPost("success")]
    public IActionResult Success()
    {
        notifier.Success("Booking saved", "Your trip to Lisbon is confirmed.");

        return Ok();
    }

    [HttpPost("unicode")]
    public IActionResult Unicode()
    {
        notifier.Success("Café in Zürich ✓", "<b>\"Booked\"</b>");

        return Ok();
    }
}
