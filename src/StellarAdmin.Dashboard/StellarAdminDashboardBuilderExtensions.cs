using Microsoft.Extensions.DependencyInjection;
using StellarAdmin.Dashboard.Areas.StellarAdmin.Controllers;
using StellarAdmin.Dashboard.Resources;
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
            builder
                .Services.AddOptions<ResourceOptions<TResource>>()
                .Validate(
                    options =>
                        options.Index.Paging is not { } paging
                        || paging.PageSize > 0
                            && paging.PageSizes is { Length: > 0 }
                            && paging.PageSizes.All(size => size > 0)
                            && paging.PageSizes.Distinct().Count() == paging.PageSizes.Length
                            && paging.PageSizes.Contains(paging.PageSize),
                    "Paging requires positive, distinct page sizes including the default page size."
                )
                .Validate(
                    options =>
                        options.Create is null
                        || options.CreateHandler is not null
                        || options.DataSourceType is { } type
                            && typeof(IResourceCreateHandler<TResource>).IsAssignableFrom(type),
                    "The create form requires a data source implementing IResourceCreateHandler or an explicit create handler."
                )
                .Validate(
                    options =>
                        options.Edit is null
                        || options.EditHandler is not null
                        || options.DataSourceType is { } type
                            && typeof(IResourceEditHandler<TResource>).IsAssignableFrom(type),
                    "The edit form requires a data source implementing IResourceEditHandler or an explicit edit handler."
                )
                .Validate(
                    options =>
                        options.Delete is null
                        || options.DataSourceType is { } type
                            && typeof(IResourceDeleteHandler<TResource>).IsAssignableFrom(type),
                    "Delete configuration requires a data source implementing IResourceDeleteHandler."
                );
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
