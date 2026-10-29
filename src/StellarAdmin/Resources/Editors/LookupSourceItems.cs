using System.Collections.Concurrent;
using System.Globalization;
using System.Linq.Expressions;
using Microsoft.Extensions.DependencyInjection;
using StellarAdmin.Diagnostics;

namespace StellarAdmin.Resources.Editors;

// What each item of a lookup source shows, shared by the source classes with and without an argument
internal interface ILookupSourceItem<in TItem, out TValue>
{
    string? GetAvatar(TItem item);

    string? GetCode(TItem item);

    string? GetDescription(TItem item);

    string? GetIcon(TItem item);

    string? GetImage(TItem item);

    string GetTitle(TItem item);

    TValue GetValue(TItem item);
}

// Turns a source's items into choices
internal static class LookupSourceItems
{
    public static async Task<IReadOnlyList<ChoiceItem>> FindAsync<TItem, TValue>(
        ILookupSourceItem<TItem, TValue> source,
        Func<
            IReadOnlyCollection<TValue>,
            CancellationToken,
            Task<IReadOnlyCollection<TItem>>
        > find,
        FieldEditorContext context,
        CancellationToken cancellationToken
    )
    {
        var values = ReadValues<TValue>(context);
        if (values.Count == 0)
        {
            return [];
        }

        var items = await find(values, cancellationToken);
        var mediaType = LookupSourceShape.For(source.GetType()).MediaType;

        // A value the source no longer has is still displayed, so the selection stays visible. A value type's default,
        // such as a new record's key of 0, is no selection
        return values
            .Select(current =>
                items.FirstOrDefault(item =>
                    EqualityComparer<TValue>.Default.Equals(source.GetValue(item), current)
                )
                    is { } item
                    ? CreateChoice(source, item, mediaType)
                    : IsUnset(current) ? null
                    : new ChoiceItem(
                        FormatValue(current),
                        Convert.ToString(current, CultureInfo.InvariantCulture) ?? ""
                    )
            )
            .OfType<ChoiceItem>()
            .ToArray();
    }

    public static LookupResults ToResults<TItem, TValue>(
        ILookupSourceItem<TItem, TValue> source,
        LookupPage<TItem> page
    )
    {
        var mediaType = LookupSourceShape.For(source.GetType()).MediaType;

        return new LookupResults(
            page.Items.Select(item => CreateChoice(source, item, mediaType)).ToArray(),
            page.HasMore
        );
    }

    private static ChoiceItem CreateChoice<TItem, TValue>(
        ILookupSourceItem<TItem, TValue> source,
        TItem item,
        Type? mediaType
    ) =>
        new(FormatValue(source.GetValue(item)), source.GetTitle(item))
        {
            Description = source.GetDescription(item),
            Media = CreateMedia(source, item, mediaType),
        };

    private static ItemMedia? CreateMedia<TItem, TValue>(
        ILookupSourceItem<TItem, TValue> source,
        TItem item,
        Type? mediaType
    )
    {
        if (mediaType == typeof(ItemMedia.Avatar))
        {
            return new ItemMedia.Avatar(source.GetAvatar(item));
        }

        if (mediaType == typeof(ItemMedia.Code))
        {
            return source.GetCode(item) is { Length: > 0 } code ? new ItemMedia.Code(code) : null;
        }

        if (mediaType == typeof(ItemMedia.Icon))
        {
            return source.GetIcon(item) is { Length: > 0 } name ? new ItemMedia.Icon(name) : null;
        }

        return mediaType == typeof(ItemMedia.Image)
            && source.GetImage(item) is { Length: > 0 } url
            ? new ItemMedia.Image(url)
            : null;
    }

    // Selected values are posted with the form, which binds them in the current culture
    private static string FormatValue<TValue>(TValue current) =>
        Convert.ToString(current, CultureInfo.CurrentCulture) ?? "";

    private static bool IsUnset<TValue>(TValue current) =>
        typeof(TValue).IsValueType && EqualityComparer<TValue>.Default.Equals(current, default!);

    private static IReadOnlyList<TValue> ReadValues<TValue>(FieldEditorContext context) =>
        context.Value switch
        {
            null => [],
            TValue current => [current],
            IEnumerable<TValue> values => values
                .Where(current => current is not null)
                .Distinct()
                .ToArray(),
            _ => throw new ConfigurationException(
                ConfigurationProblemKind.UnsuitableEditor,
                $"its value is a `{context.Value.GetType().Name}`, but the lookup's items use `{typeof(TValue).Name}`",
                $"Make the lookup's items use the property's type, or bind the field to a `{typeof(TValue).Name}` property."
            ),
        };
}

