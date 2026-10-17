using System.Globalization;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace StellarAdmin.Resources.Editors;

/// <summary>
///     Configures a single-line text input editor.
/// </summary>
/// <remarks>
///     Settings left unset are inferred from the property.
/// </remarks>
public sealed class TextInputEditor : FieldEditor, IFieldEditor<TextInputEditorHandler>
{
    /// <summary>
    ///     Gets or sets the largest number a number input accepts.
    /// </summary>
    /// <value>
    ///     The largest number, or <see langword="null" /> for no limit. The default is
    ///     <see langword="null" />.
    /// </value>
    public decimal? Max { get; set; }

    /// <summary>
    ///     Gets or sets the smallest number a number input accepts.
    /// </summary>
    /// <value>
    ///     The smallest number, or <see langword="null" /> for no limit. The default is
    ///     <see langword="null" />.
    /// </value>
    public decimal? Min { get; set; }

    /// <summary>
    ///     Gets or sets the hint text displayed while the input is empty.
    /// </summary>
    public string? Placeholder { get; set; }

    /// <summary>
    ///     Gets or sets the text displayed before the input, such as a currency symbol.
    /// </summary>
    public string? Prefix { get; set; }

    /// <summary>
    ///     Gets or sets the interval between the numbers a number input accepts.
    /// </summary>
    /// <value>
    ///     The interval, or <see langword="null" /> to infer it from the property's type. The
    ///     default is <see langword="null" />.
    /// </value>
    /// <remarks>
    ///     An integer property accepts whole numbers. Any other number property accepts any number.
    /// </remarks>
    public decimal? Step { get; set; }

    /// <summary>
    ///     Gets or sets the text displayed after the input, such as a unit.
    /// </summary>
    public string? Suffix { get; set; }

    /// <summary>
    ///     Gets or sets the kind of value the input accepts.
    /// </summary>
    /// <value>
    ///     One of the enumeration values that specifies the kind of value, or
    ///     <see langword="null" /> to infer it from the property. The default is
    ///     <see langword="null" />.
    /// </value>
    /// <remarks>
    ///     A data type attribute on the property sets the kind for email addresses, phone numbers,
    ///     URLs and passwords. A number property gets <see cref="TextInputType.Number" />.
    /// </remarks>
    public TextInputType? Type { get; set; }

    internal TextInputAttributes ResolveAttributes(ModelMetadata metadata)
    {
        var modelType = metadata.UnderlyingOrModelType;
        var isFractional =
            modelType == typeof(decimal)
            || modelType == typeof(double)
            || modelType == typeof(float);
        var isInteger =
            modelType.IsPrimitive
            && modelType != typeof(bool)
            && modelType != typeof(char)
            && !isFractional
            && modelType != typeof(nint)
            && modelType != typeof(nuint);

        var type =
            Type
            ?? metadata.DataTypeName switch
            {
                "EmailAddress" => TextInputType.Email,
                "PhoneNumber" => TextInputType.Tel,
                "Url" => TextInputType.Url,
                "Password" => TextInputType.Password,
                _ when isFractional || isInteger => TextInputType.Number,
                _ => TextInputType.Text,
            };

        string? step = null;
        if (type == TextInputType.Number)
        {
            step =
                Step is { } explicitStep ? Format(explicitStep)
                : isInteger ? "1"
                : "any";
        }

        return new TextInputAttributes(
            type.ToString().ToLowerInvariant(),
            step,
            Min is { } min ? Format(min) : null,
            Max is { } max ? Format(max) : null
        );
    }

    private static string Format(decimal value) => value.ToString(CultureInfo.InvariantCulture);
}

internal sealed record TextInputAttributes(string Type, string? Step, string? Min, string? Max);
