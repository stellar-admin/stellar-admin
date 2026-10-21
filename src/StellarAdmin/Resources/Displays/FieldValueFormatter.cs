using System.Globalization;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace StellarAdmin.Resources.Displays;

// Formats a value as the display templates show it: with the display's format string, then the
// property's [DisplayFormat], then enum members by their labels, all in the current culture
internal static class FieldValueFormatter
{
    public static string Format(object? value, ModelMetadata metadata, string? format = null)
    {
        var culture = CultureInfo.CurrentCulture;
        if (format is not null)
        {
            return value is IFormattable formattable
                ? formattable.ToString(format, culture)
                : Convert.ToString(value, culture) ?? "";
        }

        if (metadata.DisplayFormatString is { } displayFormat)
        {
            return string.Format(culture, displayFormat, value);
        }

        if (value is Enum member && metadata.IsEnum)
        {
            var numericValue = member.ToString("D");
            foreach (var (groupAndName, enumValue) in metadata.EnumGroupedDisplayNamesAndValues!)
            {
                if (enumValue == numericValue)
                {
                    return EnumMemberLabel.Get(member.ToString(), groupAndName.Name);
                }
            }
        }

        return Convert.ToString(value, culture) ?? "";
    }
}
