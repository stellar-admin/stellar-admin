using System.ComponentModel.DataAnnotations;
using System.Reflection;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Extensions.DependencyInjection;

namespace StellarAdmin.Dashboard.Resources.Editors;

/// <summary>
///     Configures the choices of a choice editor. Without items, an enum or Boolean property supplies them.
/// </summary>
public abstract class ChoiceEditor : FieldEditor
{
    /// <summary>
    ///     The text of the empty choice added for a nullable enum or Boolean property.
    /// </summary>
    public string EmptyChoiceText { get; set; } = "Not set";

    internal Func<
        IServiceProvider,
        CancellationToken,
        Task<IReadOnlyList<SelectListItem>>
    >? ItemsLoader { get; private set; }

    /// <summary>
    ///     Selects a registered provider that supplies choices for each request.
    /// </summary>
    public void UseItems<TProvider>()
        where TProvider : class, IChoiceItemsProvider
    {
        ItemsLoader = (services, cancellationToken) =>
            services.GetRequiredService<TProvider>().GetItemsAsync(cancellationToken);
    }

    /// <summary>
    ///     Supplies a fixed set of choices.
    /// </summary>
    public void UseItems(IEnumerable<SelectListItem> items)
    {
        ArgumentNullException.ThrowIfNull(items);

        var snapshot = items.ToArray();
        ItemsLoader = (_, _) => Task.FromResult<IReadOnlyList<SelectListItem>>(snapshot);
    }

    /// <summary>
    ///     Supplies choices from a request-aware asynchronous loader.
    /// </summary>
    public void UseItems(
        Func<IServiceProvider, CancellationToken, Task<IReadOnlyList<SelectListItem>>> itemsLoader
    )
    {
        ArgumentNullException.ThrowIfNull(itemsLoader);

        ItemsLoader = itemsLoader;
    }

    internal IReadOnlyList<EditorChoice> ResolveChoices(object? editorData, ModelMetadata metadata)
    {
        if (editorData is IEnumerable<SelectListItem> items)
        {
            return items
                .Select(item => new EditorChoice(
                    item.Value ?? item.Text,
                    item.Text,
                    null,
                    item.Disabled
                ))
                .ToArray();
        }

        // A collection property takes its choices from the element type
        var valueMetadata = metadata.ElementMetadata ?? metadata;
        var valueType = valueMetadata.UnderlyingOrModelType;
        List<EditorChoice> choices;
        if (valueMetadata.IsEnum && !valueMetadata.IsFlagsEnum)
        {
            choices = EnumChoices(valueMetadata, valueType);
        }
        else if (valueType == typeof(bool))
        {
            choices = [new("true", "Yes", null, false), new("false", "No", null, false)];
        }
        else
        {
            throw new InvalidOperationException(
                $"{GetType().Name} on {metadata.PropertyName} requires UseItems unless the property is a non-flags enum or a Boolean."
            );
        }

        if (metadata.IsNullableValueType)
        {
            choices.Insert(0, new("", EmptyChoiceText, null, false));
        }

        return choices;
    }

    private static List<EditorChoice> EnumChoices(ModelMetadata metadata, Type enumType)
    {
        // Choices submit the member name, which binds and matches the model like the number does
        var names = metadata
            .EnumNamesAndValues!.GroupBy(pair => pair.Value)
            .ToDictionary(group => group.Key, group => group.First().Key);

        return metadata
            .EnumGroupedDisplayNamesAndValues!.Select(pair =>
            {
                var name = names[pair.Value];
                var description = enumType
                    .GetField(name)
                    ?.GetCustomAttribute<DisplayAttribute>()
                    ?.GetDescription();

                return new EditorChoice(name, pair.Key.Name, description, false);
            })
            .ToList();
    }
}

internal sealed record EditorChoice(string Value, string Text, string? Description, bool Disabled);
