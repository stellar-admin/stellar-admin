namespace StellarAdmin.Resources.Options;

/// <summary>
///     Stores the default text of list editors.
/// </summary>
public sealed class ListEditorLabelOptions
{
    /// <summary>
    ///     Gets or sets the callback that generates the text of the button that adds an item.
    /// </summary>
    /// <value>
    ///     The default returns <c>Add item</c>.
    /// </value>
    public Func<ListEditorLabelContext, string> AddText { get; set; } = context => "Add item";

    /// <summary>
    ///     Gets or sets the callback that generates the count shown on the add row when the list
    ///     has a maximum, with <c>{count}</c> for the number of items.
    /// </summary>
    /// <value>
    ///     The default returns <c>{count} of</c> followed by the maximum, such as <c>{count} of
    ///     8</c>.
    /// </value>
    public Func<ListEditorLabelContext, string> CountText { get; set; } =
        context => $"{{count}} of {context.MaxItems}";

    /// <summary>
    ///     Gets or sets the callback that generates the error shown when an item is already in the
    ///     list, with <c>{value}</c> for the item.
    /// </summary>
    /// <value>
    ///     The default returns <c>{value} is already in the list.</c>.
    /// </value>
    public Func<ListEditorLabelContext, string> DuplicateText { get; set; } =
        context => "{value} is already in the list.";

    /// <summary>
    ///     Gets or sets the callback that generates the text shown when the list has no items.
    /// </summary>
    /// <value>
    ///     The default returns <c>No items yet</c>.
    /// </value>
    public Func<ListEditorLabelContext, string> EmptyText { get; set; } = context => "No items yet";

    /// <summary>
    ///     Gets or sets the callback that generates the text shown when the list is full, and the
    ///     error when it has too many items.
    /// </summary>
    /// <value>
    ///     The default returns <c>You can add up to 8 items.</c> for a maximum of 8.
    /// </value>
    public Func<ListEditorLabelContext, string> FullText { get; set; } =
        context =>
            context.MaxItems == 1
                ? "You can add only one item."
                : $"You can add up to {context.MaxItems} items.";

    /// <summary>
    ///     Gets or sets the callback that generates the screen reader label of an item, with
    ///     <c>{n}</c> for its number.
    /// </summary>
    /// <value>
    ///     The default returns <c>Item {n}</c>.
    /// </value>
    public Func<ListEditorLabelContext, string> ItemLabel { get; set; } = context => "Item {n}";

    /// <summary>
    ///     Gets or sets the callback that generates the error shown on the server when a text item
    ///     is too long.
    /// </summary>
    /// <value>
    ///     The default returns <c>Use 120 characters or fewer.</c> for a maximum length of 120.
    /// </value>
    public Func<ListEditorLabelContext, string> MaxLengthText { get; set; } =
        context => $"Use {context.MaxLength} characters or fewer.";

    /// <summary>
    ///     Gets or sets the callback that generates the screen reader label of an item's drag
    ///     handle, with <c>{n}</c> for its number.
    /// </summary>
    /// <value>
    ///     The default returns <c>Move item {n}</c>.
    /// </value>
    public Func<ListEditorLabelContext, string> MoveLabel { get; set; } =
        context => "Move item {n}";

    /// <summary>
    ///     Gets or sets the callback that generates the text a read-only list shows when it has no
    ///     items.
    /// </summary>
    /// <value>
    ///     The default returns <c>None</c>.
    /// </value>
    public Func<ListEditorLabelContext, string> NoneText { get; set; } = context => "None";

    /// <summary>
    ///     Gets or sets the callback that generates the error shown on the server when a text item
    ///     doesn't match the editor's pattern and the editor has no pattern message.
    /// </summary>
    /// <value>
    ///     The default returns <c>Match the requested format.</c>.
    /// </value>
    public Func<ListEditorLabelContext, string> PatternText { get; set; } =
        context => "Match the requested format.";

    /// <summary>
    ///     Gets or sets the callback that generates the error shown on the server when a number
    ///     item is out of range.
    /// </summary>
    /// <value>
    ///     The default returns <c>Enter a number from 1 to 31.</c> for a range from 1 to 31.
    /// </value>
    public Func<ListEditorLabelContext, string> RangeText { get; set; } =
        context =>
            (context.Min, context.Max) switch
            {
                ({ } min, { } max) => $"Enter a number from {min} to {max}.",
                ({ } min, null) => $"Enter {min} or more.",
                _ => $"Enter {context.Max} or less.",
            };

    /// <summary>
    ///     Gets or sets the callback that generates the screen reader label of an item's remove
    ///     button, with <c>{n}</c> for its number.
    /// </summary>
    /// <value>
    ///     The default returns <c>Remove item {n}</c>.
    /// </value>
    public Func<ListEditorLabelContext, string> RemoveLabel { get; set; } =
        context => "Remove item {n}";

    /// <summary>
    ///     Gets or sets the callback that generates the error shown when the list has fewer items
    ///     than its minimum.
    /// </summary>
    /// <value>
    ///     The default returns <c>Add at least 2 items.</c> for a minimum of 2.
    /// </value>
    public Func<ListEditorLabelContext, string> TooFewText { get; set; } =
        context =>
            context.MinItems == 1
                ? "Add at least one item."
                : $"Add at least {context.MinItems} items.";
}
