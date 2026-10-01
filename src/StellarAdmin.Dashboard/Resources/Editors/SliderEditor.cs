using System.ComponentModel.DataAnnotations;
using System.Globalization;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using StellarAdmin.TagHelpers;

namespace StellarAdmin.Dashboard.Resources.Editors;

/// <summary>
///     Configures a slider editor for a whole-number property. Bounds left unset come from the property's
///     <see cref="RangeAttribute" />.
/// </summary>
public sealed class SliderEditor : FieldEditor, IFieldEditor<SliderEditorHandler>
{
    private readonly List<SliderEditorMark> _marks = [];

    /// <summary>
    ///     Additional CSS classes for the slider editor, its value and its marks.
    /// </summary>
    public override SliderEditorClassNames ClassNames { get; } = new();

    /// <summary>
    ///     The interval between ticks, or null for no ticks.
    /// </summary>
    public int? MarkInterval { get; set; }

    /// <summary>
    ///     Which marks are labelled with their value. Defaults to <see cref="SliderMarkLabels.Ends" />,
    ///     which labels the smallest and largest values.
    /// </summary>
    public SliderMarkLabels MarkLabels { get; set; } = SliderMarkLabels.Ends;

    /// <summary>
    ///     The largest value, or null to use the property's range or 100.
    /// </summary>
    public int? Max { get; set; }

    /// <summary>
    ///     The smallest value, or null to use the property's range or 0.
    /// </summary>
    public int? Min { get; set; }

    /// <summary>
    ///     Whether the current value is shown beside the label. Defaults to true.
    /// </summary>
    public bool ShowValue { get; set; } = true;

    /// <summary>
    ///     The interval between values, or null for 1.
    /// </summary>
    public int? Step { get; set; }

    /// <summary>
    ///     How values are displayed, with <c>{0}</c> standing for the number, such as <c>{0} km</c>, or
    ///     null to show the number alone. Applies to the shown value, the mark labels and the value
    ///     announced by screen readers.
    /// </summary>
    public string? ValueFormat { get; set; }

    internal IReadOnlyList<SliderEditorMark> Marks => _marks;

    /// <summary>
    ///     Adds a mark at a value, optionally labelled. Added marks replace the marks generated from
    ///     <see cref="MarkInterval" /> and <see cref="MarkLabels" />.
    /// </summary>
    /// <param name="value">The value to mark, between the slider's bounds.</param>
    /// <param name="label">The text shown under the mark, or null for a tick alone.</param>
    public void AddMark(int value, string? label = null) => _marks.Add(new(value, label));

    internal SliderAttributes ResolveAttributes(ModelMetadata metadata)
    {
        var range = metadata.ValidatorMetadata.OfType<RangeAttribute>().FirstOrDefault();

        return new SliderAttributes(
            Min ?? ToInt32(range?.Minimum),
            Max ?? ToInt32(range?.Maximum),
            Step
        );
    }

    private static int? ToInt32(object? bound) =>
        bound switch
        {
            int value => value,
            double value
                when value == Math.Floor(value) && value is >= int.MinValue and <= int.MaxValue =>
                (int)value,
            string text when int.TryParse(text, CultureInfo.InvariantCulture, out var value) =>
                value,
            _ => null,
        };
}

internal sealed record SliderAttributes(int? Min, int? Max, int? Step);

internal sealed record SliderEditorMark(int Value, string? Label);
