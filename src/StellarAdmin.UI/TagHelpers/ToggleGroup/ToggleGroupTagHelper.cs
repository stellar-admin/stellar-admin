using System.Globalization;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Razor.TagHelpers;

namespace StellarAdmin.UI;

/// <summary>
///     Renders a group of toggle items that lets the user select one item or several.
/// </summary>
/// <remarks>
///     Place <c>&lt;sa-toggle-group-item&gt;</c> elements inside the group. Bind a single value
///     with <see cref="ToggleGroupType.Single" />, or a collection with
///     <see cref="ToggleGroupType.Multiple" />. The group works without JavaScript.
/// </remarks>
[HtmlTargetElement("sa-toggle-group")]
public class ToggleGroupTagHelper : FieldInputBaseTagHelper<ToggleGroupClassNames>
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="ToggleGroupTagHelper" /> class.
    /// </summary>
    /// <param name="htmlGenerator">The generator that renders the field's label and messages.</param>
    /// <exception cref="ArgumentNullException">
    ///     <paramref name="htmlGenerator" /> is <see langword="null" />.
    /// </exception>
    public ToggleGroupTagHelper(IHtmlGenerator htmlGenerator)
        : base(htmlGenerator) { }

    /// <summary>
    ///     Gets or sets how many items can be selected.
    /// </summary>
    /// <value>
    ///     One of the enumeration values that specifies how many items can be selected. The
    ///     default is <see cref="ToggleGroupType.Single" />.
    /// </value>
    [HtmlAttributeName("type")]
    public ToggleGroupType? Type { get; set; }

    /// <summary>
    ///     Gets or sets the visual style of the items.
    /// </summary>
    /// <value>
    ///     One of the enumeration values that specifies the style. The default is
    ///     <see cref="ToggleVariant.Default" />.
    /// </value>
    [HtmlAttributeName("variant")]
    public ToggleVariant? Variant { get; set; }

    /// <summary>
    ///     Gets or sets the size of the items.
    /// </summary>
    /// <value>
    ///     One of the enumeration values that specifies the size. The default is
    ///     <see cref="ToggleSize.Default" />.
    /// </value>
    [HtmlAttributeName("size")]
    public ToggleSize? Size { get; set; }

    /// <summary>
    ///     Gets or sets the direction in which the items are arranged.
    /// </summary>
    /// <value>
    ///     One of the enumeration values that specifies the direction. The default is
    ///     <see cref="ToggleGroupOrientation.Horizontal" />.
    /// </value>
    [HtmlAttributeName("orientation")]
    public ToggleGroupOrientation? Orientation { get; set; }

    /// <summary>
    ///     Gets or sets the space between the items.
    /// </summary>
    /// <value>
    ///     One of the enumeration values that specifies the space. The default is
    ///     <see cref="ToggleGroupSpacing.Default" />.
    /// </value>
    [HtmlAttributeName("spacing")]
    public ToggleGroupSpacing? Spacing { get; set; }

    /// <inheritdoc />
    protected override async Task<FieldLayout> RenderInput(
        TagHelperContext context,
        TagHelperOutput output,
        IDictionary<string, object?>? htmlAttributes
    )
    {
        // The group is the single authority for the items' configuration: it resolves every
        // effective value here and publishes them as a context the children consume.
        var effectiveType = Type ?? ToggleGroupType.Single;
        var effectiveVariant = Variant ?? ToggleVariant.Default;
        var effectiveSize = Size ?? ToggleSize.Default;
        var effectiveSpacing = (Spacing ?? ToggleGroupSpacing.Default).GetSpacingUnits();
        var effectiveOrientation = Orientation ?? ToggleGroupOrientation.Horizontal;

        // Resolve the shared field name once so every child input posts under the same name
        // (radios -> one scalar, checkboxes -> a collection).
        var fieldName =
            For == null ? Name : ViewContext.ViewData.TemplateInfo.GetFullHtmlFieldName(For.Name);
        var multiple = effectiveType == ToggleGroupType.Multiple;
        var valueType =
            For == null ? null
            : multiple ? ChoiceGroupValue.ElementType(For.ModelExplorer.ModelType)
            : For.ModelExplorer.ModelType;

        // Posted values win over the model so a redisplayed form keeps what was submitted
        var state = string.IsNullOrEmpty(fieldName)
            ? null
            : ViewContext.ViewData.ModelState[fieldName]?.RawValue;
        var hasState = state != null;
        var selection = ChoiceGroupValue
            .Values(hasState ? state : For?.Model)
            .Where(value => value != null)
            .Select(value => ChoiceGroupValue.Normalize(value!, valueType));
        var groupContext = new ToggleGroupContext
        {
            ClassNames = ClassNames,
            Type = effectiveType,
            Variant = effectiveVariant,
            Size = effectiveSize,
            Spacing = effectiveSpacing,
            FieldName = fieldName,
            SelectedValues = (hasState && !multiple ? selection.Take(1) : selection).ToHashSet(
                StringComparer.Ordinal
            ),
            ValueType = valueType,
        };
        SetContext(context, groupContext);

        var userClass = output.GetUserSuppliedClass();

        // The context above must be published before children are processed, since each
        // <sa-toggle-group-item> reads it from TagHelperContext.Items during this call.
        var childContent = await output.GetChildContentAsync();

        // Keep author attributes (id, aria-label, custom data-*) on the group, but drop the
        // form name the base copies onto the host — the name belongs on the child inputs.
        output.Attributes.RemoveAll("name");
        output.TagName = "div";
        output.TagMode = TagMode.StartTagAndEndTag;
        output.Attributes.SetAttribute("role", "group");
        ApplyFieldAttributes(context, output.Attributes, FieldLayout.Stacked);
        output.Attributes.SetAttribute("data-slot", "toggle-group");
        output.Attributes.SetAttribute("data-variant", effectiveVariant.GetDataAttributeText());
        output.Attributes.SetAttribute("data-size", effectiveSize.GetDataAttributeText());
        output.Attributes.SetAttribute(
            "data-spacing",
            effectiveSpacing.ToString(CultureInfo.InvariantCulture)
        );

        // toggle-group.css keys the items' orientation styles off this attribute.
        output.Attributes.SetAttribute(
            "data-orientation",
            effectiveOrientation.GetDataAttributeText()
        );

        // Layout: sa-toggle-group only carries rounding/shadow, so the flex layout + spacing
        // live here. When joined (no spacing) collapse the 1px seam between outline items;
        // otherwise drive gap-[--spacing(var(--gap))] from a --gap CSS var (so the default
        // 2 units -> 0.5rem, etc.).

        if (effectiveSpacing != 0)
        {
            var gapVar = $"--gap: {effectiveSpacing.ToString(CultureInfo.InvariantCulture)}";
            var existingStyle = output.Attributes["style"]?.Value?.ToString();
            output.Attributes.SetAttribute(
                "style",
                string.IsNullOrEmpty(existingStyle) ? gapVar : $"{gapVar}; {existingStyle}"
            );
        }

        output.Attributes.SetAttribute(
            "class",
            JoinCssClasses("sa-toggle-group", ClassNames?.Control, userClass)
        );

        output.Content.AppendHtml(childContent);

        // Unchecked checkboxes post nothing, so the checkbox group's marker lets an empty
        // selection clear the bound collection
        if (multiple && !string.IsNullOrEmpty(fieldName) && groupContext.HasEnabledItem)
        {
            var marker = new TagBuilder("input") { TagRenderMode = TagRenderMode.SelfClosing };
            marker.Attributes["type"] = "hidden";
            marker.Attributes["name"] = CheckboxGroupModelBinderProvider.MarkerPrefix + fieldName;
            marker.Attributes["value"] = "true";
            output.Content.AppendHtml(marker);
        }

        return FieldLayout.Stacked;
    }
}
