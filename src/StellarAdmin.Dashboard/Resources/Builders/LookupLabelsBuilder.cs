using Microsoft.Extensions.DependencyInjection;
using StellarAdmin.Dashboard.Resources.Options;

namespace StellarAdmin.Dashboard.Resources.Builders;

/// <summary>
///     Configures default lookup editor text.
/// </summary>
public sealed class LookupLabelsBuilder
{
    private readonly IServiceCollection _services;

    /// <summary>
    ///     The callback that generates the multi-select lookup's add button label.
    /// </summary>
    public Func<LookupLabelContext, string> AddLabel
    {
        set
        {
            ArgumentNullException.ThrowIfNull(value);

            _services.Configure<ResourceLabelOptions>(options => options.Lookup.AddLabel = value);
        }
    }

    /// <summary>
    ///     The callback that generates the lookup's change button label.
    /// </summary>
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
    ///     The callback that generates the empty lookup's choose button label.
    /// </summary>
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
    ///     The callback that generates the lookup's clear button label for screen readers.
    /// </summary>
    public Func<LookupLabelContext, string> ClearLabel
    {
        set
        {
            ArgumentNullException.ThrowIfNull(value);

            _services.Configure<ResourceLabelOptions>(options => options.Lookup.ClearLabel = value);
        }
    }

    /// <summary>
    ///     The callback that generates the empty lookup's create button label.
    /// </summary>
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
    ///     The callback that generates the lookup's failed search description.
    /// </summary>
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
    ///     The callback that generates the lookup's failed search title.
    /// </summary>
    public Func<LookupLabelContext, string> ErrorTitle
    {
        set
        {
            ArgumentNullException.ThrowIfNull(value);

            _services.Configure<ResourceLabelOptions>(options => options.Lookup.ErrorTitle = value);
        }
    }

    /// <summary>
    ///     The callback that generates the lookup's load more label.
    /// </summary>
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
    ///     The callback that generates the lookup's message for a search shorter than the minimum length.
    /// </summary>
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
    ///     The callback that generates the end of a multi-select lookup's summary, for the items it doesn't name.
    /// </summary>
    public Func<LookupLabelContext, string> MoreText
    {
        set
        {
            ArgumentNullException.ThrowIfNull(value);

            _services.Configure<ResourceLabelOptions>(options => options.Lookup.MoreText = value);
        }
    }

    /// <summary>
    ///     The callback that generates the read-only lookup's text when nothing is selected.
    /// </summary>
    public Func<LookupLabelContext, string> NoneText
    {
        set
        {
            ArgumentNullException.ThrowIfNull(value);

            _services.Configure<ResourceLabelOptions>(options => options.Lookup.NoneText = value);
        }
    }

    /// <summary>
    ///     The callback that generates the lookup's no-results description for a search term.
    /// </summary>
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
    ///     The callback that generates the lookup's no-results title.
    /// </summary>
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
    ///     The callback that generates the multi-select lookup's remove button label for screen readers.
    /// </summary>
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
    ///     The callback that generates the lookup's retry button label after a failed search.
    /// </summary>
    public Func<LookupLabelContext, string> RetryLabel
    {
        set
        {
            ArgumentNullException.ThrowIfNull(value);

            _services.Configure<ResourceLabelOptions>(options => options.Lookup.RetryLabel = value);
        }
    }

    /// <summary>
    ///     The callback that generates the lookup's search input label for screen readers when it has no placeholder.
    /// </summary>
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

    internal LookupLabelsBuilder(IServiceCollection services) => _services = services;
}
