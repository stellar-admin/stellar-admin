using System.ComponentModel.DataAnnotations;

namespace StellarAdmin.Resources.Editors;

/// <summary>
///     Configures an editor for a dictionary of keys and values, with a row per entry.
/// </summary>
/// <remarks>
///     The property must be a <see cref="Dictionary{TKey, TValue}" /> or
///     <see cref="IDictionary{TKey, TValue}" /> with a string or enum key and a string, number or
///     enum value. The entries are checked again on the server when the form is saved.
/// </remarks>
public class DictionaryEditor : FieldEditor, IFieldEditor<DictionaryEditorHandler>
{
    /// <summary>
    ///     Gets or sets the text of the button that adds an entry.
    /// </summary>
    /// <value>
    ///     The text, or <see langword="null" /> to use the dictionary editor's label option. The
    ///     default is <see langword="null" />.
    /// </value>
    public string? AddText { get; set; }

    /// <summary>
    ///     Gets or sets the text shown when the dictionary has no entries.
    /// </summary>
    /// <value>
    ///     The text, or <see langword="null" /> to use the dictionary editor's label option. The
    ///     default is <see langword="null" />.
    /// </value>
    public string? EmptyText { get; set; }

    /// <summary>
    ///     Gets or sets the largest number of entries.
    /// </summary>
    /// <value>
    ///     The number, or <see langword="null" /> to use the length from a
    ///     <see cref="MaxLengthAttribute" /> or <see cref="LengthAttribute" /> on the property.
    ///     The default is <see langword="null" />.
    /// </value>
    /// <remarks>
    ///     Keys with items allow at most one entry for each item.
    /// </remarks>
    public int? MaxItems { get; set; }

    /// <summary>
    ///     Gets or sets the smallest number of entries.
    /// </summary>
    /// <value>
    ///     The number, or <see langword="null" /> to use the length from a
    ///     <see cref="MinLengthAttribute" /> or <see cref="LengthAttribute" /> on the property.
    ///     The default is <see langword="null" />.
    /// </value>
    public int? MinItems { get; set; }

    /// <summary>
    ///     Gets or sets a value that indicates whether each entry shows its number.
    /// </summary>
    /// <value>
    ///     <see langword="true" /> to number the entries; otherwise, <see langword="false" />. The
    ///     default is <see langword="false" />.
    /// </value>
    public bool Numbered { get; set; }

    /// <inheritdoc />
    public override string TemplateName => "Editors/DictionaryEditor";

    internal DictionaryEditorKey KeyPart { get; }

    internal DictionaryEditorValue ValuePart { get; }

    internal DictionaryEditor()
        : this(new DictionaryEditorKey(), new DictionaryEditorValue()) { }

    private protected DictionaryEditor(DictionaryEditorKey key, DictionaryEditorValue value)
    {
        KeyPart = key;
        ValuePart = value;
    }
}

/// <summary>
///     Configures a dictionary editor on a form.
/// </summary>
/// <typeparam name="TModel">The type of the form's model.</typeparam>
public sealed class DictionaryEditor<TModel> : DictionaryEditor
{
    internal DictionaryEditor()
        : base(new DictionaryEditorKey<TModel>(), new DictionaryEditorValue<TModel>()) { }

    /// <summary>
    ///     Configures the keys.
    /// </summary>
    /// <param name="configure">A delegate that configures the keys.</param>
    /// <exception cref="ArgumentNullException">
    ///     <paramref name="configure" /> is <see langword="null" />.
    /// </exception>
    public void Key(Action<DictionaryEditorKey<TModel>> configure)
    {
        ArgumentNullException.ThrowIfNull(configure);

        configure((DictionaryEditorKey<TModel>)KeyPart);
    }

    /// <summary>
    ///     Configures the values.
    /// </summary>
    /// <param name="configure">A delegate that configures the values.</param>
    /// <exception cref="ArgumentNullException">
    ///     <paramref name="configure" /> is <see langword="null" />.
    /// </exception>
    public void Value(Action<DictionaryEditorValue<TModel>> configure)
    {
        ArgumentNullException.ThrowIfNull(configure);

        configure((DictionaryEditorValue<TModel>)ValuePart);
    }
}
