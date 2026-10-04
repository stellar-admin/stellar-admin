using Microsoft.Extensions.DependencyInjection;
using StellarAdmin.Dashboard.Resources.Options;

namespace StellarAdmin.Dashboard.Resources.Builders;

/// <summary>
///     Configures default index page text.
/// </summary>
public sealed class ResourceIndexLabelsBuilder
{
    private readonly IServiceCollection _services;

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

    internal ResourceIndexLabelsBuilder(IServiceCollection services) => _services = services;
}
