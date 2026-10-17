using System.Globalization;

namespace StellarAdmin.Resources.Options;

/// <summary>
///     Stores the default text of lookup editors.
/// </summary>
public sealed class LookupLabelOptions
{
    /// <summary>
    ///     Gets or sets the callback that generates the multi-select lookup's add button label.
    /// </summary>
    /// <value>
    ///     The default returns <c>Add</c> followed by the field label, such as <c>Add tags</c>
    ///     for a field labelled <c>Tags</c>.
    /// </value>
    public Func<LookupLabelContext, string> AddLabel { get; set; } =
        context => $"Add {SentenceCase(context.FieldLabel)}";

    /// <summary>
    ///     Gets or sets the callback that generates the multi-select lookup sheet's label for
    ///     showing every item.
    /// </summary>
    /// <value>
    ///     The default returns <c>All</c>.
    /// </value>
    public Func<LookupLabelContext, string> AllLabel { get; set; } = context => "All";

    /// <summary>
    ///     Gets or sets the callback that generates the lookup's change button label.
    /// </summary>
    /// <value>
    ///     The default returns <c>Change</c>.
    /// </value>
    public Func<LookupLabelContext, string> ChangeLabel { get; set; } = context => "Change";

    /// <summary>
    ///     Gets or sets the callback that generates the empty lookup's choose button label.
    /// </summary>
    /// <value>
    ///     The default returns <c>Choose</c> followed by the field label, such as <c>Choose
    ///     country</c> for a field labelled <c>Country</c>.
    /// </value>
    public Func<LookupLabelContext, string> ChooseLabel { get; set; } =
        context => $"Choose {SentenceCase(context.FieldLabel)}";

    /// <summary>
    ///     Gets or sets the callback that generates the multi-select lookup sheet's clear all
    ///     button label.
    /// </summary>
    /// <value>
    ///     The default returns <c>Clear all</c>.
    /// </value>
    public Func<LookupLabelContext, string> ClearAllLabel { get; set; } = context => "Clear all";

    /// <summary>
    ///     Gets or sets the callback that generates the lookup's clear button label for screen
    ///     readers.
    /// </summary>
    /// <value>
    ///     The default returns <c>Clear</c> followed by the field label.
    /// </value>
    public Func<LookupLabelContext, string> ClearLabel { get; set; } =
        context => $"Clear {context.FieldLabel}";

    /// <summary>
    ///     Gets or sets the callback that generates the empty lookup's create button label.
    /// </summary>
    /// <value>
    ///     The default returns <c>New</c>.
    /// </value>
    public Func<LookupLabelContext, string> CreateLabel { get; set; } = context => "New";

    /// <summary>
    ///     Gets or sets the callback that generates the multi-select lookup sheet's key hint for
    ///     closing the sheet.
    /// </summary>
    /// <value>
    ///     The default returns <c>done</c>.
    /// </value>
    public Func<LookupLabelContext, string> DoneHint { get; set; } = context => "done";

    /// <summary>
    ///     Gets or sets the callback that generates the multi-select lookup sheet's done button
    ///     label.
    /// </summary>
    /// <value>
    ///     The default returns <c>Done</c>.
    /// </value>
    public Func<LookupLabelContext, string> DoneLabel { get; set; } = context => "Done";

    /// <summary>
    ///     Gets or sets the callback that generates the lookup's failed search description.
    /// </summary>
    /// <value>
    ///     The default returns <c>The results could not be loaded.</c>.
    /// </value>
    public Func<LookupLabelContext, string> ErrorDescription { get; set; } =
        context => "The results could not be loaded.";

    /// <summary>
    ///     Gets or sets the callback that generates the lookup's failed search title.
    /// </summary>
    /// <value>
    ///     The default returns <c>Search failed</c>.
    /// </value>
    public Func<LookupLabelContext, string> ErrorTitle { get; set; } = context => "Search failed";

    /// <summary>
    ///     Gets or sets the callback that generates the lookup's load more label.
    /// </summary>
    /// <value>
    ///     The default returns <c>Load more</c>.
    /// </value>
    public Func<LookupLabelContext, string> LoadMoreLabel { get; set; } = context => "Load more";

    /// <summary>
    ///     Gets or sets the callback that generates the lookup's message for a search shorter than
    ///     the minimum length.
    /// </summary>
    /// <value>
    ///     The default returns <c>Type at least 3 characters to search.</c>, with the minimum
    ///     search length.
    /// </value>
    public Func<LookupLabelContext, string> MinimumSearchLengthMessage { get; set; } =
        context => $"Type at least {context.MinimumSearchLength} characters to search.";

