using System.Globalization;
using Microsoft.AspNetCore.Razor.TagHelpers;

namespace StellarAdmin.UI;

/// <summary>
///     Renders an item that can be selected in a toggle group.
/// </summary>
/// <remarks>
///     Place the item inside a <c>&lt;sa-toggle-group&gt;</c>. Attributes such as
///     <c>aria-label</c> apply to the item's visually hidden input.
/// </remarks>
[HtmlTargetElement("sa-toggle-group-item")]
public class ToggleGroupItemTagHelper : StellarAdminTagHelperBase
{
    /// <summary>
    ///     Gets or sets the value that the item posts when it is selected.
    /// </summary>
    [HtmlAttributeName("value")]
    public string? Value { get; set; }

    /// <summary>
    ///     Gets or sets the visual style of the item.
    /// </summary>
    /// <value>
    ///     One of the enumeration values that specifies the style. The default is the group's
    ///     style.
    /// </value>
    [HtmlAttributeName("variant")]
    public ToggleVariant? Variant { get; set; }

    /// <summary>
    ///     Gets or sets the size of the item.
    /// </summary>
    /// <value>
    ///     One of the enumeration values that specifies the size. The default is the group's size.
    /// </value>
    [HtmlAttributeName("size")]
    public ToggleSize? Size { get; set; }

    /// <summary>
    ///     Gets or sets a value that indicates whether the item is selected.
    /// </summary>
    /// <value>
    ///     <see langword="true" /> if the item is selected; otherwise, <see langword="false" />.
    ///     By default, the item is selected when the group's bound value contains the item's
    ///     value.
    /// </value>
    [HtmlAttributeName("selected")]
    public bool? Selected { get; set; }

    /// <summary>
    ///     Gets or sets a value that indicates whether the item is disabled.
    /// </summary>
    /// <value>
    ///     <see langword="true" /> if the item is disabled and can't be selected; otherwise,
    ///     <see langword="false" />. The default is <see langword="false" />.
    /// </value>
    [HtmlAttributeName("disabled")]
    public bool? Disabled { get; set; }

    /// <inheritdoc />
    public override async Task ProcessAsync(TagHelperContext context, TagHelperOutput output)
    {
        // The parent group resolves and publishes the shared configuration; the item only
        // contributes its own value and any per-item variant/size override.
        var toggleGroupContext = GetContext<ToggleGroupContext>(context);

        var effectiveVariant = Variant ?? toggleGroupContext?.Variant ?? ToggleVariant.Default;
        var effectiveSize = Size ?? toggleGroupContext?.Size ?? ToggleSize.Default;
        var type = toggleGroupContext?.Type ?? ToggleGroupType.Single;

        // Capture the author's content (icon/text) before we repurpose the host element.
        var childContent = await output.GetChildContentAsync();
        var userClass = output.GetUserSuppliedClass();

        /*
         * Single-select uses radios (native arrow-key roving + one value); multi-select uses
         * checkboxes that share a name and post a collection. Seeding the input from the host
         * attributes routes author extras (e.g. an icon-only item's aria-label) onto the
         * focusable control. The value posts back with no JavaScript.
         */
        var inputOutput = new TagHelperOutput(
            "input",
            new TagHelperAttributeList(output.Attributes),
            (_, _) => Task.FromResult<TagHelperContent>(new DefaultTagHelperContent())
        )
        {
            TagMode = TagMode.StartTagOnly,
        };
        inputOutput.Attributes.SetAttribute(
            "type",
            type == ToggleGroupType.Single ? "radio" : "checkbox"
        );
        inputOutput.Attributes.SetAttribute("data-slot", "toggle-group-item-input");

        if (!string.IsNullOrEmpty(toggleGroupContext?.FieldName))
        {
            inputOutput.Attributes.SetAttribute("name", toggleGroupContext.FieldName);
        }

        if (Value != null)
        {
            inputOutput.Attributes.SetAttribute("value", Value);
        }

        if (
            Selected
            ?? (
                Value != null
                && toggleGroupContext != null
                && toggleGroupContext.IsItemSelected(Value)
            )
        )
        {
            inputOutput.Attributes.SetAttribute("checked", "checked");
        }

        if (Disabled == true)
        {
            inputOutput.Attributes.SetAttribute("disabled", "disabled");
        }
        else if (toggleGroupContext != null)
        {
            toggleGroupContext.HasEnabledItem = true;
        }

        // Visually hidden but still focusable so native keyboard selection drives has-* state.
        inputOutput.Attributes.SetAttribute("class", "sr-only");

        output.Attributes.Clear();
        output.TagName = "label";
        output.TagMode = TagMode.StartTagAndEndTag;
        output.Attributes.SetAttribute("data-slot", "toggle-group-item");
        output.Attributes.SetAttribute("data-variant", effectiveVariant.GetDataAttributeText());
        output.Attributes.SetAttribute("data-size", effectiveSize.GetDataAttributeText());
        // The first/last edge-rounding variants (data-[spacing=0]:first:rounded-l-md) read
        // data-spacing off the item itself, so mirror the group's spacing onto each item.
        output.Attributes.SetAttribute(
            "data-spacing",
            (toggleGroupContext?.Spacing ?? 0).ToString(CultureInfo.InvariantCulture)
        );
        output.Attributes.SetAttribute(
            "class",
            ToggleRenderingHelper.BuildClass(
                effectiveVariant,
                effectiveSize,
                includeGroupItemToken: true,
                JoinCssClasses(toggleGroupContext?.ClassNames?.Item, userClass)
            )
        );

        output.Content.AppendHtml(inputOutput);
        output.Content.AppendHtml(childContent);
    }
}
