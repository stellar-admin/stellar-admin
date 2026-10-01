using System.Globalization;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace StellarAdmin.Dashboard.Resources.Editors;

/// <summary>
///     Base class for the date and time input editors.
/// </summary>
/// <typeparam name="TValue">The type of the earliest and latest accepted values.</typeparam>
public abstract class TemporalInputEditor<TValue> : FieldEditor
    where TValue : struct
{
    private protected TemporalInputEditor() { }

    /// <summary>
    ///     The latest value the input accepts.
    /// </summary>
    public TValue? Max { get; set; }

    /// <summary>
    ///     The earliest value the input accepts.
    /// </summary>
    public TValue? Min { get; set; }

    internal abstract TemporalInputAttributes ResolveAttributes(ModelMetadata metadata);

    private protected static string Format(IFormattable value, string format) =>
        value.ToString(format, CultureInfo.InvariantCulture);

    private protected static string FormatSeconds(TimeSpan step) =>
        step.TotalSeconds.ToString(CultureInfo.InvariantCulture);
}

internal sealed record TemporalInputAttributes(
    string Type,
    string Format,
    string? Step,
    string? Min,
    string? Max
);
