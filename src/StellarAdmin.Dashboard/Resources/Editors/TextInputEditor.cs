using System.Globalization;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace StellarAdmin.Dashboard.Resources.Editors;

/// <summary>
///     Configures a single-line text input editor. Settings left unset are inferred from the property.
/// </summary>
public sealed class TextInputEditor : FieldEditor, IFieldEditor<TextInputEditorHandler>
{
    /// <summary>
    ///     The largest number a number input accepts.
    /// </summary>
    public decimal? Max { get; set; }

    /// <summary>
    ///     The smallest number a number input accepts.
    /// </summary>
    public decimal? Min { get; set; }

    /// <summary>
    ///     Hint text displayed while the input is empty.
    /// </summary>
    public string? Placeholder { get; set; }

    /// <summary>
    ///     Text displayed before the input, such as a currency symbol.
    /// </summary>
    public string? Prefix { get; set; }

    /// <summary>
    ///     The granularity of a number input, or null to infer it from the property type.
    /// </summary>
    public decimal? Step { get; set; }

    /// <summary>
    ///     Text displayed after the input, such as a unit.
    /// </summary>
    public string? Suffix { get; set; }

    /// <summary>
    ///     The kind of value the input accepts, or null to infer it from the property.
    /// </summary>
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
