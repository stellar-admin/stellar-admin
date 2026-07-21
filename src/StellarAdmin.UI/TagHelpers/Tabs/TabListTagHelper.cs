using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Razor.TagHelpers;
using StellarAdmin.UI.Theming;

namespace StellarAdmin.UI.TagHelpers;

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

    public TabListTagHelper(ICssClassMerger classMerger)
        : base(classMerger) { }

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

        output.Attributes.SetAttribute(
            "class",
            ClassMerger.Merge(
                new ThemeToken("sa-tabs"),
                "group/tabs flex data-[orientation=horizontal]:flex-col",
                output.GetUserSuppliedClass()
            )
        );

        var tabListTagBuilder = new TagBuilder("div");
        tabListTagBuilder.Attributes.Add("data-slot", "tabs-list");
        tabListTagBuilder.Attributes.Add("data-variant", effectiveVariant.GetDataAttributeText());
        tabListTagBuilder.Attributes.Add(
            "class",
            ClassMerger.Merge(
                new ThemeToken("sa-tabs-list"),
                "group/tabs-list text-muted-foreground inline-flex w-fit items-center justify-center group-data-[orientation=vertical]/tabs:h-fit group-data-[orientation=vertical]/tabs:flex-col",
                effectiveVariant == TabListVariant.Default
                    ? new ThemeToken("sa-tabs-list-variant-default")
                    : new ThemeToken("sa-tabs-list-variant-line"),
                effectiveVariant == TabListVariant.Default ? "bg-muted" : "gap-1 bg-transparent",
                output.GetUserSuppliedClass()
            )
        );
        tabListTagBuilder.InnerHtml.AppendHtml(await output.GetChildContentAsync());

        output.Content.AppendHtml(tabListTagBuilder);
    }
}
