using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Razor.TagHelpers;

namespace StellarAdmin.UI;

/// <summary>
///     Renders a list of tabs.
/// </summary>
/// <remarks>
///     The list holds <c>&lt;sa-tab-link&gt;</c> elements that link to other pages. Inside a
///     <c>&lt;sa-tabs&gt;</c>, it holds <c>&lt;sa-tab&gt;</c> elements that switch between
///     panels.
/// </remarks>
[HtmlTargetElement("sa-tab-list")]
public class TabListTagHelper : StellarAdminTagHelperBase
{
    /// <summary>
    ///     Gets or sets the direction in which the tabs are arranged.
    /// </summary>
    /// <value>
    ///     One of the enumeration values that specifies the direction. The default is
    ///     <see cref="TabListOrientation.Horizontal" />.
    /// </value>
    [HtmlAttributeName("orientation")]
    public TabListOrientation? Orientation { get; set; }

    /// <summary>
    ///     Gets or sets the visual style of the tab list.
    /// </summary>
    /// <value>
    ///     One of the enumeration values that specifies the style. The default is
    ///     <see cref="TabListVariant.Default" />.
    /// </value>
    [HtmlAttributeName("variant")]
    public TabListVariant? Variant { get; set; }

    /// <inheritdoc />
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
