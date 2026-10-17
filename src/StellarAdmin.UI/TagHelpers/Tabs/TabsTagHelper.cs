using Microsoft.AspNetCore.Razor.TagHelpers;

namespace StellarAdmin.UI;

/// <summary>
///     Renders tabs that switch between panels on the page.
/// </summary>
/// <remarks>
///     Place a <c>&lt;sa-tab-list&gt;</c> of <c>&lt;sa-tab&gt;</c> elements inside it, and a
///     <c>&lt;sa-tab-panel&gt;</c> for each tab. To load a panel's content when its tab is first
///     selected, add htmx attributes with <c>hx-trigger="tabshow once"</c> to the
///     <c>&lt;sa-tab-panel&gt;</c>. Requires <c>stellar-admin.js</c>.
/// </remarks>
[HtmlTargetElement("sa-tabs")]
public sealed class TabsTagHelper : StellarAdminTagHelperBase
{
    /// <summary>
    ///     Gets or sets the <c>id</c> of the panel that is shown first.
    /// </summary>
    /// <value>
    ///     The <c>id</c> of a <c>&lt;sa-tab-panel&gt;</c>. The default is the first tab's panel.
    /// </value>
    [HtmlAttributeName("selected")]
    public string? Selected { get; set; }

    /// <inheritdoc />
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
