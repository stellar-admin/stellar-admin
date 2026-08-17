using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Razor.TagHelpers;

namespace StellarAdmin.TagHelpers;

/// <summary>
///     A list of tabs, each linking to a different view or page.
/// </summary>
[HtmlTargetElement("sa-tab-list")]
public class TabListTagHelper : StellarAdminTagHelperBase
{
    /// <summary>
    ///     The orientation in which the tabs are arranged.
    /// </summary>
    /// <remarks>
    ///     Defaults to <see cref="TabListOrientation.Horizontal" />.
    /// </remarks>
    [HtmlAttributeName("orientation")]
    public TabListOrientation? Orientation { get; set; }

    /// <summary>
    ///     The visual style of the tab list.
    /// </summary>
    /// <remarks>
    ///     Defaults to <see cref="TabListVariant.Default" />.
    /// </remarks>
    [HtmlAttributeName("variant")]
    public TabListVariant? Variant { get; set; }

    public override async Task ProcessAsync(TagHelperContext context, TagHelperOutput output)
    {
        var effectiveOrientation = Orientation ?? TabListOrientation.Horizontal;
        var effectiveVariant = Variant ?? TabListVariant.Default;

        output.TagName = "div";
        output.TagMode = TagMode.StartTagAndEndTag;

        output.Attributes.SetAttribute("data-slot", "tabs");
        output.Attributes.SetAttribute(
            "data-orientation",
            effectiveOrientation.GetDataAttributeText()
        );

        // The author's class belongs to the host only; the inner list carries its own classes.
        output.Attributes.SetAttribute(
            "class",
            JoinCssClasses("sa-tabs", "group/tabs", output.GetUserSuppliedClass())
        );

        var tabListTagBuilder = new TagBuilder("div");
        tabListTagBuilder.Attributes.Add("data-slot", "tabs-list");
        tabListTagBuilder.Attributes.Add("data-variant", effectiveVariant.GetDataAttributeText());
        tabListTagBuilder.Attributes.Add(
            "class",
            JoinCssClasses(
                "sa-tabs-list",
                "group/tabs-list",
                effectiveVariant == TabListVariant.Default
                    ? "sa-tabs-list-variant-default"
                    : "sa-tabs-list-variant-line"
            )
        );
        tabListTagBuilder.InnerHtml.AppendHtml(await output.GetChildContentAsync());

        output.Content.AppendHtml(tabListTagBuilder);
    }
}
