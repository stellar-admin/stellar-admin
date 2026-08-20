using Microsoft.AspNetCore.Mvc.ViewEngines;
using Microsoft.AspNetCore.Razor.TagHelpers;
using StellarAdmin.TagHelpers;

namespace DocsSamples.TagHelpers;

// Demonstrates StellarAdminTemplatedTagHelperBase: the markup lives in
// Pages/Shared/_TripCard.cshtml and resolves through normal view discovery, so an
// application could override it by supplying its own view with the same name.
[HtmlTargetElement("docs-trip-card")]
public class TripCardTagHelper : StellarAdminTemplatedTagHelperBase
{
    [HtmlAttributeName("booking")]
    public Booking? Booking { get; set; }

    protected override string ViewName => "_TripCard";

    public TripCardTagHelper(ICompositeViewEngine viewEngine)
        : base(viewEngine) { }

    protected override object? GetViewModel()
    {
        return Booking;
    }
}
