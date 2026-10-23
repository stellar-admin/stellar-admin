using System.Linq.Expressions;
using StellarAdmin.Diagnostics;
using StellarAdmin.Resources.Builders;

namespace StellarAdmin.Resources.Editors;

/// <summary>
///     Configures a lookup editor, which selects one item from a long list in a searchable sheet.
/// </summary>
public class LookupSheetEditor
    : FieldEditor,
        IFieldEditor<LookupSheetEditorHandler>,
        ILookupSheetEditor
{
    /// <summary>
    ///     Gets the additional CSS classes for the parts of the editor.
    /// </summary>
    public override LookupSheetEditorClassNames ClassNames { get; } = new();

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

    /// <inheritdoc />
    public override string TemplateName => "Editors/LookupSheet";

    internal bool CreateEnabled { get; private set; }

    internal Type? CreateResourceType { get; private set; }

    internal ConfigurationSource? CreateSource { get; private set; }

    internal LookupFieldOptions FieldOptions { get; } = new();

    internal LookupSheetOptions SheetOptions { get; } = new();

    string? ILookupSheetEditor.MediaClassName => ClassNames.Media;

    LookupSheetOptions ILookupSheetEditor.SheetOptions => SheetOptions;

    internal LookupSheetEditor() { }

    /// <summary>
    ///     Configures how the field is displayed in the form.
    /// </summary>
    /// <param name="configure">A delegate that configures the field.</param>
    /// <exception cref="ArgumentNullException">
    ///     <paramref name="configure" /> is <see langword="null" />.
    /// </exception>
    public void Field(Action<LookupFieldOptions> configure)
    {
        ArgumentNullException.ThrowIfNull(configure);

        configure(FieldOptions);
    }

    /// <summary>
    ///     Shows a button that creates a new item while nothing is selected.
    /// </summary>
    /// <remarks>
    ///     The create form of the resource registered for the items' type creates the item. That
    ///     resource must have a create page. Only users who can create the resource see the button.
    ///     When the items have no type, use <see cref="EnableCreate{TResource}" /> instead.
    /// </remarks>
    public void EnableCreate()
    {
        CreateEnabled = true;
        CreateSource = ConfigurationSource.Capture();
    }

    /// <summary>
    ///     Shows a button that creates a new item while nothing is selected.
    /// </summary>
    /// <typeparam name="TResource">
    ///     The type of the resource whose create form creates the item.
    /// </typeparam>
    /// <remarks>
    ///     The resource must have a create page. Only users who can create the resource see the
    ///     button.
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

    // Items with a description show their selection as a card, unless the field sets its layout
    internal LookupSheetEditorLayout GetLayout(LookupItems? items) =>
        FieldOptions.Layout
        ?? (
            items?.HasDescription == true
                ? LookupSheetEditorLayout.Card
                : LookupSheetEditorLayout.Input
        );
}

/// <summary>
///     Configures a lookup editor on a form.
/// </summary>
/// <typeparam name="TModel">The type of the form's model.</typeparam>
public sealed class LookupSheetEditor<TModel> : LookupSheetEditor
{
    internal LookupSheetEditor() { }

    /// <summary>
    ///     Takes the items from a resource, and configures them for this field.
    /// </summary>
    /// <typeparam name="TResource">The type of the resource.</typeparam>
    /// <param name="configure">A delegate that configures the items.</param>
    /// <exception cref="ArgumentNullException">
    ///     <paramref name="configure" /> is <see langword="null" />.
    /// </exception>
    /// <remarks>
    ///     See <see cref="LookupSheetEditor.UseResource{TResource}()" />.
    /// </remarks>
    public void UseResource<TResource>(Action<ResourceItemsBuilder<TModel, TResource>> configure) =>
        UseResource(ReferencedResource.For(configure));

    /// <summary>
    ///     Takes the items from a lookup source.
    /// </summary>
    /// <typeparam name="TSource">The type of the source.</typeparam>
    /// <remarks>
    ///     The source is resolved from the request's services, or created with them when it isn't
    ///     registered. A source whose values aren't the field's type, or whose search takes a value
    ///     from the form, is a configuration problem, and the field keeps its default editor.
    /// </remarks>
    public void UseSource<TSource>()
        where TSource : LookupSource =>
        UseItems(SourceLookupItems.Create<TModel>(typeof(TSource), null, TypeName));

    /// <summary>
    ///     Takes the items from a lookup source that searches with a value from the form.
    /// </summary>
    /// <typeparam name="TSource">The type of the source.</typeparam>
    /// <param name="argument">
    ///     An expression that returns the value the source searches with, from the form's model.
    /// </param>
    /// <exception cref="ArgumentNullException">
    ///     <paramref name="argument" /> is <see langword="null" />.
    /// </exception>
    /// <remarks>
    ///     Declare each field the argument reads with <c>DependsOn</c>: the form holds only those
    ///     fields' values, and the key on the edit form, and the sheet doesn't search while any of
    ///     them has no value. A value whose type isn't the one the source searches with, or its
    ///     nullable form, is a configuration problem, and the field keeps its default editor.
    /// </remarks>
    public void UseSource<TSource>(Expression<Func<TModel, object?>> argument)
        where TSource : LookupSource
    {
        ArgumentNullException.ThrowIfNull(argument);

        UseItems(SourceLookupItems.Create(typeof(TSource), argument, TypeName));
    }
}
