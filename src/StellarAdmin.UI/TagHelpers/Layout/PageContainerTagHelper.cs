using Microsoft.AspNetCore.Razor.TagHelpers;

namespace StellarAdmin.UI;

/// <summary>
///     Renders a container for the main content of a page.
/// </summary>
/// <remarks>
///     The container centers the content, adds a gutter, spaces its children vertically and can
///     limit the content's width.
/// </remarks>
[HtmlTargetElement("sa-page-container")]
public sealed class PageContainerTagHelper : StellarAdminTagHelperBase
{
    /// <summary>
    ///     Gets or sets the maximum width of the content.
    /// </summary>
    /// <value>
    ///     One of the enumeration values that specifies the maximum width. The default is
    ///     <see cref="PageContainerWidth.Full" />.
    /// </value>
    [HtmlAttributeName("width")]
    public PageContainerWidth? Width { get; set; }

    /// <inheritdoc />
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
