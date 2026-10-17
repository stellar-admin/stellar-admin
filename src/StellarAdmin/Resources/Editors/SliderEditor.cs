using System.ComponentModel.DataAnnotations;
using System.Globalization;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using StellarAdmin.UI;

namespace StellarAdmin.Resources.Editors;

/// <summary>
///     Configures a slider editor for a numeric property.
/// </summary>
/// <remarks>
///     Bounds left unset come from the property's <see cref="RangeAttribute" />.
/// </remarks>
public sealed class SliderEditor : FieldEditor
{
    private readonly List<SliderEditorMark> _marks = [];

    /// <summary>
    ///     Gets the additional CSS classes for the parts of the editor.
    /// </summary>
    public override SliderEditorClassNames ClassNames { get; } = new();

    /// <summary>
    ///     Gets or sets the interval between ticks.
    /// </summary>
    /// <value>
    ///     The interval, or <see langword="null" /> for no ticks. The default is
    ///     <see langword="null" />.
    /// </value>
    public double? MarkInterval { get; set; }

    /// <summary>
    ///     Gets or sets which marks show their value.
    /// </summary>
    /// <value>
    ///     One of the enumeration values that specifies which marks show their value. The default
    ///     is <see cref="SliderMarkLabels.Ends" />.
    /// </value>
    public SliderMarkLabels MarkLabels { get; set; } = SliderMarkLabels.Ends;

    /// <summary>
    ///     Gets or sets the largest value.
    /// </summary>
    /// <value>
    ///     The largest value, or <see langword="null" /> to use the property's range. The default
    ///     is <see langword="null" />.
    /// </value>
    /// <remarks>
    ///     A property without a range uses 100.
    /// </remarks>
    public double? Max { get; set; }

    /// <summary>
    ///     Gets or sets the smallest value.
    /// </summary>
    /// <value>
    ///     The smallest value, or <see langword="null" /> to use the property's range. The default
    ///     is <see langword="null" />.
    /// </value>
    /// <remarks>
    ///     A property without a range uses 0.
    /// </remarks>
    public double? Min { get; set; }

    /// <summary>
    ///     Gets or sets a value that indicates whether the current value is shown beside the label.
    /// </summary>
    /// <value>
    ///     <see langword="true" /> if the value is shown; otherwise, <see langword="false" />. The
    ///     default is <see langword="true" />.
    /// </value>
    public bool ShowValue { get; set; } = true;

    /// <summary>
    ///     Gets or sets the interval between values.
    /// </summary>
    /// <value>
    ///     The interval, or <see langword="null" /> for 1. The default is <see langword="null" />.
    /// </value>
    public double? Step { get; set; }

    /// <inheritdoc />
    public override string TemplateName => "Editors/Slider";

    /// <summary>
    ///     Gets or sets how values are displayed.
    /// </summary>
    /// <value>
    ///     A format with <c>{0}</c> for the number, such as <c>{0} km</c>, or
    ///     <see langword="null" /> to show the number alone. The default is
    ///     <see langword="null" />.
    /// </value>
    /// <remarks>
    ///     The format applies to the shown value, the mark labels and the value screen readers
    ///     announce.
    /// </remarks>
    public string? ValueFormat { get; set; }

    internal IReadOnlyList<SliderEditorMark> Marks => _marks;

    /// <summary>
    ///     Adds a mark without a label at a value.
    /// </summary>
    /// <param name="value">The value to mark, between the slider's bounds.</param>
    /// <remarks>
    ///     Added marks replace the marks from <see cref="MarkInterval" /> and
    ///     <see cref="MarkLabels" />.
    /// </remarks>
    public void AddMark(double value) => _marks.Add(new(value, null));

    /// <summary>
    ///     Adds a mark with a label at a value.
    /// </summary>
    /// <param name="value">The value to mark, between the slider's bounds.</param>
    /// <param name="label">The text shown under the mark.</param>
    /// <exception cref="ArgumentNullException">
    ///     <paramref name="label" /> is <see langword="null" />.
    /// </exception>
    /// <remarks>
    ///     Added marks replace the marks from <see cref="MarkInterval" /> and
    ///     <see cref="MarkLabels" />.
    /// </remarks>
    public void AddMark(double value, string label)
    {
        ArgumentNullException.ThrowIfNull(label);

        _marks.Add(new(value, label));
    }

    internal SliderAttributes ResolveAttributes(ModelMetadata metadata)
    {
        var range = metadata.ValidatorMetadata.OfType<RangeAttribute>().FirstOrDefault();

        return new SliderAttributes(
            Min ?? ToDouble(range?.Minimum),
            Max ?? ToDouble(range?.Maximum),
            Step
        );
    }

    private static double? ToDouble(object? bound) =>
        bound switch
        {
            int value => value,
            double value when double.IsFinite(value) => value,
            string text when double.TryParse(text, CultureInfo.InvariantCulture, out var value) =>
                value,
            _ => null,
        };
}

internal sealed record SliderAttributes(double? Min, double? Max, double? Step);

internal sealed record SliderEditorMark(double Value, string? Label);
