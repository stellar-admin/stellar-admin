using System.Globalization;

namespace StellarAdmin.Dashboard.Resources.Options;

/// <summary>
///     Default page text for resources.
/// </summary>
public sealed class ResourceLabelOptions
{
    /// <summary>
    ///     The callback that generates the default create form submit label.
    /// </summary>
    public Func<ResourceLabelContext, string> CreateSubmitLabel { get; set; } =
        resource => $"Create {resource.SingularLabel}";

    /// <summary>
    ///     The callback that generates the default create page title.
    /// </summary>
    public Func<ResourceLabelContext, string> CreateTitle { get; set; } =
        resource => $"Create {resource.SingularLabel}";

    /// <summary>
    ///     The callback that generates the default delete cancellation label.
    /// </summary>
    public Func<ResourceLabelContext, string> DeleteCancelLabel { get; set; } =
        resource => "Cancel";

    /// <summary>
    ///     The callback that generates the default delete confirmation button label.
    /// </summary>
    public Func<ResourceLabelContext, string> DeleteConfirmLabel { get; set; } =
        resource => $"Delete {resource.SingularLabel}";

    /// <summary>
    ///     The callback that generates the default delete confirmation message.
    /// </summary>
    public Func<ResourceLabelContext, string> DeleteMessage { get; set; } =
        resource => $"Are you sure you want to delete this {resource.SingularLabel}?";

    /// <summary>
    ///     The callback that generates the default delete confirmation title.
    /// </summary>
    public Func<ResourceLabelContext, string> DeleteTitle { get; set; } =
        resource => $"Delete {resource.SingularLabel}";

    /// <summary>
    ///     The callback that generates the default edit form submit label.
    /// </summary>
    public Func<ResourceLabelContext, string> EditSubmitLabel { get; set; } =
        resource => $"Save {resource.SingularLabel}";

    /// <summary>
    ///     The callback that generates the default edit page title.
    /// </summary>
    public Func<ResourceLabelContext, string> EditTitle { get; set; } =
        resource => $"Edit {resource.SingularLabel}";

    /// <summary>
    ///     The callback that generates the default index create button label.
    /// </summary>
    public Func<ResourceLabelContext, string> IndexCreateLabel { get; set; } = resource => "Create";

    /// <summary>
    ///     The callback that generates the default index delete button label.
    /// </summary>
    public Func<ResourceLabelContext, string> IndexDeleteLabel { get; set; } =
        resource => $"Delete {resource.SingularLabel}";

    /// <summary>
    ///     The callback that generates the default index edit link label.
    /// </summary>
    public Func<ResourceLabelContext, string> IndexEditLabel { get; set; } =
        resource => $"Edit {resource.SingularLabel}";

    /// <summary>
    ///     The callback that generates the default index search placeholder.
    /// </summary>
    public Func<ResourceLabelContext, string> IndexSearchPlaceholder { get; set; } =
        resource => $"Search {resource.PluralLabel}...";

    /// <summary>
    ///     The callback that generates the default index page title.
    /// </summary>
    public Func<ResourceLabelContext, string> IndexTitle { get; set; } =
        resource => resource.PluralLabel;

    /// <summary>
    ///     The callback that generates the lookup's change button label.
    /// </summary>
    public Func<LookupLabelContext, string> LookupChangeLabel { get; set; } = context => "Change";

    /// <summary>
    ///     The callback that generates the empty lookup's choose button label.
    /// </summary>
    public Func<LookupLabelContext, string> LookupChooseLabel { get; set; } =
        context => $"Choose {SentenceCase(context.FieldLabel)}";

    /// <summary>
    ///     The callback that generates the lookup's clear button label for screen readers.
    /// </summary>
    public Func<LookupLabelContext, string> LookupClearLabel { get; set; } =
        context => $"Clear {context.FieldLabel}";

    /// <summary>
    ///     The callback that generates the empty lookup's create button label.
    /// </summary>
    public Func<LookupLabelContext, string> LookupCreateLabel { get; set; } = context => "New";

    /// <summary>
    ///     The callback that generates the lookup's failed search description.
    /// </summary>
    public Func<LookupLabelContext, string> LookupErrorDescription { get; set; } =
        context => "The results could not be loaded.";

    /// <summary>
    ///     The callback that generates the lookup's failed search title.
    /// </summary>
    public Func<LookupLabelContext, string> LookupErrorTitle { get; set; } =
        context => "Search failed";

    /// <summary>
    ///     The callback that generates the lookup's load more label.
    /// </summary>
    public Func<LookupLabelContext, string> LookupLoadMoreLabel { get; set; } =
        context => "Load more";

    /// <summary>
    ///     The callback that generates the lookup's message for a search shorter than the minimum length.
    /// </summary>
    public Func<LookupLabelContext, string> LookupMinimumSearchLengthMessage { get; set; } =
        context => $"Type at least {context.MinimumSearchLength} characters to search.";

    /// <summary>
    ///     The callback that generates the read-only lookup's text when nothing is selected.
    /// </summary>
    public Func<LookupLabelContext, string> LookupNoneText { get; set; } = context => "None";

    /// <summary>
    ///     The callback that generates the lookup's no-results description for a search term.
    /// </summary>
    public Func<LookupLabelContext, string> LookupNoResultsDescription { get; set; } =
        context => $"Nothing matches “{context.Term}”.";

    /// <summary>
    ///     The callback that generates the lookup's no-results title.
    /// </summary>
    public Func<LookupLabelContext, string> LookupNoResultsTitle { get; set; } =
        context => "No results found";

    /// <summary>
    ///     The callback that generates the lookup's retry button label after a failed search.
    /// </summary>
    public Func<LookupLabelContext, string> LookupRetryLabel { get; set; } = context => "Try again";

    /// <summary>
    ///     The callback that generates the lookup's search input label for screen readers when it has no placeholder.
    /// </summary>
    public Func<LookupLabelContext, string> LookupSearchLabel { get; set; } = context => "Search";

    // Labels are usually sentence case, so they continue a sentence unless they start with an acronym
    private static string SentenceCase(string label)
    {
        return label is [var first, var second, ..] && char.IsUpper(first) && !char.IsUpper(second)
            ? $"{char.ToLower(first, CultureInfo.CurrentCulture)}{label[1..]}"
            : label;
    }
}
