using Microsoft.AspNetCore.Razor.TagHelpers;
using StellarAdmin.TagHelpers;

namespace DocsSamples.TagHelpers;

// Demonstrates hosting named slots in a custom tag helper: executing the child content
// runs the <sa-slot-content> children, which assign their content to this tag helper;
// TryGetNamedSlot then reads a slot back so it can render in a spot of the tag
// helper's choosing - here the header, while the remaining children become the body.
[HtmlTargetElement("docs-booking-summary")]
public class BookingSummaryTagHelper : StellarAdminTagHelperBase
{
    [HtmlAttributeName("booking")]
    public Booking? Booking { get; set; }

    public override async Task ProcessAsync(TagHelperContext context, TagHelperOutput output)
    {
        var childContent = await output.GetChildContentAsync();

        output.TagName = "div";
        output.Attributes.SetAttribute("class", "w-full max-w-sm rounded-lg border");

        output.Content.AppendHtml("<div class=\"flex items-center justify-between border-b p-3\">");
        output.Content.AppendHtml("<span class=\"font-medium\">");
        output.Content.Append(Booking?.Destination ?? string.Empty);
        output.Content.AppendHtml("</span>");

        if (TryGetNamedSlot("actions", out var actions))
        {
            output.Content.AppendHtml(actions);
        }

        output.Content.AppendHtml("</div>");
        output.Content.AppendHtml("<div class=\"p-3 text-sm\">");
        output.Content.AppendHtml(childContent);
        output.Content.AppendHtml("</div>");
    }
}
