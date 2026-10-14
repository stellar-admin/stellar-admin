using Microsoft.AspNetCore.Razor.TagHelpers;

namespace StellarAdmin.UI;

/// <summary>
///     Tabs that switch between panels on the page. Place an <c>sa-tab-list</c> of
///     <c>sa-tab</c>s and an <c>sa-tab-panel</c> for each tab inside it. Renders the
///     <c>sel-tabs</c> web component, which raises bubbling <c>tab-show</c> and
///     <c>tab-hide</c> events on the panels, so htmx attributes on an <c>sa-tab-panel</c> can
///     load its content with <c>hx-trigger="tab-show once"</c>.
/// </summary>
[HtmlTargetElement("sa-tabs")]
public class TabsTagHelper : StellarAdminTagHelperBase
{
    /// <summary>
    ///     The id of the panel shown first.
    /// </summary>
    /// <remarks>
    ///     Defaults to the first tab's panel.
    /// </remarks>
    [HtmlAttributeName("selected")]
    public string? Selected { get; set; }

    public override async Task ProcessAsync(TagHelperContext context, TagHelperOutput output)
    {
        var tabsId = output.Attributes["id"]?.Value?.ToString() is { Length: > 0 } id
            ? id
            : $"--sa-tabs-{GetUniqueId(context)}";
        SetContext(
            context,
            new TabsContext
            {
                TabsId = tabsId,
                SelectedPanelId = string.IsNullOrWhiteSpace(Selected) ? null : Selected,
            }
        );

        output.TagName = "sel-tabs";
        output.TagMode = TagMode.StartTagAndEndTag;
        output.Attributes.SetAttribute("id", tabsId);
        output.Attributes.SetAttribute("data-slot", "tabs-container");
        output.Attributes.SetAttribute(
            "class",
            JoinCssClasses("sa-tabs-container", output.GetUserSuppliedClass())
        );

        output.Content.SetHtmlContent(await output.GetChildContentAsync());
    }
}
