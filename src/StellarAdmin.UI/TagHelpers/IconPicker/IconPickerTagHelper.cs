using System.Globalization;
using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Razor.TagHelpers;
using Microsoft.Extensions.Options;
using StellarAdmin.UI.Icons;

namespace StellarAdmin.UI;

/// <summary>
///     Renders a control for choosing an icon, which posts the icon's name.
/// </summary>
/// <remarks>
///     <para>
///         The icons come from child <c>&lt;sa-icon&gt;</c> elements or, for the picker layout,
///         from <see cref="IconsUrl" />.
///     </para>
///     <para>
///         The picker layout requires <c>stellar-admin.js</c>. The inline layout works without
///         it, but clearing the choice of an optional field needs it.
///     </para>
/// </remarks>
[HtmlTargetElement("sa-icon-picker")]
public sealed class IconPickerTagHelper : FieldInputBaseTagHelper<IconPickerClassNames>
{
    private const string DefaultPlaceholder = "Choose an icon";

    private readonly IHtmlGenerator _htmlGenerator;
    private readonly IconOptions _iconOptions;

    /// <summary>
    ///     Initializes a new instance of the <see cref="IconPickerTagHelper" /> class.
    /// </summary>
    /// <param name="htmlGenerator">The generator that renders a bound label and error.</param>
    /// <param name="iconOptions">The icon options that supply the icons.</param>
    /// <exception cref="ArgumentNullException">
    ///     <paramref name="htmlGenerator" /> is <see langword="null" />.
    /// </exception>
    public IconPickerTagHelper(IHtmlGenerator htmlGenerator, IOptions<IconOptions> iconOptions)
        : base(htmlGenerator)
    {
        _htmlGenerator = htmlGenerator;
        _iconOptions = iconOptions.Value;
    }

    /// <summary>
    ///     Gets or sets a value that indicates whether the control is disabled.
    /// </summary>
    /// <value>
    ///     <see langword="true" /> if the control is disabled; otherwise, <see langword="false" />.
    ///     The default is <see langword="false" />.
    /// </value>
    /// <remarks>
    ///     A disabled control doesn't post its value.
    /// </remarks>
    [HtmlAttributeName("disabled")]
    public bool? Disabled { get; set; }

    /// <summary>
    ///     Gets or sets the <c>id</c> of the form that the value belongs to.
    /// </summary>
    /// <remarks>
    ///     Use it when the control is outside that form.
    /// </remarks>
    [HtmlAttributeName("form")]
    public string? FormName { get; set; }

    /// <summary>
    ///     Gets or sets the URL that the picker loads its icons from.
    /// </summary>
    /// <remarks>
    ///     <para>
    ///         The picker requests the URL once, when it first opens, and searches the icons in
    ///         the browser. The endpoint answers with JSON:
    ///         <c>{ "icons": [{ "name": "...", "svg": "&lt;svg ...&gt;...&lt;/svg&gt;", "keywords": ["..."] }] }</c>.
    ///         <c>name</c> and <c>svg</c> are required, and <c>keywords</c> is optional. Search
    ///         matches the names and keywords.
    ///     </para>
    ///     <para>
    ///         Can't be combined with child <c>&lt;sa-icon&gt;</c> elements or the inline layout.
    ///     </para>
    /// </remarks>
    [HtmlAttributeName("icons-url")]
    public string? IconsUrl { get; set; }

    /// <summary>
    ///     Gets or sets how the control shows its icons.
    /// </summary>
    /// <value>
    ///     One of the enumeration values that specifies the layout. The default is
    ///     <see cref="IconPickerLayout.Picker" />.
    /// </value>
    [HtmlAttributeName("layout")]
    public IconPickerLayout? Layout { get; set; }

    /// <summary>
    ///     Gets or sets the text the button shows when no icon is chosen.
    /// </summary>
    /// <value>
    ///     The default is "Choose an icon".
    /// </value>
    [HtmlAttributeName("placeholder")]
    public string? Placeholder { get; set; }

    /// <summary>
    ///     Gets or sets a value that indicates whether an icon must be chosen.
    /// </summary>
    /// <value>
    ///     <see langword="true" /> if an icon must be chosen; otherwise,
    ///     <see langword="false" />. The default is whether the bound property is required.
    /// </value>
    /// <remarks>
    ///     An optional control lets the user clear the choice.
    /// </remarks>
    [HtmlAttributeName("required")]
    public bool? Required { get; set; }

