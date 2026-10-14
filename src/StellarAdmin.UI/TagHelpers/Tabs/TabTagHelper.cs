using Microsoft.AspNetCore.Razor.TagHelpers;

namespace StellarAdmin.UI;

/// <summary>
///     A tab within the <c>&lt;sa-tab-list&gt;</c> of an <c>&lt;sa-tabs&gt;</c>, which shows
///     its panel when selected.
/// </summary>
[HtmlTargetElement("sa-tab")]
public class TabTagHelper : StellarAdminTagHelperBase
{
    /// <summary>
    ///     The id of the <c>&lt;sa-tab-panel&gt;</c> the tab shows.
    /// </summary>
    [HtmlAttributeName("for")]
    public string? For { get; set; }

    /// <summary>
    ///     Whether the tab is disabled and cannot be selected.
    /// </summary>
    /// <remarks>
    ///     Defaults to <c>false</c>.
    /// </remarks>
    [HtmlAttributeName("disabled")]
    public bool? IsDisabled { get; set; }

    public override async Task ProcessAsync(TagHelperContext context, TagHelperOutput output)
    {
        var tabs =
            GetContext<TabsContext>(context)
            ?? throw new InvalidOperationException("<sa-tab> must be inside <sa-tabs>.");
        if (string.IsNullOrWhiteSpace(For))
        {
            throw new InvalidOperationException(
                "<sa-tab> needs a for attribute naming the id of its <sa-tab-panel>."
            );
        }

        var tabId = output.Attributes["id"]?.Value?.ToString() is { Length: > 0 } id
            ? id
            : $"{For}-tab";
        tabs.TabIdsByPanelId[For] = tabId;
        tabs.SelectedPanelId ??= For;
        var isSelected = tabs.SelectedPanelId == For;

        output.TagName = "button";
        output.TagMode = TagMode.StartTagAndEndTag;
        output.Attributes.SetAttribute("type", "button");
        output.Attributes.SetAttribute("id", tabId);
        output.Attributes.SetAttribute("role", "tab");
        output.Attributes.SetAttribute("command", "--select-tab");
        output.Attributes.SetAttribute("commandfor", tabs.TabsId);
        output.Attributes.SetAttribute("aria-controls", For);
        output.Attributes.SetAttribute("aria-selected", isSelected ? "true" : "false");
        output.Attributes.SetAttribute("tabindex", isSelected ? "0" : "-1");
        output.Attributes.SetAttribute("data-slot", "tabs-trigger");
        if (isSelected)
        {
            output.Attributes.SetAttribute("data-active", "");
        }

        if (IsDisabled ?? false)
        {
            output.Attributes.SetAttribute("disabled", "disabled");
        }

        output.Attributes.SetAttribute(
            "class",
            JoinCssClasses("sa-tabs-trigger", output.GetUserSuppliedClass())
        );

        output.Content.SetHtmlContent(await output.GetChildContentAsync());
    }
}
