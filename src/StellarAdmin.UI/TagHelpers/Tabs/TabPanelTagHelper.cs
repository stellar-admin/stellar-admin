using Microsoft.AspNetCore.Razor.TagHelpers;

namespace StellarAdmin.UI;

/// <summary>
///     Renders the panel that a tab shows.
/// </summary>
/// <remarks>
///     Place the panel inside a <c>&lt;sa-tabs&gt;</c>. It needs an <c>id</c>, which the
///     <c>for</c> attribute of its <c>&lt;sa-tab&gt;</c> names. The panel raises the bubbling
///     <c>tabshow</c> and <c>tabhide</c> events when the user selects its tab or another tab.
///     The panel shown when the page loads doesn't raise <c>tabshow</c>.
/// </remarks>
[HtmlTargetElement("sa-tab-panel")]
public sealed class TabPanelTagHelper : StellarAdminTagHelperBase
{
    /// <inheritdoc />
    /// <exception cref="InvalidOperationException">
    ///     The panel isn't inside a <c>&lt;sa-tabs&gt;</c>, or it has no <c>id</c>.
    /// </exception>
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
