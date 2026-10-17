using Microsoft.Extensions.DependencyInjection;
using StellarAdmin.Resources.Options;

namespace StellarAdmin.Resources.Builders;

/// <summary>
///     Configures the default text of lookup editors.
/// </summary>
public sealed class LookupLabelsBuilder
{
    private readonly IServiceCollection _services;

    /// <summary>
    ///     Sets the callback that generates the multi-select lookup's add button label.
    /// </summary>
    /// <value>
    ///     The default returns <c>Add</c> followed by the field label, such as <c>Add tags</c>
    ///     for a field labelled <c>Tags</c>.
    /// </value>
    /// <exception cref="ArgumentNullException">
    ///     The value is <see langword="null" />.
    /// </exception>
    public Func<LookupLabelContext, string> AddLabel
    {
        set
        {
            ArgumentNullException.ThrowIfNull(value);

            _services.Configure<ResourceLabelOptions>(options => options.Lookup.AddLabel = value);
        }
    }

    /// <summary>
    ///     Sets the callback that generates the multi-select lookup sheet's label for showing
    ///     every item.
    /// </summary>
    /// <value>
    ///     The default returns <c>All</c>.
    /// </value>
    /// <exception cref="ArgumentNullException">
    ///     The value is <see langword="null" />.
    /// </exception>
    public Func<LookupLabelContext, string> AllLabel
    {
        set
        {
            ArgumentNullException.ThrowIfNull(value);

            _services.Configure<ResourceLabelOptions>(options => options.Lookup.AllLabel = value);
        }
    }

    /// <summary>
    ///     Sets the callback that generates the lookup's change button label.
    /// </summary>
    /// <value>
    ///     The default returns <c>Change</c>.
    /// </value>
    /// <exception cref="ArgumentNullException">
    ///     The value is <see langword="null" />.
    /// </exception>
    public Func<LookupLabelContext, string> ChangeLabel
    {
        set
        {
            ArgumentNullException.ThrowIfNull(value);

            _services.Configure<ResourceLabelOptions>(options =>
                options.Lookup.ChangeLabel = value
            );
        }
    }

    /// <summary>
    ///     Sets the callback that generates the empty lookup's choose button label.
    /// </summary>
    /// <value>
    ///     The default returns <c>Choose</c> followed by the field label, such as <c>Choose
    ///     country</c> for a field labelled <c>Country</c>.
    /// </value>
    /// <exception cref="ArgumentNullException">
    ///     The value is <see langword="null" />.
    /// </exception>
    public Func<LookupLabelContext, string> ChooseLabel
    {
        set
        {
            ArgumentNullException.ThrowIfNull(value);

            _services.Configure<ResourceLabelOptions>(options =>
                options.Lookup.ChooseLabel = value
            );
        }
    }

    /// <summary>
    ///     Sets the callback that generates the multi-select lookup sheet's clear all button
    ///     label.
    /// </summary>
    /// <value>
    ///     The default returns <c>Clear all</c>.
    /// </value>
    /// <exception cref="ArgumentNullException">
    ///     The value is <see langword="null" />.
    /// </exception>
    public Func<LookupLabelContext, string> ClearAllLabel
    {
        set
        {
            ArgumentNullException.ThrowIfNull(value);

            _services.Configure<ResourceLabelOptions>(options =>
                options.Lookup.ClearAllLabel = value
            );
        }
    }

    /// <summary>
    ///     Sets the callback that generates the lookup's clear button label for screen readers.
    /// </summary>
    /// <value>
    ///     The default returns <c>Clear</c> followed by the field label.
    /// </value>
    /// <exception cref="ArgumentNullException">
    ///     The value is <see langword="null" />.
    /// </exception>
    public Func<LookupLabelContext, string> ClearLabel
    {
        set
        {
            ArgumentNullException.ThrowIfNull(value);

            _services.Configure<ResourceLabelOptions>(options => options.Lookup.ClearLabel = value);
        }
    }

    /// <summary>
    ///     Sets the callback that generates the empty lookup's create button label.
    /// </summary>
    /// <value>
    ///     The default returns <c>New</c>.
    /// </value>
    /// <exception cref="ArgumentNullException">
    ///     The value is <see langword="null" />.
    /// </exception>
    public Func<LookupLabelContext, string> CreateLabel
    {
        set
        {
            ArgumentNullException.ThrowIfNull(value);

            _services.Configure<ResourceLabelOptions>(options =>
                options.Lookup.CreateLabel = value
            );
        }
    }

    /// <summary>
    ///     Sets the callback that generates the multi-select lookup sheet's key hint for
    ///     closing the sheet.
    /// </summary>
    /// <value>
    ///     The default returns <c>done</c>.
    /// </value>
    /// <exception cref="ArgumentNullException">
    ///     The value is <see langword="null" />.
    /// </exception>
    public Func<LookupLabelContext, string> DoneHint
    {
        set
        {
            ArgumentNullException.ThrowIfNull(value);

            _services.Configure<ResourceLabelOptions>(options => options.Lookup.DoneHint = value);
        }
    }

