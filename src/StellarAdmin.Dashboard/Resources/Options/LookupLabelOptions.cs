using System.Globalization;

namespace StellarAdmin.Dashboard.Resources.Options;

/// <summary>
///     Default lookup editor text.
/// </summary>
public sealed class LookupLabelOptions
{
    /// <summary>
    ///     The callback that generates the multi-select lookup's add button label.
    /// </summary>
    public Func<LookupLabelContext, string> AddLabel { get; set; } =
        context => $"Add {SentenceCase(context.FieldLabel)}";

    /// <summary>
    ///     The callback that generates the multi-select lookup sheet's label for showing every item.
    /// </summary>
    public Func<LookupLabelContext, string> AllLabel { get; set; } = context => "All";

    /// <summary>
    ///     The callback that generates the lookup's change button label.
    /// </summary>
    public Func<LookupLabelContext, string> ChangeLabel { get; set; } = context => "Change";

    /// <summary>
    ///     The callback that generates the empty lookup's choose button label.
    /// </summary>
    public Func<LookupLabelContext, string> ChooseLabel { get; set; } =
        context => $"Choose {SentenceCase(context.FieldLabel)}";

    /// <summary>
    ///     The callback that generates the multi-select lookup sheet's clear all button label.
    /// </summary>
    public Func<LookupLabelContext, string> ClearAllLabel { get; set; } = context => "Clear all";

    /// <summary>
    ///     The callback that generates the lookup's clear button label for screen readers.
    /// </summary>
    public Func<LookupLabelContext, string> ClearLabel { get; set; } =
        context => $"Clear {context.FieldLabel}";

    /// <summary>
    ///     The callback that generates the empty lookup's create button label.
    /// </summary>
    public Func<LookupLabelContext, string> CreateLabel { get; set; } = context => "New";

    /// <summary>
    ///     The callback that generates the multi-select lookup sheet's key hint for closing the sheet.
    /// </summary>
    public Func<LookupLabelContext, string> DoneHint { get; set; } = context => "done";

    /// <summary>
    ///     The callback that generates the multi-select lookup sheet's done button label.
    /// </summary>
    public Func<LookupLabelContext, string> DoneLabel { get; set; } = context => "Done";

    /// <summary>
    ///     The callback that generates the lookup's failed search description.
    /// </summary>
    public Func<LookupLabelContext, string> ErrorDescription { get; set; } =
        context => "The results could not be loaded.";

    /// <summary>
    ///     The callback that generates the lookup's failed search title.
    /// </summary>
    public Func<LookupLabelContext, string> ErrorTitle { get; set; } = context => "Search failed";

    /// <summary>
    ///     The callback that generates the lookup's load more label.
    /// </summary>
    public Func<LookupLabelContext, string> LoadMoreLabel { get; set; } = context => "Load more";

    /// <summary>
    ///     The callback that generates the lookup's message for a search shorter than the minimum length.
    /// </summary>
    public Func<LookupLabelContext, string> MinimumSearchLengthMessage { get; set; } =
        context => $"Type at least {context.MinimumSearchLength} characters to search.";

    /// <summary>
    ///     The callback that generates the end of a multi-select lookup's summary, for the
    ///     <see cref="LookupLabelContext.Count" /> items it doesn't name.
    /// </summary>
    public Func<LookupLabelContext, string> MoreText { get; set; } =
        context => $"and {context.Count} more";

    /// <summary>
    ///     The callback that generates the multi-select lookup sheet's key hint for creating an item.
    /// </summary>
    public Func<LookupLabelContext, string> NewHint { get; set; } = context => "new";

    /// <summary>
    ///     The callback that generates the read-only lookup's text when nothing is selected.
    /// </summary>
    public Func<LookupLabelContext, string> NoneText { get; set; } = context => "None";

    /// <summary>
    ///     The callback that generates the lookup's no-results description for a search term.
    /// </summary>
    public Func<LookupLabelContext, string> NoResultsDescription { get; set; } =
        context => $"Nothing matches “{context.Term}”.";

    /// <summary>
    ///     The callback that generates the lookup's no-results title.
    /// </summary>
    public Func<LookupLabelContext, string> NoResultsTitle { get; set; } =
        context => "No results found";

    /// <summary>
    ///     The callback that generates the multi-select lookup sheet's title when no selected item matches the search.
    /// </summary>
    public Func<LookupLabelContext, string> NoSelectedTitle { get; set; } =
        context =>
            context.Term is null
                ? $"No {SentenceCase(context.FieldLabel)} selected"
                : $"No selected {SentenceCase(context.FieldLabel)} match “{context.Term}”";

    /// <summary>
    ///     The callback that generates the multi-select lookup's remove button label for screen readers, which the
    ///     item's title follows.
    /// </summary>
    public Func<LookupLabelContext, string> RemoveLabel { get; set; } = context => "Remove";

    /// <summary>
    ///     The callback that generates the lookup's retry button label after a failed search.
    /// </summary>
    public Func<LookupLabelContext, string> RetryLabel { get; set; } = context => "Try again";

    /// <summary>
    ///     The callback that generates the lookup's search input label for screen readers when it has no placeholder.
    /// </summary>
    public Func<LookupLabelContext, string> SearchLabel { get; set; } = context => "Search";

    /// <summary>
    ///     The callback that generates the multi-select lookup sheet's label for showing the selected items.
    /// </summary>
    public Func<LookupLabelContext, string> SelectedLabel { get; set; } = context => "Selected";

    /// <summary>
    ///     The callback that generates the multi-select lookup sheet's key hint for selecting or deselecting an item.
    /// </summary>
    public Func<LookupLabelContext, string> ToggleHint { get; set; } = context => "toggle";

    /// <summary>
    ///     The callback that generates the multi-select lookup sheet's label for screen readers of the choice between every item and the selected items.
    /// </summary>
    public Func<LookupLabelContext, string> ViewLabel { get; set; } = context => "Show";

    // Labels are usually sentence case, so they continue a sentence unless they start with an acronym
    private static string SentenceCase(string label)
    {
        return label is [var first, var second, ..] && char.IsUpper(first) && !char.IsUpper(second)
            ? $"{char.ToLower(first, CultureInfo.CurrentCulture)}{label[1..]}"
            : label;
    }
}
