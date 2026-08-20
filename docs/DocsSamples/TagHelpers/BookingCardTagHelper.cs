using Microsoft.AspNetCore.Mvc.ViewEngines;
using Microsoft.AspNetCore.Razor.TagHelpers;
using StellarAdmin.TagHelpers;

namespace DocsSamples.TagHelpers;

// Demonstrates a templated tag helper whose view renders caller-supplied content
// through a slot: Pages/Shared/_BookingCard.cshtml places an <sa-slot-outlet> named
// "actions" in the card footer, with a default button as fallback.
[HtmlTargetElement("docs-booking-card")]
public class BookingCardTagHelper : StellarAdminTemplatedTagHelperBase
{
    [HtmlAttributeName("booking")]
    public Booking? Booking { get; set; }

    protected override string ViewName => "_BookingCard";

    public BookingCardTagHelper(ICompositeViewEngine viewEngine)
        : base(viewEngine) { }

    protected override object? GetViewModel()
    {
        return Booking;
    }
}
