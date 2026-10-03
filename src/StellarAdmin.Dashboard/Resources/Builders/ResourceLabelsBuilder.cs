using Microsoft.Extensions.DependencyInjection;
using StellarAdmin.Dashboard.Resources.Options;

namespace StellarAdmin.Dashboard.Resources.Builders;

/// <summary>
///     Configures default page text for resources.
/// </summary>
public sealed class ResourceLabelsBuilder
{
    private readonly IServiceCollection _services;

    /// <summary>
    ///     The callback that generates the default create form submit label.
    /// </summary>
    public Func<ResourceLabelContext, string> CreateSubmitLabel
    {
        set
        {
            ArgumentNullException.ThrowIfNull(value);

            _services.Configure<ResourceLabelOptions>(options => options.CreateSubmitLabel = value);
        }
    }

    /// <summary>
    ///     The callback that generates the default create page title.
    /// </summary>
    public Func<ResourceLabelContext, string> CreateTitle
    {
        set
        {
            ArgumentNullException.ThrowIfNull(value);

            _services.Configure<ResourceLabelOptions>(options => options.CreateTitle = value);
        }
    }

    /// <summary>
    ///     The callback that generates the default delete cancellation label.
    /// </summary>
    public Func<ResourceLabelContext, string> DeleteCancelLabel
    {
        set
        {
            ArgumentNullException.ThrowIfNull(value);

            _services.Configure<ResourceLabelOptions>(options => options.DeleteCancelLabel = value);
        }
    }

    /// <summary>
    ///     The callback that generates the default delete confirmation button label.
    /// </summary>
    public Func<ResourceLabelContext, string> DeleteConfirmLabel
    {
        set
        {
            ArgumentNullException.ThrowIfNull(value);

            _services.Configure<ResourceLabelOptions>(options =>
                options.DeleteConfirmLabel = value
            );
        }
    }

    /// <summary>
    ///     The callback that generates the default delete confirmation message.
    /// </summary>
    public Func<ResourceLabelContext, string> DeleteMessage
    {
        set
        {
            ArgumentNullException.ThrowIfNull(value);

            _services.Configure<ResourceLabelOptions>(options => options.DeleteMessage = value);
        }
    }

    /// <summary>
    ///     The callback that generates the default delete confirmation title.
    /// </summary>
    public Func<ResourceLabelContext, string> DeleteTitle
    {
        set
        {
            ArgumentNullException.ThrowIfNull(value);

            _services.Configure<ResourceLabelOptions>(options => options.DeleteTitle = value);
        }
    }

    /// <summary>
    ///     The callback that generates the default edit form submit label.
    /// </summary>
    public Func<ResourceLabelContext, string> EditSubmitLabel
    {
        set
        {
            ArgumentNullException.ThrowIfNull(value);

            _services.Configure<ResourceLabelOptions>(options => options.EditSubmitLabel = value);
        }
    }

    /// <summary>
    ///     The callback that generates the default edit page title.
    /// </summary>
    public Func<ResourceLabelContext, string> EditTitle
    {
        set
        {
            ArgumentNullException.ThrowIfNull(value);

            _services.Configure<ResourceLabelOptions>(options => options.EditTitle = value);
        }
    }

    /// <summary>
    ///     The callback that generates the default index create button label.
    /// </summary>
    public Func<ResourceLabelContext, string> IndexCreateLabel
    {
        set
        {
            ArgumentNullException.ThrowIfNull(value);

            _services.Configure<ResourceLabelOptions>(options => options.IndexCreateLabel = value);
        }
    }

    /// <summary>
    ///     The callback that generates the default index delete button label.
    /// </summary>
    public Func<ResourceLabelContext, string> IndexDeleteLabel
    {
        set
        {
            ArgumentNullException.ThrowIfNull(value);

            _services.Configure<ResourceLabelOptions>(options => options.IndexDeleteLabel = value);
        }
    }

    /// <summary>
    ///     The callback that generates the default index edit link label.
    /// </summary>
    public Func<ResourceLabelContext, string> IndexEditLabel
    {
        set
        {
            ArgumentNullException.ThrowIfNull(value);

            _services.Configure<ResourceLabelOptions>(options => options.IndexEditLabel = value);
        }
    }

    /// <summary>
    ///     The callback that generates the default index search placeholder.
    /// </summary>
    public Func<ResourceLabelContext, string> IndexSearchPlaceholder
    {
        set
        {
            ArgumentNullException.ThrowIfNull(value);

            _services.Configure<ResourceLabelOptions>(options =>
                options.IndexSearchPlaceholder = value
            );
        }
    }

    /// <summary>
    ///     The callback that generates the default index page title.
    /// </summary>
    public Func<ResourceLabelContext, string> IndexTitle
    {
        set
        {
            ArgumentNullException.ThrowIfNull(value);

            _services.Configure<ResourceLabelOptions>(options => options.IndexTitle = value);
        }
    }

