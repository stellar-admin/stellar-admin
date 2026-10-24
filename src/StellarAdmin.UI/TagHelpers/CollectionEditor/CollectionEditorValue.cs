using System.Collections;
using System.Globalization;

namespace StellarAdmin.UI;

internal static class CollectionEditorValue
{
    private static readonly Type[] Integers =
    [
        typeof(byte),
        typeof(sbyte),
        typeof(short),
        typeof(ushort),
        typeof(int),
        typeof(uint),
        typeof(long),
        typeof(ulong),
    ];

    private static readonly Type[] Fractions = [typeof(float), typeof(double), typeof(decimal)];

    // The dictionaries MVC binds from indexed names: a string or enum key, and a string, number or
    // enum value
    public static (Type Key, Type Value)? DictionaryTypes(Type type)
    {
        if (
            !type.IsGenericType
            || (
                type.GetGenericTypeDefinition() != typeof(Dictionary<,>)
                && type.GetGenericTypeDefinition() != typeof(IDictionary<,>)
            )
        )
        {
            return null;
        }

        var arguments = type.GetGenericArguments();
        return IsKey(arguments[0]) && IsValue(arguments[1]) ? (arguments[0], arguments[1]) : null;
    }

    public static IEnumerable<(object? Key, object? Value)> Entries(object? model)
    {
        if (model is IDictionary dictionary)
        {
            foreach (DictionaryEntry entry in dictionary)
            {
                yield return (entry.Key, entry.Value);
            }

            yield break;
        }

        if (model is not IEnumerable entries)
        {
            yield break;
        }

        foreach (var entry in entries)
        {
            var type = entry?.GetType();
            yield return (
                type?.GetProperty("Key")?.GetValue(entry),
                type?.GetProperty("Value")?.GetValue(entry)
            );
        }
    }

    // Numbers use the invariant culture, as a number input expects
    public static string Format(object? value) =>
        value switch
        {
            null => "",
            IFormattable formattable when IsNumber(value.GetType()) => formattable.ToString(
                null,
                CultureInfo.InvariantCulture
            ),
            _ => Convert.ToString(value, CultureInfo.InvariantCulture) ?? "",
        };

    public static bool IsInteger(Type type) => Integers.Contains(Underlying(type));

    public static bool IsKey(Type type) => type == typeof(string) || type.IsEnum;

    public static bool IsListElement(Type type) =>
        type == typeof(string) || (Nullable.GetUnderlyingType(type) == null && IsNumber(type));

    public static bool IsNumber(Type type) =>
        IsInteger(type) || Fractions.Contains(Underlying(type));

    public static bool IsValue(Type type) =>
        type == typeof(string) || IsNumber(type) || Underlying(type).IsEnum;

    public static Type Underlying(Type type) => Nullable.GetUnderlyingType(type) ?? type;
}