// What a lookup source's type says about its items, read from its base class and the media methods it overrides
internal sealed record LookupSourceShape(
    Type ItemType,
    Type ValueType,
    Type? ArgumentType,
    bool HasDescription,
    IReadOnlyList<string> MediaMethods,
    Type? MediaType
)
{
    private static readonly ConcurrentDictionary<Type, LookupSourceShape> Shapes = new();

    private static readonly (string Method, Type Media)[] Media =
    [
        (nameof(LookupSource<,>.GetAvatar), typeof(ItemMedia.Avatar)),
        (nameof(LookupSource<,>.GetCode), typeof(ItemMedia.Code)),
        (nameof(LookupSource<,>.GetIcon), typeof(ItemMedia.Icon)),
        (nameof(LookupSource<,>.GetImage), typeof(ItemMedia.Image)),
    ];

    public static LookupSourceShape For(Type sourceType) => Shapes.GetOrAdd(sourceType, Read);

    private static LookupSourceShape Read(Type sourceType)
    {
        var baseType = sourceType;
        while (
            baseType is not null
            && !(
                baseType.IsGenericType
                && baseType.GetGenericTypeDefinition() is var definition
                && (definition == typeof(LookupSource<,>) || definition == typeof(LookupSource<,,>))
            )
        )
        {
            baseType = baseType.BaseType;
        }

        if (baseType is null)
        {
            throw new ArgumentException(
                $"{sourceType.Name} doesn't derive from LookupSource<TItem, TValue> or LookupSource<TItem, TValue, TArgument>.",
                nameof(sourceType)
            );
        }

        var arguments = baseType.GetGenericArguments();
        var itemType = arguments[0];
        var media = Media.Where(media => Overrides(media.Method)).ToArray();

        return new LookupSourceShape(
            itemType,
            arguments[1],
            arguments.Length == 3 ? arguments[2] : null,
            Overrides(nameof(LookupSource<,>.GetDescription)),
            media.Select(media => media.Method).ToArray(),
            media.FirstOrDefault().Media
        );

        bool Overrides(string name) => sourceType.GetMethod(name, [itemType])?.DeclaringType != baseType;
    }
}

// Items from a lookup source, resolved for each request
internal sealed class SourceLookupItems(
    Type sourceType,
    Func<object, object?>? argument,
    Type? argumentType
) : LookupItems
{
    // The type of the value the field passes to the source's search, or null when it passes none
    public Type? ArgumentType => argumentType;

    public override bool HasDescription => Shape.HasDescription;

    public override Type ItemType => Shape.ItemType;

    public override Type? MediaType => Shape.MediaType;

    public LookupSourceShape Shape { get; } = LookupSourceShape.For(sourceType);

    public Type SourceType => sourceType;

    public override Type ValueType => Shape.ValueType;

    public static SourceLookupItems Create<TModel>(
        Type sourceType,
        Expression<Func<TModel, object?>>? argument,
        string editorName
    )
    {
        ArgumentNullException.ThrowIfNull(sourceType);
        if (argument is null)
        {
            return new(sourceType, null, null);
        }

        // The expression returns object, so the value's own type is the operand of the boxing conversion
        var body =
            argument.Body
                is UnaryExpression
                {
                    NodeType: ExpressionType.Convert or ExpressionType.ConvertChecked,
                } convert
            && argument.Body.Type == typeof(object)
                ? convert.Operand
                : argument.Body;
        var compiled = argument.Compile();

        return new(
            sourceType,
            model => compiled(FormModel.Cast<TModel>(model, editorName)),
            body.Type
        );
    }

    public override Task<IReadOnlyList<ChoiceItem>> FindAsync(
        IServiceProvider services,
        FieldEditorContext context,
        CancellationToken cancellationToken
    ) => Resolve(services).FindItemsAsync(context, cancellationToken);

    public override Task<LookupResults> SearchAsync(
        IServiceProvider services,
        LookupQuery query,
        CancellationToken cancellationToken
    )
    {
        object? value = null;
        if (argument is not null)
        {
            var model =
                query.Model
                ?? throw new InvalidOperationException(
                    "The lookup's items are searched with a value from the form, but the search has no form."
                );
            value = argument(model);
        }

        return Resolve(services).SearchItemsAsync(value, query, cancellationToken);
    }

    private LookupSource Resolve(IServiceProvider services) =>
        (LookupSource)ActivatorUtilities.GetServiceOrCreateInstance(services, sourceType);
}
