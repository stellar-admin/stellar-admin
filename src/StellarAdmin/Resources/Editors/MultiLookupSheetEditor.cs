using System.Linq.Expressions;
using StellarAdmin.Resources.Builders;

namespace StellarAdmin.Resources.Editors;

/// <summary>
///     Configures a multi-select lookup editor, which selects several items from a long list in a
///     searchable sheet.
/// </summary>
/// <remarks>
///     The field binds a collection of values, such as an array or a list.
/// </remarks>
public class MultiLookupSheetEditor
    : LookupSheetEditorBase,
        IFieldEditor<MultiLookupSheetEditorHandler>
{
    /// <summary>
    ///     Gets the additional CSS classes for the parts of the editor.
    /// </summary>
    public override MultiLookupSheetEditorClassNames ClassNames { get; } = new();

    /// <inheritdoc />
    public override string TemplateName => "Editors/MultiLookupSheet";

    internal MultiLookupFieldOptions FieldOptions { get; } = new();

    internal override string? MediaClassName => ClassNames.Media;

    internal MultiLookupSheetEditor() { }

    /// <summary>
    ///     Configures how the field is displayed in the form.
    /// </summary>
    /// <param name="configure">A delegate that configures the field.</param>
    /// <exception cref="ArgumentNullException">
    ///     <paramref name="configure" /> is <see langword="null" />.
    /// </exception>
    public void Field(Action<MultiLookupFieldOptions> configure)
    {
        ArgumentNullException.ThrowIfNull(configure);

        configure(FieldOptions);
    }
}

/// <summary>
///     Configures a multi-select lookup editor on a form.
/// </summary>
/// <typeparam name="TModel">The type of the form's model.</typeparam>
public sealed class MultiLookupSheetEditor<TModel> : MultiLookupSheetEditor
{
    internal MultiLookupSheetEditor() { }

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
