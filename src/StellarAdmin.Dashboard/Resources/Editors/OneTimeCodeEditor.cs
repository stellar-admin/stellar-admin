using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace StellarAdmin.Dashboard.Resources.Editors;

/// <summary>
///     Configures a one-time code editor that shows one box per digit.
/// </summary>
public sealed class OneTimeCodeEditor : FieldEditor, IFieldEditor<OneTimeCodeEditorHandler>
{
    /// <summary>
    ///     The number of digits, or null to use the property's maximum length or 6.
    /// </summary>
    public int? Length { get; set; }

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
