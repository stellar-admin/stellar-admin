using System.ComponentModel.DataAnnotations;
using System.Globalization;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace StellarAdmin.Dashboard.Resources.Editors;

/// <summary>
///     Configures a slider editor for a whole-number property. Bounds left unset come from the property's
///     <see cref="RangeAttribute" />.
/// </summary>
public sealed class SliderEditor : FieldEditor, IFieldEditor<SliderEditorHandler>
{
    /// <summary>
    ///     The largest value, or null to use the property's range or 100.
    /// </summary>
    public int? Max { get; set; }

    /// <summary>
    ///     The smallest value, or null to use the property's range or 0.
    /// </summary>
    public int? Min { get; set; }

    /// <summary>
    ///     The interval between values, or null for 1.
    /// </summary>
    public int? Step { get; set; }

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
