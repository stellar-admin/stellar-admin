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

    internal Func<IServiceProvider, CancellationToken, Task<IReadOnlyList<ChoiceItem>>>? ItemsLoader
    {
        get;
        private set;
    }

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
    public void UseItems(IEnumerable<ChoiceItem> items)
    {
        ArgumentNullException.ThrowIfNull(items);

        var snapshot = items.ToArray();
        ItemsLoader = (_, _) => Task.FromResult<IReadOnlyList<ChoiceItem>>(snapshot);
    }

    /// <summary>
    ///     Supplies a fixed set of choices from select list items. An item without a value posts its text, and its
    ///     selected state is ignored, since the field's value decides the selection.
    /// </summary>
    public void UseItems(IEnumerable<SelectListItem> items)
    {
        ArgumentNullException.ThrowIfNull(items);

        UseItems(
            items.Select(item => new ChoiceItem(item.Value ?? item.Text, item.Text)
            {
                Disabled = item.Disabled,
                Group = item.Group is null
                    ? null
                    : new ChoiceGroup(item.Group.Name ?? "") { Disabled = item.Group.Disabled },
            })
        );
    }

    /// <summary>
    ///     Supplies choices from a request-aware asynchronous loader.
    /// </summary>
    public void UseItems(
        Func<IServiceProvider, CancellationToken, Task<IReadOnlyList<ChoiceItem>>> itemsLoader
    )
    {
        ArgumentNullException.ThrowIfNull(itemsLoader);

        ItemsLoader = itemsLoader;
    }

    internal IReadOnlyList<ChoiceItem> ResolveChoices(object? editorData, ModelMetadata metadata)
    {
        if (editorData is IReadOnlyList<ChoiceItem> items)
        {
            return items;
        }

        // A collection property takes its choices from the element type
        var valueMetadata = metadata.ElementMetadata ?? metadata;
        var valueType = valueMetadata.UnderlyingOrModelType;
        List<ChoiceItem> choices;
        if (valueMetadata.IsEnum && !valueMetadata.IsFlagsEnum)
        {
            choices = EnumChoices(valueMetadata, valueType);
        }
        else if (valueType == typeof(bool))
        {
            choices = [new("true", "Yes"), new("false", "No")];
        }
        else
        {
            throw new InvalidOperationException(
                $"{GetType().Name} on {metadata.PropertyName} requires UseItems unless the property is a non-flags enum or a Boolean."
            );
        }

        if (metadata.IsNullableValueType)
        {
            choices.Insert(0, new("", EmptyChoiceText));
        }

        return choices;
    }

    private static List<ChoiceItem> EnumChoices(ModelMetadata metadata, Type enumType)
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

                return new ChoiceItem(name, pair.Key.Name)
                {
                    Description = description,
                    Group = string.IsNullOrEmpty(pair.Key.Group)
                        ? null
                        : new ChoiceGroup(pair.Key.Group),
                };
            })
            .ToList();
    }
}