    /// <summary>
    ///     Sets the callback that generates the multi-select lookup sheet's done button label.
    /// </summary>
    /// <value>
    ///     The default returns <c>Done</c>.
    /// </value>
    /// <exception cref="ArgumentNullException">
    ///     The value is <see langword="null" />.
    /// </exception>
    public Func<LookupLabelContext, string> DoneLabel
    {
        set
        {
            ArgumentNullException.ThrowIfNull(value);

            _services.Configure<ResourceLabelOptions>(options => options.Lookup.DoneLabel = value);
        }
    }

    /// <summary>
    ///     Sets the callback that generates the lookup's failed search description.
    /// </summary>
    /// <value>
    ///     The default returns <c>The results could not be loaded.</c>.
    /// </value>
    /// <exception cref="ArgumentNullException">
    ///     The value is <see langword="null" />.
    /// </exception>
    public Func<LookupLabelContext, string> ErrorDescription
    {
        set
        {
            ArgumentNullException.ThrowIfNull(value);

            _services.Configure<ResourceLabelOptions>(options =>
                options.Lookup.ErrorDescription = value
            );
        }
    }

    /// <summary>
    ///     Sets the callback that generates the lookup's failed search title.
    /// </summary>
    /// <value>
    ///     The default returns <c>Search failed</c>.
    /// </value>
    /// <exception cref="ArgumentNullException">
    ///     The value is <see langword="null" />.
    /// </exception>
    public Func<LookupLabelContext, string> ErrorTitle
    {
        set
        {
            ArgumentNullException.ThrowIfNull(value);

            _services.Configure<ResourceLabelOptions>(options => options.Lookup.ErrorTitle = value);
        }
    }

    /// <summary>
    ///     Sets the callback that generates the lookup's load more label.
    /// </summary>
    /// <value>
    ///     The default returns <c>Load more</c>.
    /// </value>
    /// <exception cref="ArgumentNullException">
    ///     The value is <see langword="null" />.
    /// </exception>
    public Func<LookupLabelContext, string> LoadMoreLabel
    {
        set
        {
            ArgumentNullException.ThrowIfNull(value);

            _services.Configure<ResourceLabelOptions>(options =>
                options.Lookup.LoadMoreLabel = value
            );
        }
    }

    /// <summary>
    ///     Sets the callback that generates the lookup's message for a search shorter than the
    ///     minimum length.
    /// </summary>
    /// <value>
    ///     The default returns <c>Type at least 3 characters to search.</c>, with the minimum
    ///     search length.
    /// </value>
    /// <exception cref="ArgumentNullException">
    ///     The value is <see langword="null" />.
    /// </exception>
    public Func<LookupLabelContext, string> MinimumSearchLengthMessage
    {
        set
        {
            ArgumentNullException.ThrowIfNull(value);

            _services.Configure<ResourceLabelOptions>(options =>
                options.Lookup.MinimumSearchLengthMessage = value
            );
        }
    }

    /// <summary>
    ///     Sets the callback that generates the end of a multi-select lookup's summary, for the
    ///     items it doesn't name.
    /// </summary>
    /// <value>
    ///     The default returns <c>and 2 more</c>, with the number of items not named.
    /// </value>
    /// <exception cref="ArgumentNullException">
    ///     The value is <see langword="null" />.
    /// </exception>
    public Func<LookupLabelContext, string> MoreText
    {
        set
        {
            ArgumentNullException.ThrowIfNull(value);

            _services.Configure<ResourceLabelOptions>(options => options.Lookup.MoreText = value);
        }
    }

    /// <summary>
    ///     Sets the callback that generates the multi-select lookup sheet's key hint for
    ///     creating an item.
    /// </summary>
    /// <value>
    ///     The default returns <c>new</c>.
    /// </value>
    /// <exception cref="ArgumentNullException">
    ///     The value is <see langword="null" />.
    /// </exception>
    public Func<LookupLabelContext, string> NewHint
    {
        set
        {
            ArgumentNullException.ThrowIfNull(value);

            _services.Configure<ResourceLabelOptions>(options => options.Lookup.NewHint = value);
        }
    }

    /// <summary>
    ///     Sets the callback that generates the read-only lookup's text when nothing is
    ///     selected.
    /// </summary>
    /// <value>
    ///     The default returns <c>None</c>.
    /// </value>
    /// <exception cref="ArgumentNullException">
    ///     The value is <see langword="null" />.
    /// </exception>
    public Func<LookupLabelContext, string> NoneText
    {
        set
        {
            ArgumentNullException.ThrowIfNull(value);

            _services.Configure<ResourceLabelOptions>(options => options.Lookup.NoneText = value);
        }
    }

    /// <summary>
    ///     Sets the callback that generates the lookup's no-results description for a search
    ///     term.
    /// </summary>
    /// <value>
    ///     The default returns <c>Nothing matches “term”.</c>, with the search term.
    /// </value>
    /// <exception cref="ArgumentNullException">
    ///     The value is <see langword="null" />.
    /// </exception>
    public Func<LookupLabelContext, string> NoResultsDescription
    {
        set
        {
            ArgumentNullException.ThrowIfNull(value);

            _services.Configure<ResourceLabelOptions>(options =>
                options.Lookup.NoResultsDescription = value
            );
        }
    }

