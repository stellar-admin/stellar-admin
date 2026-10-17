using System.Collections;
using System.Globalization;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Razor.TagHelpers;

namespace StellarAdmin.UI;

/// <summary>
///     Renders an input that selects a number, or a range of numbers, by dragging thumbs along a
///     track.
/// </summary>
/// <remarks>
///     Each thumb posts its value in a hidden input. Requires <c>stellar-admin.js</c>.
/// </remarks>
[HtmlTargetElement("sa-slider")]
public sealed class SliderTagHelper : FieldInputBaseTagHelper<SliderClassNames>
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="SliderTagHelper" /> class.
    /// </summary>
    /// <param name="htmlGenerator">The generator that renders the field's label and messages.</param>
    /// <exception cref="ArgumentNullException">
    ///     <paramref name="htmlGenerator" /> is <see langword="null" />.
    /// </exception>
    public SliderTagHelper(IHtmlGenerator htmlGenerator)
        : base(htmlGenerator) { }

    /// <summary>
    ///     Gets or sets the lowest value that the slider can take.
    /// </summary>
    /// <value>
    ///     The lowest value, which must be less than the maximum. The default is <c>0</c>.
    /// </value>
    [HtmlAttributeName("min")]
    public double? Min { get; set; }

    /// <summary>
    ///     Gets or sets the highest value that the slider can take.
    /// </summary>
    /// <value>
    ///     The highest value, which must be greater than the minimum. The default is <c>100</c>.
    /// </value>
    [HtmlAttributeName("max")]
    public double? Max { get; set; }

    /// <summary>
    ///     Gets or sets the distance between the values that the slider can take.
    /// </summary>
    /// <value>
    ///     The distance, which must be positive. The default is <c>1</c>.
    /// </value>
    [HtmlAttributeName("step")]
    public double? Step { get; set; }

    /// <summary>
    ///     Gets or sets the smallest distance allowed between neighboring thumbs of a range slider.
    /// </summary>
    /// <value>
    ///     The distance in slider values. The default is <c>0</c>.
    /// </value>
    [HtmlAttributeName("min-distance")]
    public double? MinDistance { get; set; }

    /// <summary>
    ///     Gets or sets the direction of the slider's track.
    /// </summary>
    /// <value>
    ///     One of the enumeration values that specifies the direction. The default is
    ///     <see cref="SliderOrientation.Horizontal" />.
    /// </value>
    [HtmlAttributeName("orientation")]
    public SliderOrientation? Orientation { get; set; }

    /// <summary>
    ///     Gets or sets how each thumb is positioned relative to its value.
    /// </summary>
    /// <value>
    ///     One of the enumeration values that specifies the thumb position. The default is
    ///     <see cref="SliderThumbAlignment.Edge" />.
    /// </value>
    [HtmlAttributeName("thumb-alignment")]
    public SliderThumbAlignment? ThumbAlignment { get; set; }

    /// <summary>
    ///     Gets or sets a value that indicates whether the slider is disabled.
    /// </summary>
    /// <value>
    ///     <see langword="true" /> if the slider is disabled and can't be moved; otherwise,
    ///     <see langword="false" />. The default is <see langword="false" />.
    /// </value>
    [HtmlAttributeName("disabled")]
    public bool? Disabled { get; set; }

    /// <summary>
    ///     Gets or sets the value of each thumb.
    /// </summary>
    /// <value>
    ///     A number, or a comma-separated list of numbers such as <c>20,80</c> for a range slider.
    ///     Each number must be between the minimum and maximum. The default is a single thumb at
    ///     the minimum.
    /// </value>
    /// <remarks>
    ///     The slider shows one thumb for each value. Ignored when bound with <c>asp-for</c> to a
    ///     model value that isn't <see langword="null" /> or empty.
    /// </remarks>
    [HtmlAttributeName("value")]
    public string? Value { get; set; }

    /// <summary>
    ///     Gets or sets the template that each displayed value is shown in.
    /// </summary>
    /// <value>
    ///     A template that contains a <c>{0}</c> placeholder for the value, such as <c>${0}</c> or
    ///     <c>{0} km</c>. The default shows the number alone.
    /// </value>
    /// <remarks>
    ///     The template applies to <c>&lt;sa-slider-value&gt;</c>, generated mark labels and the
    ///     value that screen readers announce for each thumb. Numbers are formatted in the current
    ///     culture, with up to as many decimals as the minimum and step have.
    /// </remarks>
    [HtmlAttributeName("value-format")]
    public string? ValueFormat { get; set; }

    /// <summary>
    ///     Gets or sets the accessible names of the thumbs of a range slider.
    /// </summary>
    /// <value>
    ///     A comma-separated list of names in thumb order, such as
    ///     <c>Minimum price,Maximum price</c>. A name can't contain a comma. The default names
    ///     every thumb after the field label.
    /// </value>
    [HtmlAttributeName("thumb-labels")]
    public string? ThumbLabels { get; set; }

    /// <summary>
    ///     Gets or sets the <c>id</c> of the form that the slider's values post with.
    /// </summary>
    /// <remarks>
    ///     Use it when the slider isn't inside the form.
    /// </remarks>
    [HtmlAttributeName("form")]
    public string? FormName { get; set; }

    /// <inheritdoc />
    /// <exception cref="ArgumentOutOfRangeException">
    ///     <see cref="Min" /> isn't less than <see cref="Max" />, <see cref="Step" /> isn't
    ///     positive, or a value isn't between the minimum and maximum.
    /// </exception>
    /// <exception cref="ArgumentException">
    ///     <see cref="ValueFormat" /> doesn't contain a <c>{0}</c> placeholder.
    /// </exception>
    protected override async Task<FieldLayout> RenderInput(
        TagHelperContext context,
        TagHelperOutput output,
        IDictionary<string, object?>? htmlAttributes
    )
    {
        var effectiveMin = Min ?? 0;
        var effectiveMax = Max ?? 100;
        var effectiveStep = Step ?? 1;
        var effectiveMinDistance = MinDistance ?? 0;
        var effectiveOrientation = Orientation ?? SliderOrientation.Horizontal;
        var effectiveThumbAlignment = ThumbAlignment ?? SliderThumbAlignment.Edge;
        var effectiveDisabled = Disabled ?? false;

        if (effectiveMin >= effectiveMax)
        {
            throw new ArgumentOutOfRangeException(nameof(Min), "Min must be less than Max");
        }

        if (effectiveStep <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(Step), "Step must be positive");
        }

        var decimals = Math.Max(
            SliderContext.CountDecimals(effectiveMin),
            SliderContext.CountDecimals(effectiveStep)
        );

        var values = ResolveValues(effectiveMin, effectiveMax);
        foreach (var value in values)
        {
            if (value < effectiveMin || value > effectiveMax)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(Value),
                    "Each slider value must be between Min and Max"
                );
            }
        }

        // Razor binds an empty attribute, including one set from null, as an empty string
        var valueFormat = string.IsNullOrEmpty(ValueFormat) ? null : ValueFormat;
        if (valueFormat != null && !valueFormat.Contains("{0}", StringComparison.Ordinal))
        {
            throw new ArgumentException(
                "ValueFormat must contain a {0} placeholder for the value",
                nameof(ValueFormat)
            );
        }

        var culture = CultureInfo.CurrentCulture;
        var thumbLabels = ThumbLabels?.Split(
            ',',
            StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries
        );

        var orientationText = effectiveOrientation.GetDataAttributeText();
        var inputName = ResolveName();
        var userClass = output.GetUserSuppliedClass();

        // The host becomes <sel-slider>, which is itself the "slider" root. Drop the
        // name attribute the base copied onto the host: the value posts through the hidden
        // inputs we render per thumb, not through the host element.
        output.Attributes.RemoveAll("name");
        output.TagName = "sel-slider";
        output.TagMode = TagMode.StartTagAndEndTag;
        output.Attributes.SetAttribute("data-slot", "slider");
        output.Attributes.SetAttribute("data-orientation", orientationText);
        output.Attributes.SetAttribute("min", effectiveMin.ToString(CultureInfo.InvariantCulture));
        output.Attributes.SetAttribute("max", effectiveMax.ToString(CultureInfo.InvariantCulture));
        output.Attributes.SetAttribute(
            "step",
            effectiveStep.ToString(CultureInfo.InvariantCulture)
        );
        output.Attributes.SetAttribute(
            "data-min-distance",
            effectiveMinDistance.ToString(CultureInfo.InvariantCulture)
        );
        output.Attributes.SetAttribute(
            "data-thumb-alignment",
            effectiveThumbAlignment.GetDataAttributeText()
        );
        // The client formats values the same way the server formats the marks and thumbs
        if (culture.Name.Length > 0)
        {
            output.Attributes.SetAttribute("data-value-locale", culture.Name);
        }
        if (valueFormat != null)
        {
            output.Attributes.SetAttribute("data-value-format", valueFormat);
        }
        if (effectiveDisabled)
        {
            output.Attributes.SetAttribute("data-disabled", "true");
        }
        output.Attributes.SetAttribute(
            "class",
            JoinCssClasses("sa-slider", ClassNames?.Control, userClass)
        );

        // Track + filled range. For a single thumb the range fills from the start; for a range
        // slider it spans between the lowest and highest thumb.
        var lowPercent = values.Count > 1 ? Percent(values.Min(), effectiveMin, effectiveMax) : 0d;
        var highPercent = Percent(values.Max(), effectiveMin, effectiveMax);

        var track = new TagBuilder("span");
        track.Attributes.Add("data-slot", "slider-track");
        track.Attributes.Add("data-orientation", orientationText);
        track.Attributes.Add("class", JoinCssClasses("sa-slider-track", ClassNames?.Track));

        var range = new TagBuilder("span");
        range.Attributes.Add("data-slot", "slider-range");
        range.Attributes.Add("data-orientation", orientationText);
        range.Attributes.Add("class", JoinCssClasses("sa-slider-range", ClassNames?.Range));
        range.Attributes.Add("style", RangeStyle(effectiveOrientation, lowPercent, highPercent));
        track.InnerHtml.AppendHtml(range);
        output.Content.AppendHtml(track);

        // Publish the resolved state before rendering children so marks can position themselves.
        // The marks precede the thumbs, so the thumbs paint over them.
        SetContext(
            context,
            new SliderContext
            {
                ClassNames = ClassNames,
                Min = effectiveMin,
                Max = effectiveMax,
                Step = effectiveStep,
                Decimals = decimals,
                Values = values,
                Orientation = effectiveOrientation,
                ThumbAlignment = effectiveThumbAlignment,
                ValueFormat = valueFormat,
                Culture = culture,
            }
        );
        output.Content.AppendHtml(await output.GetChildContentAsync());

        // The thumbs are what a screen reader reaches, so they carry the field's descriptions and
        // invalid state rather than the host.
        var thumbAttributes = new TagHelperAttributeList();
        foreach (var name in new[] { "aria-describedby", "aria-invalid", "aria-labelledby" })
        {
            if (output.Attributes[name] is { } attribute)
            {
                thumbAttributes.Add(attribute);
                output.Attributes.Remove(attribute);
            }
        }

        ApplyFieldAttributes(context, thumbAttributes, FieldLayout.Stacked);

        // A label cannot target the host or a thumb, so a rendered field label is referenced by id
        // instead. In an explicit field the client links the author's label on hydration.
        if (WillRenderFieldLabel())
        {
            LabelId = $"sa-{GetUniqueId(context)}-label";
            if (!thumbAttributes.ContainsName("aria-labelledby"))
            {
                thumbAttributes.SetAttribute("aria-labelledby", LabelId);
            }
        }

        // One thumb (+ a hidden input so it posts) per value.
        for (var index = 0; index < values.Count; index++)
        {
            var value = values[index];

            var thumb = new TagBuilder("span");
            thumb.Attributes.Add("data-slot", "slider-thumb");
            thumb.Attributes.Add("data-orientation", orientationText);
            thumb.Attributes.Add("role", "slider");
            thumb.Attributes.Add("tabindex", effectiveDisabled ? "-1" : "0");
            thumb.Attributes.Add("aria-orientation", orientationText);
            thumb.Attributes.Add(
                "aria-valuemin",
                effectiveMin.ToString(CultureInfo.InvariantCulture)
            );
            thumb.Attributes.Add(
                "aria-valuemax",
                effectiveMax.ToString(CultureInfo.InvariantCulture)
            );
            thumb.Attributes.Add("aria-valuenow", value.ToString(CultureInfo.InvariantCulture));
            if (valueFormat != null)
            {
                thumb.Attributes.Add(
                    "aria-valuetext",
                    SliderContext.FormatValue(value, valueFormat, culture, decimals)
                );
            }
            if (effectiveDisabled)
            {
                // Mark the thumb for assistive tech, but don't stamp data-disabled here: the
                // host carries it and applies the single opacity-50 dim for the whole subtree.
                // A second data-disabled:opacity-50 on the thumb would double-dim it (the white
                // fill drops to ~25% and the track bleeds through), and data-disabled:pointer-
                // events-none would kill the token's hover:ring-4. Disabled state is kept on the
                // host wrapper only, so per-thumb disabled utilities are left inert here.
                thumb.Attributes.Add("aria-disabled", "true");
            }
            foreach (var attribute in thumbAttributes)
            {
                thumb.Attributes.Add(attribute.Name, attribute.Value?.ToString());
            }
            if (thumbLabels != null && index < thumbLabels.Length)
            {
                // Its own name tells a range thumb apart from the others
                thumb.Attributes.Remove("aria-labelledby");
                thumb.Attributes.Add("aria-label", thumbLabels[index]);
            }

            thumb.Attributes.Add("class", JoinCssClasses("sa-slider-thumb", ClassNames?.Thumb));
            thumb.Attributes.Add(
                "style",
                ThumbStyle(
                    effectiveOrientation,
                    effectiveThumbAlignment,
                    Percent(value, effectiveMin, effectiveMax)
                )
            );
            output.Content.AppendHtml(thumb);

            if (inputName != null)
            {
                var hidden = new TagBuilder("input") { TagRenderMode = TagRenderMode.SelfClosing };
                hidden.Attributes.Add("type", "hidden");
                hidden.Attributes.Add("data-slot", "slider-input");
                hidden.Attributes.Add("name", inputName);
                hidden.Attributes.Add("value", value.ToString(CultureInfo.InvariantCulture));
                if (FormName != null)
                {
                    hidden.Attributes.Add("form", FormName);
                }
                output.Content.AppendHtml(hidden);
            }
        }

        return FieldLayout.Stacked;
    }

    /// <summary>
    ///     Resolves the current thumb values: from the bound model when <c>asp-for</c> is set
    ///     (scalar or collection), otherwise from the comma-separated <see cref="Value" />,
    ///     otherwise a single thumb at <paramref name="min" />. Bound values are clamped to the
    ///     bounds, because a model can hold any number, such as 0 before a range applies.
    /// </summary>
    private IReadOnlyList<double> ResolveValues(double min, double max)
    {
        if (For?.Model is { } model and not string)
        {
            if (model is IEnumerable enumerable)
            {
                var fromModel = new List<double>();
                foreach (var item in enumerable)
                {
                    if (item != null)
                    {
                        fromModel.Add(
                            Math.Clamp(
                                Convert.ToDouble(item, CultureInfo.InvariantCulture),
                                min,
                                max
                            )
                        );
                    }
                }

                if (fromModel.Count > 0)
                {
                    return fromModel;
                }
            }
            else
            {
                return
                [
                    Math.Clamp(Convert.ToDouble(model, CultureInfo.InvariantCulture), min, max),
                ];
            }
        }

        if (!string.IsNullOrWhiteSpace(Value))
        {
            return Value
                .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                .Select(v => double.Parse(v, CultureInfo.InvariantCulture))
                .ToList();
        }

        return [min];
    }

    /// <summary>
    ///     Resolves the <c>name</c> applied to the posted hidden input(s): the explicit
    ///     <see cref="FieldInputBaseTagHelper.Name" /> when supplied, otherwise the fully
    ///     qualified field name from the bound expression (respecting any <c>HtmlFieldPrefix</c>).
    /// </summary>
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

    private static double Percent(double value, double min, double max) =>
        (value - min) / (max - min) * 100d;

    private static string FormatPercent(double percent) =>
        percent.ToString("0.####", CultureInfo.InvariantCulture);

    private static string RangeStyle(SliderOrientation orientation, double low, double high) =>
        orientation == SliderOrientation.Vertical
            ? $"bottom: {FormatPercent(low)}%; top: {FormatPercent(100d - high)}%;"
            : $"left: {FormatPercent(low)}%; right: {FormatPercent(100d - high)}%;";

    private static string ThumbStyle(
        SliderOrientation orientation,
        SliderThumbAlignment alignment,
        double percent
    )
    {
        // Edge alignment shifts the thumb by its value percentage (relative to its own width),
        // so its leading/trailing edge stays flush with the track ends instead of overhanging;
        // center alignment shifts by a constant 50%. The percentage translate means we never
        // need to know the thumb's pixel size.
        var position = FormatPercent(percent);
        var shift = FormatPercent(alignment == SliderThumbAlignment.Edge ? percent : 50d);
        return orientation == SliderOrientation.Vertical
            ? $"bottom: {position}%; transform: translateY({shift}%);"
            : $"left: {position}%; transform: translateX(-{shift}%);";
    }
}
