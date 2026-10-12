using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Razor.TagHelpers;

namespace StellarAdmin.TagHelpers;

/// <summary>
///     A list of tabs: <c>&lt;sa-tab-link&gt;</c>s, each linking to a different view or page, or
///     inside an <c>&lt;sa-tabs&gt;</c>, <c>&lt;sa-tab&gt;</c>s that switch between panels.
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
            JoinCssClasses("sa-tabs", output.GetUserSuppliedClass())
        );

        var tabListTagBuilder = new TagBuilder("div");
        tabListTagBuilder.Attributes.Add("data-slot", "tabs-list");
        tabListTagBuilder.Attributes.Add("data-variant", effectiveVariant.GetDataAttributeText());
        // Only tabs that switch panels form a tab list; link tabs keep link semantics.
        if (GetContext<TabsContext>(context) is not null)
        {
            tabListTagBuilder.Attributes.Add("role", "tablist");
            tabListTagBuilder.Attributes.Add(
                "aria-orientation",
                effectiveOrientation.GetDataAttributeText()
            );
        }
        tabListTagBuilder.Attributes.Add(
            "class",
            JoinCssClasses(
                "sa-tabs-list",
                effectiveVariant == TabListVariant.Default
                    ? "sa-tabs-list-variant-default"
                    : "sa-tabs-list-variant-line"
            )
        );
        tabListTagBuilder.InnerHtml.AppendHtml(await output.GetChildContentAsync());

        output.Content.AppendHtml(tabListTagBuilder);
    }
}