    /// <summary>
    ///     Sets the callback that generates the lookup's no-results title.
    /// </summary>
    /// <value>
    ///     The default returns <c>No results found</c>.
    /// </value>
    /// <exception cref="ArgumentNullException">
    ///     The value is <see langword="null" />.
    /// </exception>
    public Func<LookupLabelContext, string> NoResultsTitle
    {
        set
        {
            ArgumentNullException.ThrowIfNull(value);

            _services.Configure<ResourceLabelOptions>(options =>
                options.Lookup.NoResultsTitle = value
            );
        }
    }

    /// <summary>
    ///     Sets the callback that generates the multi-select lookup sheet's title when no
    ///     selected item matches the search.
    /// </summary>
    /// <value>
    ///     The default returns <c>No tags selected</c> for a field labelled <c>Tags</c>, or
    ///     <c>No selected tags match “term”</c> when there is a search term.
    /// </value>
    /// <exception cref="ArgumentNullException">
    ///     The value is <see langword="null" />.
    /// </exception>
    public Func<LookupLabelContext, string> NoSelectedTitle
    {
        set
        {
            ArgumentNullException.ThrowIfNull(value);

            _services.Configure<ResourceLabelOptions>(options =>
                options.Lookup.NoSelectedTitle = value
            );
        }
    }

    /// <summary>
    ///     Sets the callback that generates the multi-select lookup's remove button label for
    ///     screen readers.
    /// </summary>
    /// <value>
    ///     The default returns <c>Remove</c>.
    /// </value>
    /// <exception cref="ArgumentNullException">
    ///     The value is <see langword="null" />.
    /// </exception>
    /// <remarks>
    ///     The item's title follows the label.
    /// </remarks>
    public Func<LookupLabelContext, string> RemoveLabel
    {
        set
        {
            ArgumentNullException.ThrowIfNull(value);

            _services.Configure<ResourceLabelOptions>(options =>
                options.Lookup.RemoveLabel = value
            );
        }
    }

    /// <summary>
    ///     Sets the callback that generates the lookup's retry button label after a failed
    ///     search.
    /// </summary>
    /// <value>
    ///     The default returns <c>Try again</c>.
    /// </value>
    /// <exception cref="ArgumentNullException">
    ///     The value is <see langword="null" />.
    /// </exception>
    public Func<LookupLabelContext, string> RetryLabel
    {
        set
        {
            ArgumentNullException.ThrowIfNull(value);

            _services.Configure<ResourceLabelOptions>(options => options.Lookup.RetryLabel = value);
        }
    }

    /// <summary>
    ///     Sets the callback that generates the lookup's search input label for screen readers
    ///     when it has no placeholder.
    /// </summary>
    /// <value>
    ///     The default returns <c>Search</c>.
    /// </value>
    /// <exception cref="ArgumentNullException">
    ///     The value is <see langword="null" />.
    /// </exception>
    public Func<LookupLabelContext, string> SearchLabel
    {
        set
        {
            ArgumentNullException.ThrowIfNull(value);

            _services.Configure<ResourceLabelOptions>(options =>
                options.Lookup.SearchLabel = value
            );
        }
    }

    /// <summary>
    ///     Sets the callback that generates the multi-select lookup sheet's label for showing
    ///     the selected items.
    /// </summary>
    /// <value>
    ///     The default returns <c>Selected</c>.
    /// </value>
    /// <exception cref="ArgumentNullException">
    ///     The value is <see langword="null" />.
    /// </exception>
    public Func<LookupLabelContext, string> SelectedLabel
    {
        set
        {
            ArgumentNullException.ThrowIfNull(value);

            _services.Configure<ResourceLabelOptions>(options =>
                options.Lookup.SelectedLabel = value
            );
        }
    }

    /// <summary>
    ///     Sets the callback that generates the multi-select lookup sheet's key hint for
    ///     selecting or deselecting an item.
    /// </summary>
    /// <value>
    ///     The default returns <c>toggle</c>.
    /// </value>
    /// <exception cref="ArgumentNullException">
    ///     The value is <see langword="null" />.
    /// </exception>
    public Func<LookupLabelContext, string> ToggleHint
    {
        set
        {
            ArgumentNullException.ThrowIfNull(value);

            _services.Configure<ResourceLabelOptions>(options => options.Lookup.ToggleHint = value);
        }
    }

    /// <summary>
    ///     Sets the callback that generates the screen reader label for the multi-select lookup
    ///     sheet's choice between all items and the selected items.
    /// </summary>
    /// <value>
    ///     The default returns <c>Show</c>.
    /// </value>
    /// <exception cref="ArgumentNullException">
    ///     The value is <see langword="null" />.
    /// </exception>
    public Func<LookupLabelContext, string> ViewLabel
    {
        set
        {
            ArgumentNullException.ThrowIfNull(value);

            _services.Configure<ResourceLabelOptions>(options => options.Lookup.ViewLabel = value);
        }
    }

    internal LookupLabelsBuilder(IServiceCollection services) => _services = services;
}
