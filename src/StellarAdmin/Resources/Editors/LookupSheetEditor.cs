using System.Linq.Expressions;
using StellarAdmin.Resources.Builders;

namespace StellarAdmin.Resources.Editors;

/// <summary>
///     Configures a lookup editor, which selects one item from a long list in a searchable sheet.
/// </summary>
public class LookupSheetEditor : LookupSheetEditorBase, IFieldEditor<LookupSheetEditorHandler>
{
    /// <summary>
    ///     Gets the additional CSS classes for the parts of the editor.
    /// </summary>
    public override LookupSheetEditorClassNames ClassNames { get; } = new();

    /// <inheritdoc />
    public override string TemplateName => "Editors/LookupSheet";

    internal LookupFieldOptions FieldOptions { get; } = new();

    internal override string? MediaClassName => ClassNames.Media;

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
    ///     See <see cref="LookupSheetEditorBase.UseResource{TResource}()" />.
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
