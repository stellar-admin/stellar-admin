using Microsoft.AspNetCore.Razor.TagHelpers;

namespace StellarAdmin.TagHelpers;

/// <summary>
///     A wrapper for the main content of a page. It centers the content horizontally,
///     applies a consistent gutter, spaces its children vertically, and can constrain
///     the content to a maximum width appropriate for the type of page.
/// </summary>
[HtmlTargetElement("sa-page-container")]
public class PageContainerTagHelper : StellarAdminTagHelperBase
{
    /// <summary>
    ///     The maximum width to which the content is constrained.
    /// </summary>
    /// <remarks>
    ///     Defaults to <see cref="PageContainerWidth.Full" />.
    /// </remarks>
    [HtmlAttributeName("width")]
    public PageContainerWidth? Width { get; set; }

    public override Task ProcessAsync(TagHelperContext context, TagHelperOutput output)
    {
        var effectiveWidth = Width ?? PageContainerWidth.Full;

        output.TagName = "div";
        output.TagMode = TagMode.StartTagAndEndTag;

        output.Attributes.SetAttribute("data-slot", "page-container");
        output.Attributes.SetAttribute("data-width", effectiveWidth.GetDataAttributeText());

        output.Attributes.SetAttribute(
            "class",
            JoinCssClasses(
                "sa-page-container",
                effectiveWidth.GetWidthCssClass(),
                output.GetUserSuppliedClass()
            )
        );

        return Task.CompletedTask;
    }
}
