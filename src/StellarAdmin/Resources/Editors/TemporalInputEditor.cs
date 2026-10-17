using System.Globalization;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace StellarAdmin.Resources.Editors;

/// <summary>
///     Provides the shared settings of the date and time input editors.
/// </summary>
/// <typeparam name="TValue">The type of the earliest and latest accepted values.</typeparam>
public abstract class TemporalInputEditor<TValue> : FieldEditor
    where TValue : struct
{
    private protected TemporalInputEditor() { }

    /// <summary>
    ///     Gets or sets the latest value the input accepts.
    /// </summary>
    /// <value>
    ///     The latest value, or <see langword="null" /> for no limit. The default is
    ///     <see langword="null" />.
    /// </value>
    public TValue? Max { get; set; }

    /// <summary>
    ///     Gets or sets the earliest value the input accepts.
    /// </summary>
    /// <value>
    ///     The earliest value, or <see langword="null" /> for no limit. The default is
    ///     <see langword="null" />.
    /// </value>
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