    /// <summary>
    ///     Gets or sets the name of the chosen icon.
    /// </summary>
    /// <remarks>
    ///     Used when the control isn't bound with <c>asp-for</c>.
    /// </remarks>
    [HtmlAttributeName("value")]
    public string? Value { get; set; }

    /// <inheritdoc />
    /// <exception cref="InvalidOperationException">
    ///     The control has both child <c>&lt;sa-icon&gt;</c> elements and an
    ///     <see cref="IconsUrl" />, or neither, uses the inline layout with an
    ///     <see cref="IconsUrl" />, or has a child <c>&lt;sa-icon&gt;</c> without a name.
    /// </exception>
    protected override async Task<FieldLayout> RenderInput(
        TagHelperContext context,
        TagHelperOutput output,
        IDictionary<string, object?>? htmlAttributes
    )
    {
        var effectiveLayout = Layout ?? IconPickerLayout.Picker;
        var effectiveDisabled = Disabled ?? false;
        var effectiveRequired = Required ?? For?.Metadata.IsRequired ?? false;

        var pickerContext = new IconPickerContext { Owner = this };
        SetContext(context, pickerContext);
        await output.GetChildContentAsync();

        var hasIconsUrl = !string.IsNullOrEmpty(IconsUrl);
        if (pickerContext.Icons.Count > 0 && hasIconsUrl)
        {
            throw new InvalidOperationException(
                "An icon picker takes its icons from child sa-icon elements or from icons-url, not both."
            );
        }

        if (pickerContext.Icons.Count == 0 && !hasIconsUrl)
        {
            throw new InvalidOperationException(
                "An icon picker needs child sa-icon elements or an icons-url."
            );
        }

        if (effectiveLayout == IconPickerLayout.Inline && hasIconsUrl)
        {
            throw new InvalidOperationException(
                "An inline icon picker can't load its icons from icons-url. List them as child sa-icon elements."
            );
        }

        if (pickerContext.Icons.Any(icon => string.IsNullOrEmpty(icon.Name)))
        {
            throw new InvalidOperationException("Each sa-icon in an icon picker needs a name.");
        }

        var icons = pickerContext
            .Icons.DistinctBy(icon => icon.Name, StringComparer.Ordinal)
            .Select(icon => (Name: icon.Name!, icon.Svg))
            .ToList();
        foreach (var (name, svg) in icons)
        {
            svg.Attributes.SetAttribute("aria-hidden", "true");
            svg.Attributes.SetAttribute("data-icon-name", name);
        }

        var value = For?.Model?.ToString() ?? Value;
        var picker = new Picker(
            icons,
            string.IsNullOrEmpty(value) ? null : value,
            ResolveName(),
            effectiveDisabled,
            effectiveRequired
        );

        output.Attributes.RemoveAll("name");
        output.TagName = "sel-icon-picker";
        output.TagMode = TagMode.StartTagAndEndTag;

        output.Attributes.SetAttribute("data-slot", "icon-picker");
        output.Attributes.SetAttribute("data-layout", effectiveLayout.GetDataAttributeText());
        if (effectiveLayout == IconPickerLayout.Picker)
        {
            output.Attributes.SetAttribute("data-placeholder", Placeholder ?? DefaultPlaceholder);
        }

        if (hasIconsUrl)
        {
            output.Attributes.SetAttribute("data-icons-url", IconsUrl);
        }

        if (effectiveRequired)
        {
            output.Attributes.SetAttribute("data-required", "true");
        }

        if (effectiveDisabled)
        {
            output.Attributes.SetAttribute("data-disabled", "true");
        }

        output.Attributes.SetAttribute(
            "class",
            JoinCssClasses("sa-icon-picker", ClassNames?.Control, output.GetUserSuppliedClass())
        );

        if (effectiveLayout == IconPickerLayout.Inline)
        {
            RenderInline(context, output, picker);
        }
        else
        {
            RenderPicker(context, output, picker);
        }

        return FieldLayout.Stacked;
    }

    private TagHelperOutput RenderIcon(TagHelperContext context, string? name, string? className)
    {
        var icon = new TagHelperOutput(
            "svg",
            [new TagHelperAttribute("aria-hidden", "true")],
            (_, _) => Task.FromResult<TagHelperContent>(new DefaultTagHelperContent())
        );
        if (className != null)
        {
            icon.Attributes.SetAttribute("class", className);
        }

        new IconTagHelper(_iconOptions) { Name = name }.Process(context, icon);

        return icon;
    }

