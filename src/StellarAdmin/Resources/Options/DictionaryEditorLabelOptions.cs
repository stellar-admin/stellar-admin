namespace StellarAdmin.Resources.Options;

/// <summary>
///     Stores the default text of dictionary editors.
/// </summary>
public sealed class DictionaryEditorLabelOptions
{
    /// <summary>
    ///     Gets or sets the callback that generates the text of the button that adds an entry.
    /// </summary>
    /// <value>
    ///     The default returns <c>Add item</c>.
    /// </value>
    public Func<DictionaryEditorLabelContext, string> AddText { get; set; } = context => "Add item";

    /// <summary>
    ///     Gets or sets the callback that generates the count shown on the add row when the
    ///     dictionary has a maximum, with <c>{count}</c> for the number of entries.
    /// </summary>
    /// <value>
    ///     The default returns <c>{count} of</c> followed by the maximum, such as <c>{count} of
    ///     8</c>.
    /// </value>
    public Func<DictionaryEditorLabelContext, string> CountText { get; set; } =
        context => $"{{count}} of {context.MaxItems}";

    /// <summary>
    ///     Gets or sets the callback that generates the error shown when a key is already used,
    ///     with <c>{value}</c> for the key.
    /// </summary>
    /// <value>
    ///     The default returns <c>{value} is already used.</c>.
    /// </value>
    public Func<DictionaryEditorLabelContext, string> DuplicateText { get; set; } =
        context => "{value} is already used.";

    /// <summary>
    ///     Gets or sets the callback that generates the text shown when the dictionary has no
    ///     entries.
    /// </summary>
    /// <value>
    ///     The default returns <c>No items yet</c>.
    /// </value>
    public Func<DictionaryEditorLabelContext, string> EmptyText { get; set; } =
        context => "No items yet";

    /// <summary>
    ///     Gets or sets the callback that generates the text shown when the dictionary is full, and
    ///     the error when it has too many entries.
    /// </summary>
    /// <value>
    ///     The default returns <c>You can add up to 8 items.</c> for a maximum of 8.
    /// </value>
    public Func<DictionaryEditorLabelContext, string> FullText { get; set; } =
        context =>
            context.MaxItems == 1
                ? "You can add only one item."
                : $"You can add up to {context.MaxItems} items.";

    /// <summary>
    ///     Gets or sets the callback that generates the label of the key column.
    /// </summary>
    /// <value>
    ///     The default returns <c>Key</c>.
    /// </value>
    public Func<DictionaryEditorLabelContext, string> KeyLabel { get; set; } = context => "Key";

    /// <summary>
    ///     Gets or sets the callback that generates the error shown on the server when a text key
    ///     or value is too long.
    /// </summary>
    /// <value>
    ///     The default returns <c>Use 120 characters or fewer.</c> for a maximum length of 120.
    /// </value>
    public Func<DictionaryEditorLabelContext, string> MaxLengthText { get; set; } =
        context => $"Use {context.MaxLength} characters or fewer.";

    /// <summary>
    ///     Gets or sets the callback that generates the text a read-only dictionary shows when it
    ///     has no entries.
    /// </summary>
    /// <value>
    ///     The default returns <c>None</c>.
    /// </value>
    public Func<DictionaryEditorLabelContext, string> NoneText { get; set; } = context => "None";

    /// <summary>
    ///     Gets or sets the callback that generates the error shown on the server when a text key
    ///     or value doesn't match its pattern and has no pattern message.
    /// </summary>
    /// <value>
    ///     The default returns <c>Match the requested format.</c>.
    /// </value>
    public Func<DictionaryEditorLabelContext, string> PatternText { get; set; } =
        context => "Match the requested format.";

    /// <summary>
    ///     Gets or sets the callback that generates the error shown on the server when a number
    ///     value is out of range.
    /// </summary>
    /// <value>
    ///     The default returns <c>Enter a number from 0 to 50.</c> for a range from 0 to 50.
    /// </value>
    public Func<DictionaryEditorLabelContext, string> RangeText { get; set; } =
        context =>
            (context.Min, context.Max) switch
            {
                ({ } min, { } max) => $"Enter a number from {min} to {max}.",
                ({ } min, null) => $"Enter {min} or more.",
                _ => $"Enter {context.Max} or less.",
            };

    /// <summary>
    ///     Gets or sets the callback that generates the screen reader label of an entry's remove
    ///     button, with <c>{n}</c> for its number.
    /// </summary>
    /// <value>
    ///     The default returns <c>Remove row {n}</c>.
    /// </value>
    public Func<DictionaryEditorLabelContext, string> RemoveLabel { get; set; } =
        context => "Remove row {n}";

    /// <summary>
    ///     Gets or sets the callback that generates the error shown when an entry has a key and no
    ///     value, or a value and no key.
    /// </summary>
    /// <value>
    ///     The default returns the column label followed by <c>is required.</c>, such as
    ///     <c>Language is required.</c>.
    /// </value>
    public Func<DictionaryEditorLabelContext, string> RequiredText { get; set; } =
        context => $"{context.ColumnLabel} is required.";

    /// <summary>
    ///     Gets or sets the callback that generates the error shown when the dictionary has fewer
    ///     entries than its minimum.
    /// </summary>
    /// <value>
    ///     The default returns <c>Add at least 2 items.</c> for a minimum of 2.
    /// </value>
    public Func<DictionaryEditorLabelContext, string> TooFewText { get; set; } =
        context =>
            context.MinItems == 1
                ? "Add at least one item."
                : $"Add at least {context.MinItems} items.";

    /// <summary>
    ///     Gets or sets the callback that generates the label of the value column.
    /// </summary>
    /// <value>
    ///     The default returns <c>Value</c>.
    /// </value>
    public Func<DictionaryEditorLabelContext, string> ValueLabel { get; set; } = context => "Value";
}
