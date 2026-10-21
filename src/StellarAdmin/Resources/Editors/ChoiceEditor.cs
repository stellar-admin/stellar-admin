using System.ComponentModel.DataAnnotations;
using System.Globalization;
using System.Reflection;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Extensions.DependencyInjection;
using StellarAdmin.Diagnostics;

namespace StellarAdmin.Resources.Editors;

/// <summary>
///     Configures the choices of a choice editor.
/// </summary>
/// <remarks>
///     Without <c>UseItems</c>, the choices come from the property, which must be an enum without
///     flags or a Boolean.
/// </remarks>
public abstract class ChoiceEditor : FieldEditor, IFieldEditor<ChoiceEditorHandler>
{
    /// <summary>
    ///     Gets the additional CSS classes for the parts of the editor.
    /// </summary>
    public override ChoiceEditorClassNames ClassNames { get; } = new();

    /// <summary>
    ///     Gets or sets whether the choices start with an empty choice, which clears the value.
    /// </summary>
    /// <value>
    ///     One of the enumeration values that specifies whether the choices start with an empty
    ///     choice. The default is <see cref="EmptyChoice.Auto" />.
    /// </value>
    /// <remarks>
    ///     A collection property never gets an empty choice.
    /// </remarks>
    public EmptyChoice EmptyChoice { get; set; }

    /// <summary>
    ///     Gets or sets the text of the empty choice.
    /// </summary>
    /// <value>
    ///     The text of the empty choice. The default is "Not set".
    /// </value>
    public string EmptyChoiceText { get; set; } = "Not set";

    internal Func<IServiceProvider, CancellationToken, Task<IReadOnlyList<ChoiceItem>>>? ItemsLoader
    {
        get;
        private set;
    }

    // A select shows its first choice as selected when none is, so it keeps the empty choice while a required value
    // is unset or matches no choice, such as the 0 of a new entity's foreign key
    internal virtual bool KeepsEmptyChoiceWhileUnset => false;

    /// <summary>
    ///     Sets a registered provider that supplies the choices for each request.
    /// </summary>
    /// <typeparam name="TProvider">The type of the provider.</typeparam>
    /// <remarks>
    ///     Register <typeparamref name="TProvider" /> as a service.
    /// </remarks>
    public void UseItems<TProvider>()
        where TProvider : class, IChoiceItemsProvider
    {
        ItemsLoader = (services, cancellationToken) =>
            services.GetRequiredService<TProvider>().GetItemsAsync(cancellationToken);
    }

    /// <summary>
    ///     Sets a fixed list of choices.
    /// </summary>
    /// <param name="items">The choices, in display order.</param>
    /// <exception cref="ArgumentNullException">
    ///     <paramref name="items" /> is <see langword="null" />.
    /// </exception>
    public void UseItems(IEnumerable<ChoiceItem> items)
    {
        ArgumentNullException.ThrowIfNull(items);

        var snapshot = items.ToArray();
        ItemsLoader = (_, _) => Task.FromResult<IReadOnlyList<ChoiceItem>>(snapshot);
    }

    /// <summary>
    ///     Sets a fixed list of choices from select list items.
    /// </summary>
    /// <param name="items">The select list items, in display order.</param>
    /// <exception cref="ArgumentNullException">
    ///     <paramref name="items" /> is <see langword="null" />.
    /// </exception>
    /// <remarks>
    ///     An item without a value posts its text. The field's value decides which choices are
    ///     selected, so <see cref="SelectListItem.Selected" /> is ignored.
    /// </remarks>
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
    ///     Sets a delegate that loads the choices for each request.
    /// </summary>
    /// <param name="itemsLoader">
    ///     A delegate that loads the choices with the request's services.
    /// </param>
    /// <exception cref="ArgumentNullException">
    ///     <paramref name="itemsLoader" /> is <see langword="null" />.
    /// </exception>
    public void UseItems(
        Func<IServiceProvider, CancellationToken, Task<IReadOnlyList<ChoiceItem>>> itemsLoader
    )
    {
        ArgumentNullException.ThrowIfNull(itemsLoader);

        ItemsLoader = itemsLoader;
    }

    internal IReadOnlyList<ChoiceItem> ResolveChoices(
        object? editorData,
        ModelMetadata metadata,
        object? model
    )
    {
        var choices = editorData as IReadOnlyList<ChoiceItem> ?? PropertyChoices(metadata);

        // Supplied choices that already clear the value keep their own empty choice
        return
            IncludesEmptyChoice(metadata, model, choices)
            && !choices.Any(choice => choice.Value == "")
            ? [new ChoiceItem("", EmptyChoiceText), .. choices]
            : choices;
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

    // A select matches an enum by name or number and other values by their text, ignoring case
    private static bool MatchesChoice(object? model, IReadOnlyList<ChoiceItem> choices)
    {
        if (model is null)
        {
            return false;
        }

        string?[] values =
            model is Enum
                ?
                [
                    model.ToString(),
                    Convert.ToString(
                        Convert.ChangeType(
                            model,
                            Enum.GetUnderlyingType(model.GetType()),
                            CultureInfo.InvariantCulture
                        ),
                        CultureInfo.InvariantCulture
                    ),
                ]
                : [Convert.ToString(model, CultureInfo.CurrentCulture)];

        return choices.Any(choice =>
            values.Any(value =>
                string.Equals(value, choice.Value, StringComparison.OrdinalIgnoreCase)
            )
        );
    }

    private bool IncludesEmptyChoice(
        ModelMetadata metadata,
        object? model,
        IReadOnlyList<ChoiceItem> choices
    ) =>
        metadata.ElementMetadata is null
        && EmptyChoice switch
        {
            EmptyChoice.Include => true,
            EmptyChoice.Omit => false,
            _ => !metadata.IsRequired
                || (KeepsEmptyChoiceWhileUnset && !MatchesChoice(model, choices)),
        };

    private List<ChoiceItem> PropertyChoices(ModelMetadata metadata)
    {
        // A collection property takes its choices from the element type
        var valueMetadata = metadata.ElementMetadata ?? metadata;
        var valueType = valueMetadata.UnderlyingOrModelType;
        if (valueMetadata.IsEnum && !valueMetadata.IsFlagsEnum)
        {
            return EnumChoices(valueMetadata, valueType);
        }

        if (valueType == typeof(bool))
        {
            return [new("true", "Yes"), new("false", "No")];
        }

        throw new ConfigurationException(
            ConfigurationProblemKind.UnsuitableEditor,
            $"`{GetType().Name}` has no items, and `{metadata.PropertyName}` isn't a non-flags enum or a Boolean to take them from",
            $"Give the `{GetType().Name}` items with `UseItems`."
        );
    }
}