    private void RenderInline(TagHelperContext context, TagHelperOutput output, Picker picker)
    {
        var spacing = ToggleGroupSpacing.Default.GetSpacingUnits();

        var group = new TagHelperOutput(
            "div",
            [],
            (_, _) => Task.FromResult<TagHelperContent>(new DefaultTagHelperContent())
        )
        {
            TagMode = TagMode.StartTagAndEndTag,
        };
        group.Attributes.SetAttribute("role", "group");
        foreach (var name in new[] { "aria-label", "aria-describedby" })
        {
            if (output.Attributes[name] is { } attribute)
            {
                group.Attributes.SetAttribute(name, attribute.Value);
                output.Attributes.RemoveAll(name);
            }
        }

        ApplyFieldAttributes(context, group.Attributes, FieldLayout.Stacked);
        if (WillRenderFieldLabel())
        {
            LabelId = $"sa-{GetUniqueId(context)}-label";
            if (!group.Attributes.ContainsName("aria-label"))
            {
                group.Attributes.SetAttribute("aria-labelledby", LabelId);
            }
        }

        group.Attributes.SetAttribute("data-slot", "toggle-group");
        group.Attributes.SetAttribute("data-variant", ToggleVariant.Outline.GetDataAttributeText());
        group.Attributes.SetAttribute("data-size", ToggleSize.Default.GetDataAttributeText());
        group.Attributes.SetAttribute(
            "data-spacing",
            spacing.ToString(CultureInfo.InvariantCulture)
        );
        group.Attributes.SetAttribute("data-orientation", "horizontal");
        group.Attributes.SetAttribute(
            "style",
            $"--gap: {spacing.ToString(CultureInfo.InvariantCulture)}"
        );
        group.Attributes.SetAttribute("class", "sa-toggle-group");

        foreach (var (name, svg) in picker.Icons)
        {
            var input = new TagBuilder("input") { TagRenderMode = TagRenderMode.SelfClosing };
            input.Attributes.Add("type", "radio");
            input.Attributes.Add("data-slot", "toggle-group-item-input");
            input.Attributes.Add("class", "sr-only");
            if (picker.FieldName != null)
            {
                input.Attributes.Add("name", picker.FieldName);
            }

            input.Attributes.Add("value", name);
            input.Attributes.Add("aria-label", name);
            if (string.Equals(name, picker.Value, StringComparison.Ordinal))
            {
                input.Attributes.Add("checked", "checked");
            }

            if (picker.Disabled)
            {
                input.Attributes.Add("disabled", "disabled");
            }

            if (FormName != null)
            {
                input.Attributes.Add("form", FormName);
            }

            var item = new TagBuilder("label");
            item.Attributes.Add("data-slot", "toggle-group-item");
            item.Attributes.Add("data-variant", ToggleVariant.Outline.GetDataAttributeText());
            item.Attributes.Add("data-size", ToggleSize.Default.GetDataAttributeText());
            item.Attributes.Add("data-spacing", spacing.ToString(CultureInfo.InvariantCulture));
            item.Attributes.Add("title", name);
            item.Attributes.Add(
                "class",
                ToggleRenderingHelper.BuildClass(
                    ToggleVariant.Outline,
                    ToggleSize.Default,
                    includeGroupItemToken: true,
                    ClassNames?.Item
                )
            );
            item.InnerHtml.AppendHtml(input);
            item.InnerHtml.AppendHtml(svg);
            group.Content.AppendHtml(item);
        }

        output.Content.AppendHtml(group);

        // Posts a stored value that isn't one of the tiles, so it saves unchanged, and posts an
        // empty value once the script clears an optional field. A checked tile comes first, so
        // its value wins.
        var isTile = picker.Icons.Any(icon =>
            string.Equals(icon.Name, picker.Value, StringComparison.Ordinal)
        );
        var fallback = new TagBuilder("input") { TagRenderMode = TagRenderMode.SelfClosing };
        fallback.Attributes.Add("type", "hidden");
        fallback.Attributes.Add("data-slot", "icon-picker-value");
        if (picker.FieldName != null)
        {
            fallback.Attributes.Add("name", picker.FieldName);
        }

        fallback.Attributes.Add("value", isTile ? string.Empty : picker.Value ?? string.Empty);
        if (isTile || picker.Value == null || picker.Disabled)
        {
            fallback.Attributes.Add("disabled", "disabled");
        }

        if (FormName != null)
        {
            fallback.Attributes.Add("form", FormName);
        }

        output.Content.AppendHtml(fallback);
    }