    /// <summary>
    ///     Gets or sets the callback that generates the end of a multi-select lookup's summary, for
    ///     the items it doesn't name.
    /// </summary>
    /// <value>
    ///     The default returns <c>and 2 more</c>, with the number of items not named.
    /// </value>
    public Func<LookupLabelContext, string> MoreText { get; set; } =
        context => $"and {context.Count} more";

    /// <summary>
    ///     Gets or sets the callback that generates the multi-select lookup sheet's key hint for
    ///     creating an item.
    /// </summary>
    /// <value>
    ///     The default returns <c>new</c>.
    /// </value>
    public Func<LookupLabelContext, string> NewHint { get; set; } = context => "new";

    /// <summary>
    ///     Gets or sets the callback that generates the read-only lookup's text when nothing is
    ///     selected.
    /// </summary>
    /// <value>
    ///     The default returns <c>None</c>.
    /// </value>
    public Func<LookupLabelContext, string> NoneText { get; set; } = context => "None";

    /// <summary>
    ///     Gets or sets the callback that generates the lookup's no-results description for a
    ///     search term.
    /// </summary>
    /// <value>
    ///     The default returns <c>Nothing matches “term”.</c>, with the search term.
    /// </value>
    public Func<LookupLabelContext, string> NoResultsDescription { get; set; } =
        context => $"Nothing matches “{context.Term}”.";

    /// <summary>
    ///     Gets or sets the callback that generates the lookup's no-results title.
    /// </summary>
    /// <value>
    ///     The default returns <c>No results found</c>.
    /// </value>
    public Func<LookupLabelContext, string> NoResultsTitle { get; set; } =
        context => "No results found";

    /// <summary>
    ///     Gets or sets the callback that generates the multi-select lookup sheet's title when no
    ///     selected item matches the search.
    /// </summary>
    /// <value>
    ///     The default returns <c>No tags selected</c> for a field labelled <c>Tags</c>, or
    ///     <c>No selected tags match “term”</c> when there is a search term.
    /// </value>
    public Func<LookupLabelContext, string> NoSelectedTitle { get; set; } =
        context =>
            context.Term is null
                ? $"No {SentenceCase(context.FieldLabel)} selected"
                : $"No selected {SentenceCase(context.FieldLabel)} match “{context.Term}”";

    /// <summary>
    ///     Gets or sets the callback that generates the multi-select lookup's remove button label
    ///     for screen readers.
    /// </summary>
    /// <value>
    ///     The default returns <c>Remove</c>.
    /// </value>
    /// <remarks>
    ///     The item's title follows the label.
    /// </remarks>
    public Func<LookupLabelContext, string> RemoveLabel { get; set; } = context => "Remove";

    /// <summary>
    ///     Gets or sets the callback that generates the lookup's retry button label after a failed
    ///     search.
    /// </summary>
    /// <value>
    ///     The default returns <c>Try again</c>.
    /// </value>
    public Func<LookupLabelContext, string> RetryLabel { get; set; } = context => "Try again";

    /// <summary>
    ///     Gets or sets the callback that generates the lookup's search input label for screen
    ///     readers when it has no placeholder.
    /// </summary>
    /// <value>
    ///     The default returns <c>Search</c>.
    /// </value>
    public Func<LookupLabelContext, string> SearchLabel { get; set; } = context => "Search";

    /// <summary>
    ///     Gets or sets the callback that generates the multi-select lookup sheet's label for
    ///     showing the selected items.
    /// </summary>
    /// <value>
    ///     The default returns <c>Selected</c>.
    /// </value>
    public Func<LookupLabelContext, string> SelectedLabel { get; set; } = context => "Selected";

    /// <summary>
    ///     Gets or sets the callback that generates the multi-select lookup sheet's key hint for
    ///     selecting or deselecting an item.
    /// </summary>
    /// <value>
    ///     The default returns <c>toggle</c>.
    /// </value>
    public Func<LookupLabelContext, string> ToggleHint { get; set; } = context => "toggle";

    /// <summary>
    ///     Gets or sets the callback that generates the screen reader label for the multi-select
    ///     lookup sheet's choice between all items and the selected items.
    /// </summary>
    /// <value>
    ///     The default returns <c>Show</c>.
    /// </value>
    public Func<LookupLabelContext, string> ViewLabel { get; set; } = context => "Show";

    // Labels are usually sentence case, so they continue a sentence unless they start with an acronym
    private static string SentenceCase(string label)
    {
        return label is [var first, var second, ..] && char.IsUpper(first) && !char.IsUpper(second)
            ? $"{char.ToLower(first, CultureInfo.CurrentCulture)}{label[1..]}"
            : label;
    }
}
