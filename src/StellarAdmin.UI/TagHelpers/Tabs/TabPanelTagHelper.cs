using Microsoft.AspNetCore.Razor.TagHelpers;

namespace StellarAdmin.UI;

/// <summary>
///     The panel an <c>&lt;sa-tab&gt;</c> of an <c>&lt;sa-tabs&gt;</c> shows. Its <c>id</c> is
///     the one the tab's <c>for</c> attribute names.
/// </summary>
[HtmlTargetElement("sa-tab-panel")]
public class TabPanelTagHelper : StellarAdminTagHelperBase
{
    public override async Task ProcessAsync(TagHelperContext context, TagHelperOutput output)
    {
        var tabs =
            GetContext<TabsContext>(context)
            ?? throw new InvalidOperationException("<sa-tab-panel> must be inside <sa-tabs>.");
        if (output.Attributes["id"]?.Value?.ToString() is not { Length: > 0 } panelId)
        {
            throw new InvalidOperationException(
                "<sa-tab-panel> needs an id that the for attribute of an <sa-tab> names."
            );
        }

        tabs.SelectedPanelId ??= panelId;
        var isSelected = tabs.SelectedPanelId == panelId;

        output.TagName = "div";
        output.TagMode = TagMode.StartTagAndEndTag;
        output.Attributes.SetAttribute("role", "tabpanel");
        output.Attributes.SetAttribute(
            "aria-labelledby",
            tabs.TabIdsByPanelId.GetValueOrDefault(panelId, $"{panelId}-tab")
        );
        output.Attributes.SetAttribute("tabindex", "0");
        output.Attributes.SetAttribute("data-slot", "tabs-content");
        if (!isSelected)
        {
            output.Attributes.SetAttribute("hidden", "hidden");
        }

        output.Attributes.SetAttribute(
            "class",
            JoinCssClasses("sa-tab-panel", output.GetUserSuppliedClass())
        );

        // The panel's content is outside these tabs: a link tab list in it is not a tab list of panels.
        context.Items.Remove(typeof(TabsContext));
        output.Content.SetHtmlContent(await output.GetChildContentAsync());
    }
}