    /// <summary>
    ///     The callback that generates the lookup's change button label.
    /// </summary>
    public Func<LookupLabelContext, string> LookupChangeLabel
    {
        set
        {
            ArgumentNullException.ThrowIfNull(value);

            _services.Configure<ResourceLabelOptions>(options => options.LookupChangeLabel = value);
        }
    }

    /// <summary>
    ///     The callback that generates the empty lookup's choose button label.
    /// </summary>
    public Func<LookupLabelContext, string> LookupChooseLabel
    {
        set
        {
            ArgumentNullException.ThrowIfNull(value);

            _services.Configure<ResourceLabelOptions>(options => options.LookupChooseLabel = value);
        }
    }

    /// <summary>
    ///     The callback that generates the lookup's clear button label for screen readers.
    /// </summary>
    public Func<LookupLabelContext, string> LookupClearLabel
    {
        set
        {
            ArgumentNullException.ThrowIfNull(value);

            _services.Configure<ResourceLabelOptions>(options => options.LookupClearLabel = value);
        }
    }

    /// <summary>
    ///     The callback that generates the empty lookup's create button label.
    /// </summary>
    public Func<LookupLabelContext, string> LookupCreateLabel
    {
        set
        {
            ArgumentNullException.ThrowIfNull(value);

            _services.Configure<ResourceLabelOptions>(options => options.LookupCreateLabel = value);
        }
    }

    /// <summary>
    ///     The callback that generates the lookup's failed search description.
    /// </summary>
    public Func<LookupLabelContext, string> LookupErrorDescription
    {
        set
        {
            ArgumentNullException.ThrowIfNull(value);

            _services.Configure<ResourceLabelOptions>(options =>
                options.LookupErrorDescription = value
            );
        }
    }

    /// <summary>
    ///     The callback that generates the lookup's failed search title.
    /// </summary>
    public Func<LookupLabelContext, string> LookupErrorTitle
    {
        set
        {
            ArgumentNullException.ThrowIfNull(value);

            _services.Configure<ResourceLabelOptions>(options => options.LookupErrorTitle = value);
        }
    }

    /// <summary>
    ///     The callback that generates the lookup's load more label.
    /// </summary>
    public Func<LookupLabelContext, string> LookupLoadMoreLabel
    {
        set
        {
            ArgumentNullException.ThrowIfNull(value);

            _services.Configure<ResourceLabelOptions>(options =>
                options.LookupLoadMoreLabel = value
            );
        }
    }

    /// <summary>
    ///     The callback that generates the lookup's message for a search shorter than the minimum length.
    /// </summary>
    public Func<LookupLabelContext, string> LookupMinimumSearchLengthMessage
    {
        set
        {
            ArgumentNullException.ThrowIfNull(value);

            _services.Configure<ResourceLabelOptions>(options =>
                options.LookupMinimumSearchLengthMessage = value
            );
        }
    }

    /// <summary>
    ///     The callback that generates the read-only lookup's text when nothing is selected.
    /// </summary>
    public Func<LookupLabelContext, string> LookupNoneText
    {
        set
        {
            ArgumentNullException.ThrowIfNull(value);

            _services.Configure<ResourceLabelOptions>(options => options.LookupNoneText = value);
        }
    }

    /// <summary>
    ///     The callback that generates the lookup's no-results description for a search term.
    /// </summary>
    public Func<LookupLabelContext, string> LookupNoResultsDescription
    {
        set
        {
            ArgumentNullException.ThrowIfNull(value);

            _services.Configure<ResourceLabelOptions>(options =>
                options.LookupNoResultsDescription = value
            );
        }
    }

    /// <summary>
    ///     The callback that generates the lookup's no-results title.
    /// </summary>
    public Func<LookupLabelContext, string> LookupNoResultsTitle
    {
        set
        {
            ArgumentNullException.ThrowIfNull(value);

            _services.Configure<ResourceLabelOptions>(options =>
                options.LookupNoResultsTitle = value
            );
        }
    }

    /// <summary>
    ///     The callback that generates the lookup's retry button label after a failed search.
    /// </summary>
    public Func<LookupLabelContext, string> LookupRetryLabel
    {
        set
        {
            ArgumentNullException.ThrowIfNull(value);

            _services.Configure<ResourceLabelOptions>(options => options.LookupRetryLabel = value);
        }
    }

    /// <summary>
    ///     The callback that generates the lookup's search input label for screen readers when it has no placeholder.
    /// </summary>
    public Func<LookupLabelContext, string> LookupSearchLabel
    {
        set
        {
            ArgumentNullException.ThrowIfNull(value);

            _services.Configure<ResourceLabelOptions>(options => options.LookupSearchLabel = value);
        }
    }

    internal ResourceLabelsBuilder(IServiceCollection services) => _services = services;
}
