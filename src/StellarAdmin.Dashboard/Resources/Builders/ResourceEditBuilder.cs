using Microsoft.Extensions.DependencyInjection;
using StellarAdmin.Dashboard.Resources.Options;
using StellarAdmin.TagHelpers;

namespace StellarAdmin.Dashboard.Resources.Builders;

/// <summary>
///     Configures a resource's edit page.
/// </summary>
public sealed class ResourceEditBuilder<TResource>
{
    private readonly IServiceCollection _services;

    /// <summary>
    ///     The layout of form sections.
    /// </summary>
    public FormSectionLayout? SectionLayout
    {
        set =>
            _services.Configure<ResourceOptions<TResource>>(options =>
                options.Edit.SectionLayout = value
            );
    }

    /// <summary>
    ///     The submit button label.
    /// </summary>
    public string? SubmitLabel
    {
        set =>
            _services.Configure<ResourceOptions<TResource>>(options =>
                options.Edit.SubmitLabel = value
            );
    }

    /// <summary>
    ///     The page title.
    /// </summary>
    public string? Title
    {
        set =>
            _services.Configure<ResourceOptions<TResource>>(options => options.Edit.Title = value);
    }

    internal ResourceEditBuilder(IServiceCollection services) => _services = services;

    /// <summary>
    ///     Configures the form fields.
    /// </summary>
    public ResourceEditBuilder<TResource> Fields(Action<ResourceFieldsBuilder<TResource>> configure)
    {
        ArgumentNullException.ThrowIfNull(configure);

        configure(
            new(action =>
                _services.Configure<ResourceOptions<TResource>>(options =>
                    action(options.Edit.Items)
                )
            )
        );

        return this;
    }
}
