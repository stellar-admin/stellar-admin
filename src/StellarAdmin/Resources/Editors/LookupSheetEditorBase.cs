using Microsoft.Extensions.DependencyInjection;
using StellarAdmin.Diagnostics;

namespace StellarAdmin.Resources.Editors;

/// <summary>
///     Configures what the lookup editors share: their items and the sheet in which they're
///     searched.
/// </summary>
/// <remarks>
///     <see cref="LookupSheetEditor" /> selects one item and <see cref="MultiLookupSheetEditor" />
///     several. Take this type to configure either of them.
/// </remarks>
public abstract class LookupSheetEditorBase : FieldEditor
{
    /// <summary>
    ///     Gets the items the editor looks up.
    /// </summary>
    /// <value>
    ///     The items, or <see langword="null" /> until <c>UseItems</c> is called, and when the
    ///     editor takes its items from a resource.
    /// </value>
    public LookupItems? Items { get; private set; }

    /// <summary>
    ///     Gets the resource the editor takes its items from.
    /// </summary>
    /// <value>
    ///     The resource, or <see langword="null" /> until <c>UseResource</c> is called.
    /// </value>
    public ReferencedResource? Resource { get; private set; }

    internal bool CreateEnabled { get; private set; }

    internal Type? CreateResourceType { get; private set; }

    internal ConfigurationSource? CreateSource { get; private set; }

    internal abstract string? MediaClassName { get; }

    internal LookupSheetOptions SheetOptions { get; } = new();

    private protected LookupSheetEditorBase() { }

    /// <summary>
    ///     Shows a button that creates a new item and selects it.
    /// </summary>
    /// <remarks>
    ///     A lookup shows the button while nothing is selected, and a multi-select lookup shows it
    ///     in the sheet. The create form of the resource registered for the items' type creates the
    ///     item. That resource must have a create page. Only users who can create the resource see
    ///     the button. When the items have no type, use <see cref="EnableCreate{TResource}" />
    ///     instead.
    /// </remarks>
    public void EnableCreate()
    {
        CreateEnabled = true;
        CreateSource = ConfigurationSource.Capture();
    }

    /// <summary>
    ///     Shows a button that creates a new item and selects it.
    /// </summary>
    /// <typeparam name="TResource">
    ///     The type of the resource whose create form creates the item.
    /// </typeparam>
    /// <remarks>
    ///     A lookup shows the button while nothing is selected, and a multi-select lookup shows it
    ///     in the sheet. The resource must have a create page. Only users who can create the
    ///     resource see the button.
    /// </remarks>
    public void EnableCreate<TResource>()
    {
        CreateEnabled = true;
        CreateResourceType = typeof(TResource);
        CreateSource = ConfigurationSource.Capture();
    }

    /// <summary>
    ///     Configures the sheet in which items are searched.
    /// </summary>
    /// <param name="configure">A delegate that configures the sheet.</param>
    /// <exception cref="ArgumentNullException">
    ///     <paramref name="configure" /> is <see langword="null" />.
    /// </exception>
    public void Sheet(Action<LookupSheetOptions> configure)
    {
        ArgumentNullException.ThrowIfNull(configure);

        configure(SheetOptions);
    }

    /// <summary>
    ///     Sets the items the editor looks up.
    /// </summary>
    /// <param name="items">The items, such as those an integration package supplies.</param>
    /// <exception cref="ArgumentNullException">
    ///     <paramref name="items" /> is <see langword="null" />.
    /// </exception>
    public void UseItems(LookupItems items)
    {
        ArgumentNullException.ThrowIfNull(items);

        Items = items;
        Resource = null;
    }

    /// <summary>
    ///     Takes the items from a resource: its records, named by their titles.
    /// </summary>
    /// <typeparam name="TResource">The type of the resource.</typeparam>
    /// <remarks>
    ///     Each item's value is the record's key. The resource's <c>Reference</c> settings add a
    ///     description and media, and an EF Core resource's settings search, order and filter the
    ///     items. When no resource is registered for <typeparamref name="TResource" />, the editor
    ///     has no items.
    /// </remarks>
    public void UseResource<TResource>() => UseResource(ReferencedResource.For<TResource>());

    internal void UseResource(ReferencedResource resource)
    {
        Resource = resource;
        Items = null;
    }

    // The items UseItems set, or those of the resource UseResource refers to, found when the form renders
    internal LookupItems? ResolveItems(IServiceProvider services) =>
        Items
        ?? (
            Resource is { } resource
                ? services.GetRequiredService<ResourceReferences>().GetItems(resource)
                : null
        );
}
