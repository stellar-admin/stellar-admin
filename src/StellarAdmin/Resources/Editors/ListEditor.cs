using System.ComponentModel.DataAnnotations;
using StellarAdmin.UI;

namespace StellarAdmin.Resources.Editors;

/// <summary>
///     Configures an editor for a list of text or number values, with a row per item.
/// </summary>
/// <remarks>
///     The property must be an array or list of strings or numbers. The items are checked again
///     on the server when the form is saved.
/// </remarks>
public sealed class ListEditor : FieldEditor
{
    /// <summary>
    ///     Gets or sets the text of the button that adds an item.
    /// </summary>
    /// <value>
    ///     The text, or <see langword="null" /> to use the list editor's label option. The default
    ///     is <see langword="null" />.
    /// </value>
    public string? AddText { get; set; }

    /// <summary>
    ///     Gets or sets how each item is aligned within its row.
    /// </summary>
    /// <value>
    ///     One of the enumeration values that specifies the alignment, or <see langword="null" />
    ///     to align numbers to the end and text to the start. The default is
    ///     <see langword="null" />.
    /// </value>
    public ListEditorAlign? Align { get; set; }

    /// <summary>
    ///     Gets or sets how an item's case changes as it is entered.
    /// </summary>
    /// <value>
    ///     One of the enumeration values that specifies the case. The default is
    ///     <see cref="ListEditorCasing.None" />.
    /// </value>
    public ListEditorCasing Casing { get; set; }

    /// <summary>
    ///     Gets or sets the text shown when the list has no items.
    /// </summary>
    /// <value>
    ///     The text, or <see langword="null" /> to use the list editor's label option. The default
    ///     is <see langword="null" />.
    /// </value>
    public string? EmptyText { get; set; }

    /// <summary>
    ///     Gets or sets the largest number a number item can be.
    /// </summary>
    public double? Max { get; set; }

    /// <summary>
    ///     Gets or sets the largest number of items.
    /// </summary>
    /// <value>
    ///     The number, or <see langword="null" /> to use the length from a
    ///     <see cref="MaxLengthAttribute" /> or <see cref="LengthAttribute" /> on the property.
    ///     The default is <see langword="null" />.
    /// </value>
    public int? MaxItems { get; set; }

    /// <summary>
    ///     Gets or sets the largest number of characters a text item can have.
    /// </summary>
    public int? MaxLength { get; set; }

    /// <summary>
    ///     Gets or sets the smallest number a number item can be.
    /// </summary>
    public double? Min { get; set; }

    /// <summary>
    ///     Gets or sets the smallest number of items.
    /// </summary>
    /// <value>
    ///     The number, or <see langword="null" /> to use the length from a
    ///     <see cref="MinLengthAttribute" /> or <see cref="LengthAttribute" /> on the property.
    ///     The default is <see langword="null" />.
    /// </value>
    public int? MinItems { get; set; }

    /// <summary>
    ///     Gets or sets a value that indicates whether each item shows its number.
    /// </summary>
    /// <value>
    ///     <see langword="true" /> to number the items; otherwise, <see langword="false" />. The
    ///     default is <see langword="false" />.
    /// </value>
    public bool Numbered { get; set; }

    /// <summary>
    ///     Gets or sets a regular expression that each text item must match.
    /// </summary>
    public string? Pattern { get; set; }

    /// <summary>
    ///     Gets or sets the message shown when an item doesn't match <see cref="Pattern" />.
    /// </summary>
    /// <value>
    ///     The message, or <see langword="null" /> to use the browser's message, and the list
    ///     editor's label option on the server. The default is <see langword="null" />.
    /// </value>
    public string? PatternMessage { get; set; }

    /// <summary>
    ///     Gets or sets the placeholder of an empty item.
    /// </summary>
    public string? Placeholder { get; set; }

    /// <summary>
    ///     Gets or sets the text shown before each item, such as a currency symbol.
    /// </summary>
    public string? Prefix { get; set; }

    /// <summary>
    ///     Gets or sets a value that indicates whether the items can be reordered.
    /// </summary>
    /// <value>
    ///     <see langword="true" /> to reorder items by dragging their handle or with Alt+Up and
    ///     Alt+Down; otherwise, <see langword="false" />. The default is
    ///     <see langword="false" />.
    /// </value>
    public bool Sortable { get; set; }

    /// <summary>
    ///     Gets or sets the step of a number item.
    /// </summary>
    /// <value>
    ///     The step, or <see langword="null" /> for <c>1</c> for a whole-number type and any step
    ///     for other numbers. The default is <see langword="null" />.
    /// </value>
    public double? Step { get; set; }

    /// <summary>
    ///     Gets or sets the text shown after each item, such as a unit.
    /// </summary>
    public string? Suffix { get; set; }

    /// <inheritdoc />
    public override string TemplateName => "Editors/ListEditor";

    /// <summary>
    ///     Gets or sets a value that indicates whether each item must be different.
    /// </summary>
    /// <value>
    ///     <see langword="true" /> to reject an item that is already in the list, ignoring case;
    ///     otherwise, <see langword="false" />. The default is <see langword="false" />.
    /// </value>
    public bool Unique { get; set; }
}
