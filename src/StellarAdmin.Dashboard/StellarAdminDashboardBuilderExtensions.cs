using System.Text.RegularExpressions;
using Humanizer;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;
using StellarAdmin.Dashboard.Areas.StellarAdmin.Controllers;
using StellarAdmin.Dashboard.Resources;
using StellarAdmin.Dashboard.Resources.Builders;
using StellarAdmin.Dashboard.Resources.Options;
using StellarAdmin.Dashboard.Sidebar;

namespace StellarAdmin.Dashboard;

/// <summary>
///     Registers Dashboard resources.
/// </summary>
public static partial class StellarAdminDashboardBuilderExtensions
{
    extension(StellarAdminDashboardBuilder builder)
    {
        /// <summary>
        ///     Adds a resource.
        /// </summary>
        /// <remarks>
        ///     The resource's URL slug defaults to the kebab-case plural of the resource type's
        ///     name, e.g. <c>order-items</c> for <c>OrderItem</c>.
        /// </remarks>
        public ResourceBuilder<TResource> AddResource<TResource>() =>
            builder.AddResource<TResource>(DefaultSlug<TResource>());

        /// <summary>
        ///     Adds a resource under the specified URL slug.
        /// </summary>
        /// <param name="slug">
        ///     The resource's URL segment: lowercase letters and digits, optionally separated by
        ///     single hyphens.
        /// </param>
        public ResourceBuilder<TResource> AddResource<TResource>(string slug)
        {
            ArgumentNullException.ThrowIfNull(builder);
            ValidateSlug(slug);

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
                        options.Index.Scopes is not { } scopes
                        || scopes.Items.All(scope =>
                            !string.IsNullOrWhiteSpace(scope.Id)
                            && !string.IsNullOrWhiteSpace(scope.Title)
                        )
                            && scopes
                                .Items.Select(scope => scope.Id)
                                .Distinct(StringComparer.OrdinalIgnoreCase)
                                .Count() == scopes.Items.Count
                            && (
                                scopes.DefaultScope is null
                                || scopes.Items.Any(scope => scope.Id == scopes.DefaultScope)
                            ),
                    "Scopes require distinct identifiers, nonblank labels, and a default scope matching a configured identifier."
                )
                .Validate(
                    options =>
                        options.Index.DefaultSort is not { } sort
                        || options.Index.Columns.Any(column =>
                            column.Sortable && column.FieldName == sort.Field
                        ),
                    "The default sort must select a configured sortable column."
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
            builder.AddController(typeof(ResourceController<TResource>), slug);

            builder.Services.TryAddEnumerable(
                ServiceDescriptor.Singleton<ISidebarItemsProvider, ResourceSidebarItemsProvider>()
            );
            if (
                !builder.Services.Any(descriptor =>
                    descriptor.ServiceType == typeof(ResourceRegistration)
                    && descriptor.ImplementationInstance is ResourceRegistration registration
                    && registration.ControllerType == typeof(ResourceController<TResource>)
                )
            )
            {
                builder.Services.AddSingleton(
                    new ResourceRegistration(
                        typeof(TResource),
                        typeof(ResourceController<TResource>),
                        slug,
                        services =>
                        {
                            var options = services
                                .GetRequiredService<IOptions<ResourceOptions<TResource>>>()
                                .Value;
                            var sidebarItem = options.SidebarItem;

                            return new ResourceSidebarItem(
                                sidebarItem.Label ?? options.PluralLabel,
                                sidebarItem.Group,
                                sidebarItem.Order,
                                sidebarItem.Visible
                            );
                        },
                        services =>
                            services
                                .GetRequiredService<
                                    IOptions<ResourceAuthorizationOptions<TResource>>
                                >()
                                .Value.Metadata,
                        services =>
                            services
                                .GetRequiredService<IOptions<ResourceOptions<TResource>>>()
                                .Value.Create
                                is not null
                    )
                );
            }

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
        ///     Adds and configures a resource under the specified URL slug.
        /// </summary>
        /// <param name="slug">
        ///     The resource's URL segment: lowercase letters and digits, optionally separated by
        ///     single hyphens.
        /// </param>
        /// <param name="configure">Configures the resource.</param>
        public StellarAdminDashboardBuilder AddResource<TResource>(
            string slug,
            Action<ResourceBuilder<TResource>> configure
        )
        {
            ArgumentNullException.ThrowIfNull(builder);
            ArgumentNullException.ThrowIfNull(configure);

            configure(builder.AddResource<TResource>(slug));

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

    private static string DefaultSlug<TResource>() =>
        typeof(TResource).Name.Split('`')[0].Kebaberize().Pluralize();

    private static void ValidateSlug(string slug)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(slug);
        if (!SlugPattern().IsMatch(slug))
        {
            throw new ArgumentException(
                "Use lowercase letters and digits, optionally separated by single hyphens.",
                nameof(slug)
            );
        }
    }

    [GeneratedRegex("^[a-z0-9]+(?:-[a-z0-9]+)*$")]
    private static partial Regex SlugPattern();
}
