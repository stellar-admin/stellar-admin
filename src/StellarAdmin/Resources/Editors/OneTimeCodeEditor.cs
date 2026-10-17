using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace StellarAdmin.Resources.Editors;

/// <summary>
///     Configures a one-time code editor that shows one box per digit.
/// </summary>
public sealed class OneTimeCodeEditor : FieldEditor
{
    /// <summary>
    ///     Gets or sets the number of digits.
    /// </summary>
    /// <value>
    ///     The number of digits, or <see langword="null" /> to use the property's maximum length.
    ///     The default is <see langword="null" />.
    /// </value>
    /// <remarks>
    ///     A property without a maximum length gets 6 digits.
    /// </remarks>
    public int? Length { get; set; }

    /// <inheritdoc />
    public override string TemplateName => "Editors/OneTimeCode";

    internal int ResolveLength(ModelMetadata metadata)
    {
        if (Length is { } length)
        {
            return length;
        }

        foreach (var validator in metadata.ValidatorMetadata)
        {
            switch (validator)
            {
                case StringLengthAttribute stringLength:
                    return stringLength.MaximumLength;
                case MaxLengthAttribute { Length: > 0 } maxLength:
                    return maxLength.Length;
            }
        }

        return 6;
    }
}
