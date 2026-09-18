using System.Collections;
using System.ComponentModel;
using System.Globalization;

namespace StellarAdmin.TagHelpers;

internal static class ChoiceGroupValue
{
    public static Type? ElementType(Type type)
    {
        if (type.IsArray)
        {
            return type.GetArrayRank() == 1 && type != typeof(byte[])
                ? type.GetElementType()
                : null;
        }

        if (
            type.IsGenericType
            && new[]
            {
                typeof(List<>),
                typeof(IList<>),
                typeof(ICollection<>),
                typeof(IEnumerable<>),
                typeof(IReadOnlyList<>),
                typeof(IReadOnlyCollection<>),
            }.Contains(type.GetGenericTypeDefinition())
        )
        {
            return type.GetGenericArguments()[0];
        }

        return null;
    }

    public static string Format(object value) =>
        Convert.ToString(value, CultureInfo.CurrentCulture) ?? "";

    public static bool IsSupported(Type type)
    {
        type = Nullable.GetUnderlyingType(type) ?? type;
        return type.IsEnum
            || type == typeof(string)
            || type == typeof(Guid)
            || type == typeof(bool)
            || type == typeof(byte)
            || type == typeof(sbyte)
            || type == typeof(short)
            || type == typeof(ushort)
            || type == typeof(int)
            || type == typeof(uint)
            || type == typeof(long)
            || type == typeof(ulong)
            || type == typeof(float)
            || type == typeof(double)
            || type == typeof(decimal);
    }

    public static string Normalize(object value, Type? type)
    {
        var text = Format(value);
        type = type == null ? null : Nullable.GetUnderlyingType(type) ?? type;
        if (type == null || type == typeof(string))
        {
            return text;
        }

        try
        {
            var converted = type.IsEnum
                ? Enum.Parse(type, text, true)
                : TypeDescriptor
                    .GetConverter(type)
                    .ConvertFromString(null, CultureInfo.CurrentCulture, text);
            return converted == null ? text : Format(converted);
        }
        catch (Exception exception)
            when (exception
                    is ArgumentException
                        or FormatException
                        or OverflowException
                        or NotSupportedException
            )
        {
            // Preserve invalid submitted values for redisplay without selecting a valid alternative.
            return text;
        }
    }

    public static IEnumerable<object?> Values(object? value)
    {
        return value is IEnumerable values and not string ? values.Cast<object?>()
            : value == null ? []
            : [value];
    }
}
