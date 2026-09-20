using Microsoft.Extensions.DependencyInjection;
using StellarAdmin.Dashboard.Areas.StellarAdmin.Controllers;
using StellarAdmin.Dashboard.Resources.Builders;
using StellarAdmin.Dashboard.Resources.Options;

namespace StellarAdmin.Dashboard;

/// <summary>
///     Registers Dashboard resources.
/// </summary>
public static class StellarAdminDashboardBuilderExtensions
{
    extension(StellarAdminDashboardBuilder builder)
    {
        /// <summary>
        ///     Adds a resource.
        /// </summary>
        public ResourceBuilder<TResource> AddResource<TResource>()
        {
            ArgumentNullException.ThrowIfNull(builder);

            builder.Services.AddOptions<ResourceLabelOptions>();
            builder.Services.AddOptions<ResourceOptions<TResource>>();
            builder.AddController(
                typeof(ResourceController<TResource>),
                typeof(TResource).Name.Split('`')[0]
            );

            return new(builder.Services);
        }

        /// <summary>
        ///     Adds and configures a resource.
        /// </summary>
        public StellarAdminDashboardBuilder AddResource<TResource>(
            Action<ResourceBuilder<TResource>> configure
        )
        {
            ArgumentNullException.ThrowIfNull(builder);
            ArgumentNullException.ThrowIfNull(configure);

            configure(builder.AddResource<TResource>());

            return builder;
        }

        /// <summary>
        ///     Configures default page text for resources.
        /// </summary>
        public StellarAdminDashboardBuilder ConfigureResourceLabels(
            Action<ResourceLabelsBuilder> configure
        )
        {
            ArgumentNullException.ThrowIfNull(builder);
            ArgumentNullException.ThrowIfNull(configure);

            configure(new(builder.Services));

            return builder;
        }
    }
}
