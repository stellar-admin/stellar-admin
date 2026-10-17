using Microsoft.AspNetCore.Razor.TagHelpers;

namespace StellarAdmin.UI;

/// <summary>
///     Renders a tab that shows its panel when selected.
/// </summary>
/// <remarks>
///     Place the tab in the <c>&lt;sa-tab-list&gt;</c> of a <c>&lt;sa-tabs&gt;</c>.
/// </remarks>
[HtmlTargetElement("sa-tab")]
public sealed class TabTagHelper : StellarAdminTagHelperBase
{
    /// <summary>
    ///     Gets or sets the <c>id</c> of the <c>&lt;sa-tab-panel&gt;</c> that the tab shows.
    /// </summary>
    /// <remarks>
    ///     Required.
    /// </remarks>
    [HtmlAttributeName("for")]
    public string? For { get; set; }

    /// <summary>
    ///     Gets or sets a value that indicates whether the tab is disabled.
    /// </summary>
    /// <value>
    ///     <see langword="true" /> if the tab is disabled and can't be selected; otherwise,
    ///     <see langword="false" />. The default is <see langword="false" />.
    /// </value>
    [HtmlAttributeName("disabled")]
    public bool? Disabled { get; set; }

    /// <inheritdoc />
    /// <exception cref="InvalidOperationException">
    ///     The tab isn't inside a <c>&lt;sa-tabs&gt;</c>, or <see cref="For" /> isn't set.
    /// </exception>
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

        if (Disabled ?? false)
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