    private void RenderPicker(TagHelperContext context, TagHelperOutput output, Picker picker)
    {
        var uniqueId = GetUniqueId(context);
        var popoverId = $"sa-{uniqueId}-popover";
        var gridId = $"sa-{uniqueId}-grid";

        // The field's label targets the trigger: the framework's id for a bound control, or the
        // id the author set or the base minted, moved off the host
        var triggerId =
            output.Attributes["id"]?.Value?.ToString()
            ?? (
                For != null
                    ? TagBuilder.CreateSanitizedId(
                        ViewContext.ViewData.TemplateInfo.GetFullHtmlFieldName(For.Name),
                        _htmlGenerator.IdAttributeDotReplacement
                    )
                    : $"sa-{uniqueId}-trigger"
            );
        output.Attributes.RemoveAll("id");
        LabelForId = triggerId;

        var hidden = new TagBuilder("input") { TagRenderMode = TagRenderMode.SelfClosing };
        hidden.Attributes.Add("type", "hidden");
        hidden.Attributes.Add("data-slot", "icon-picker-value");
        if (picker.FieldName != null)
        {
            hidden.Attributes.Add("name", picker.FieldName);
        }

        hidden.Attributes.Add("value", picker.Value ?? string.Empty);
        if (picker.Disabled)
        {
            hidden.Attributes.Add("disabled", "disabled");
        }

        if (FormName != null)
        {
            hidden.Attributes.Add("form", FormName);
        }

        output.Content.AppendHtml(hidden);

        var trigger = new TagHelperOutput(
            "button",
            [],
            (_, _) => Task.FromResult<TagHelperContent>(new DefaultTagHelperContent())
        )
        {
            TagMode = TagMode.StartTagAndEndTag,
        };
        trigger.Attributes.SetAttribute("type", "button");
        trigger.Attributes.SetAttribute("id", triggerId);
        trigger.Attributes.SetAttribute("data-slot", "icon-picker-trigger");
        trigger.Attributes.SetAttribute("popovertarget", popoverId);
        trigger.Attributes.SetAttribute("aria-haspopup", "dialog");
        foreach (var name in new[] { "aria-label", "aria-describedby" })
        {
            if (output.Attributes[name] is { } attribute)
            {
                trigger.Attributes.SetAttribute(name, attribute.Value);
                output.Attributes.RemoveAll(name);
            }
        }

        if (picker.Disabled)
        {
            trigger.Attributes.SetAttribute("disabled", "disabled");
        }

        ApplyFieldAttributes(context, trigger.Attributes, FieldLayout.Stacked);
        trigger.Attributes.SetAttribute(
            "class",
            JoinCssClasses("sa-input", "sa-icon-picker-trigger", ClassNames?.Trigger)
        );

        var text = new TagBuilder("span");
        text.Attributes.Add("data-slot", "icon-picker-text");
        if (picker.Value == null)
        {
            text.Attributes.Add("data-placeholder", "true");
            text.InnerHtml.Append(Placeholder ?? DefaultPlaceholder);
        }
        else
        {
            trigger.Content.AppendHtml(RenderValueIcon(context, picker.Value));
            text.InnerHtml.Append(picker.Value);
        }

        trigger.Content.AppendHtml(text);
        trigger.Content.AppendHtml(
            RenderIcon(
                context,
                _iconOptions.GetSemanticIconName(SemanticIconRole.DropdownIndicator),
                "sa-icon-picker-chevron"
            )
        );
        output.Content.AppendHtml(trigger);

        output.Content.AppendHtml(RenderPopover(context, picker, popoverId, gridId));

        if (picker.Icons.Count > 0)
        {
            var template = new TagBuilder("template");
            template.Attributes.Add("data-slot", "icon-picker-icons");
            foreach (var (_, svg) in picker.Icons)
            {
                template.InnerHtml.AppendHtml(svg);
            }

            output.Content.AppendHtml(template);
        }
    }

