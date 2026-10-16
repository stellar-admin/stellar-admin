using Microsoft.Extensions.DependencyInjection;
using StellarAdmin.Resources.Options;

namespace StellarAdmin.Resources.Builders;

/// <summary>
///     Configures default index page text.
/// </summary>
public sealed class ResourceIndexLabelsBuilder
{
    private readonly IServiceCollection _services;

    /// <summary>
    ///     The callback that generates the default inline row cancel button label.
    /// </summary>
    public Func<ResourceLabelContext, string> CancelLabel
    {
        set
        {
            ArgumentNullException.ThrowIfNull(value);

            _services.Configure<ResourceLabelOptions>(options => options.Index.CancelLabel = value);
        }
    }

    /// <summary>
    ///     The callback that generates the default index create button label.
    /// </summary>
    public Func<ResourceLabelContext, string> CreateLabel
    {
        set
        {
            ArgumentNullException.ThrowIfNull(value);

            _services.Configure<ResourceLabelOptions>(options => options.Index.CreateLabel = value);
        }
    }

    /// <summary>
    ///     The callback that generates the default message of the toast shown when a resource is
    ///     added in a new row.
    /// </summary>
    public Func<ResourceLabelContext, string> CreatedMessage
    {
        set
        {
            ArgumentNullException.ThrowIfNull(value);

            _services.Configure<ResourceLabelOptions>(options =>
                options.Index.CreatedMessage = value
            );
        }
    }

    /// <summary>
    ///     The callback that generates the default index delete button label.
    /// </summary>
    public Func<ResourceLabelContext, string> DeleteLabel
    {
        set
        {
            ArgumentNullException.ThrowIfNull(value);

            _services.Configure<ResourceLabelOptions>(options => options.Index.DeleteLabel = value);
        }
    }

    /// <summary>
    ///     The callback that generates the default index edit link label.
    /// </summary>
    public Func<ResourceLabelContext, string> EditLabel
    {
        set
        {
            ArgumentNullException.ThrowIfNull(value);

            _services.Configure<ResourceLabelOptions>(options => options.Index.EditLabel = value);
        }
    }

    /// <summary>
    ///     The callback that generates the default description of the toast shown when a saved
    ///     inline row isn't on the reloaded page.
    /// </summary>
    public Func<ResourceLabelContext, string> NotShownMessage
    {
        set
        {
            ArgumentNullException.ThrowIfNull(value);

            _services.Configure<ResourceLabelOptions>(options =>
                options.Index.NotShownMessage = value
            );
        }
    }

    /// <summary>
    ///     The callback that generates the default inline row save button label.
    /// </summary>
    public Func<ResourceLabelContext, string> SaveLabel
    {
        set
        {
            ArgumentNullException.ThrowIfNull(value);

            _services.Configure<ResourceLabelOptions>(options => options.Index.SaveLabel = value);
        }
    }

    /// <summary>
    ///     The callback that generates the default message of the toast shown when an edited inline
    ///     row isn't on the reloaded page.
    /// </summary>
    public Func<ResourceLabelContext, string> SavedMessage
    {
        set
        {
            ArgumentNullException.ThrowIfNull(value);

            _services.Configure<ResourceLabelOptions>(options =>
                options.Index.SavedMessage = value
            );
        }
    }

    /// <summary>
    ///     The callback that generates the default index search placeholder.
    /// </summary>
    public Func<ResourceLabelContext, string> SearchPlaceholder
    {
        set
        {
            ArgumentNullException.ThrowIfNull(value);

            _services.Configure<ResourceLabelOptions>(options =>
                options.Index.SearchPlaceholder = value
            );
        }
    }

    /// <summary>
    ///     The callback that generates the default index page title.
    /// </summary>
    public Func<ResourceLabelContext, string> Title
    {
        set
        {
            ArgumentNullException.ThrowIfNull(value);

            _services.Configure<ResourceLabelOptions>(options => options.Index.Title = value);
        }
    }

    /// <summary>
    ///     The callback that generates the default index view link label.
    /// </summary>
    public Func<ResourceLabelContext, string> ViewLabel
    {
        set
        {
            ArgumentNullException.ThrowIfNull(value);

            _services.Configure<ResourceLabelOptions>(options => options.Index.ViewLabel = value);
        }
    }

    internal ResourceIndexLabelsBuilder(IServiceCollection services) => _services = services;
}
