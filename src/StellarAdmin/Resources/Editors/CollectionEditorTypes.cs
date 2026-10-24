namespace StellarAdmin.Resources.Editors;

// The properties the list and dictionary editors edit, as StellarAdmin.UI's tag helpers accept them: an array or list of
// strings or numbers, and a dictionary with a string or enum key and a string, number or enum value
internal static class CollectionEditorTypes
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

    private static readonly Type[] Lists =
    [
        typeof(List<>),
        typeof(IList<>),
        typeof(ICollection<>),
        typeof(IEnumerable<>),
        typeof(IReadOnlyList<>),
        typeof(IReadOnlyCollection<>),
    ];

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
        var valueType = Underlying(arguments[1]);

        return
            (arguments[0] == typeof(string) || arguments[0].IsEnum)
            && (arguments[1] == typeof(string) || IsNumber(valueType) || valueType.IsEnum)
            ? (arguments[0], arguments[1])
            : null;
    }

    public static bool IsInteger(Type type) => Integers.Contains(Underlying(type));

    public static bool IsNumber(Type type) =>
        IsInteger(type)
        || Underlying(type) == typeof(float)
        || Underlying(type) == typeof(double)
        || Underlying(type) == typeof(decimal);

    // The element type of a list the list editor edits, or null for any other property. A byte array is binary data.
    public static Type? ListElementType(Type type)
    {
        var elementType =
            type.IsArray
                ? (
                    type.GetArrayRank() == 1 && type != typeof(byte[])
                        ? type.GetElementType()
                        : null
                )
            : type.IsGenericType && Lists.Contains(type.GetGenericTypeDefinition())
                ? type.GetGenericArguments()[0]
            : null;

        return
            elementType == typeof(string)
            || elementType is not null
                && Nullable.GetUnderlyingType(elementType) is null
                && IsNumber(elementType)
            ? elementType
            : null;
    }

    public static Type Underlying(Type type) => Nullable.GetUnderlyingType(type) ?? type;
}