    private TagBuilder RenderPopover(
        TagHelperContext context,
        Picker picker,
        string popoverId,
        string gridId
    )
    {
        var input = new TagBuilder("input") { TagRenderMode = TagRenderMode.SelfClosing };
        input.Attributes.Add("type", "search");
        input.Attributes.Add("data-slot", "input-group-control");
        input.Attributes.Add("class", "sa-input sa-input-group-input");
        input.Attributes.Add("placeholder", "Search icons");
        input.Attributes.Add("aria-label", "Search icons");
        input.Attributes.Add("aria-controls", gridId);
        input.Attributes.Add("autocomplete", "off");
        input.Attributes.Add("spellcheck", "false");

        var addon = new TagBuilder("div");
        addon.Attributes.Add("role", "group");
        addon.Attributes.Add("data-slot", "input-group-addon");
        addon.Attributes.Add("data-align", "start");
        addon.Attributes.Add("class", "sa-input-group-addon sa-input-group-addon-align-start");
        addon.InnerHtml.AppendHtml(
            RenderIcon(context, _iconOptions.GetSemanticIconName(SemanticIconRole.Search), null)
        );

        var inputGroup = new TagBuilder("div");
        inputGroup.Attributes.Add("role", "group");
        inputGroup.Attributes.Add("data-slot", "input-group");
        inputGroup.Attributes.Add("class", "sa-input-group");
        inputGroup.InnerHtml.AppendHtml(input);
        inputGroup.InnerHtml.AppendHtml(addon);

        var search = new TagBuilder("div");
        search.Attributes.Add("data-slot", "icon-picker-search");
        search.Attributes.Add("class", "sa-icon-picker-search");
        search.InnerHtml.AppendHtml(inputGroup);

        var grid = new TagBuilder("div");
        grid.Attributes.Add("id", gridId);
        grid.Attributes.Add("role", "listbox");
        grid.Attributes.Add("aria-label", "Icons");
        grid.Attributes.Add("data-slot", "icon-picker-grid");
        grid.Attributes.Add("class", "sa-icon-picker-grid");

        var scroll = new TagBuilder("div");
        scroll.Attributes.Add("data-slot", "icon-picker-scroll");
        scroll.Attributes.Add("class", "sa-icon-picker-scroll");
        scroll.InnerHtml.AppendHtml(grid);

        var name = new TagBuilder("span");
        name.Attributes.Add("data-slot", "icon-picker-name");
        var count = new TagBuilder("span");
        count.Attributes.Add("data-slot", "icon-picker-count");
        count.Attributes.Add("aria-live", "polite");

        var status = new TagBuilder("div");
        status.Attributes.Add("data-slot", "icon-picker-status");
        status.Attributes.Add("class", "sa-icon-picker-status");
        status.InnerHtml.AppendHtml(name);
        status.InnerHtml.AppendHtml(count);
        if (!picker.Required)
        {
            var clear = new TagHelperOutput(
                "button",
                [new TagHelperAttribute("type", "button")],
                (_, _) => Task.FromResult<TagHelperContent>(new DefaultTagHelperContent())
            );
            ButtonRenderingHelper.RenderAttributes(
                clear,
                ButtonVariant.Ghost,
                ButtonSize.ExtraSmall
            );
            clear.Attributes.SetAttribute("data-action", "clear");
            clear.Content.Append("Clear");
            status.InnerHtml.AppendHtml(clear);
        }

        var popover = new TagBuilder("div");
        popover.Attributes.Add("id", popoverId);
        popover.Attributes.Add("popover", string.Empty);
        popover.Attributes.Add("data-slot", "icon-picker-popover");
        popover.Attributes.Add("data-anchor-side", "bottom");
        popover.Attributes.Add("role", "dialog");
        popover.Attributes.Add("aria-label", "Choose an icon");
        popover.Attributes.Add(
            "class",
            JoinCssClasses(
                "sa-popover-content",
                AnchorPlacement.GetClassName("bottom", "start"),
                "sa-icon-picker-popover",
                ClassNames?.Popover
            )
        );
        popover.InnerHtml.AppendHtml(search);
        popover.InnerHtml.AppendHtml(scroll);
        popover.InnerHtml.AppendHtml(status);

        return popover;
    }

    // A child icon's output can only be written once, so the trigger renders its own copy
    private IHtmlContent RenderValueIcon(TagHelperContext context, string value)
    {
        if (_iconOptions.TryGetIcon(value, out _))
        {
            return RenderIcon(context, value, null);
        }

        var missing = new TagBuilder("span");
        missing.Attributes.Add("class", "sa-icon-missing");
        missing.Attributes.Add("aria-hidden", "true");

        return missing;
    }

    private string? ResolveName()
    {
        if (!string.IsNullOrEmpty(Name))
        {
            return Name;
        }

        // An editor template binds asp-for="@Model", whose name is empty and lives in the prefix
        if (For != null)
        {
            var fullName = ViewContext.ViewData.TemplateInfo.GetFullHtmlFieldName(For.Name);

            return string.IsNullOrEmpty(fullName) ? null : fullName;
        }

        return null;
    }

    private sealed record Picker(
        List<(string Name, TagHelperOutput Svg)> Icons,
        string? Value,
        string? FieldName,
        bool Disabled,
        bool Required
    );
}
