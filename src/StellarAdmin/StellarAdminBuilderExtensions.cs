using System.Text.RegularExpressions;
using Humanizer;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;
using StellarAdmin.Areas.StellarAdmin.Controllers;
using StellarAdmin.Resources;
using StellarAdmin.Resources.Builders;
using StellarAdmin.Resources.Options;
using StellarAdmin.Sidebar;

namespace StellarAdmin;

/// <summary>
///     Provides extension methods that add resources to the admin panel.
/// </summary>
public static partial class StellarAdminBuilderExtensions
{
    extension(StellarAdminBuilder builder)
    {
        /// <summary>
        ///     Adds a resource.
        /// </summary>
        /// <typeparam name="TResource">The type of the resource.</typeparam>
        /// <returns>A builder that configures the resource.</returns>
        /// <remarks>
        ///     The URL slug of the resource is the kebab-case plural of the resource type's name,
        ///     such as <c>order-items</c> for <c>OrderItem</c>.
        /// </remarks>
        public ResourceBuilder<TResource> AddResource<TResource>() =>
            builder.AddResource<TResource>(DefaultSlug<TResource>());

        /// <summary>
        ///     Adds a resource under a URL slug.
        /// </summary>
        /// <typeparam name="TResource">The type of the resource.</typeparam>
        /// <param name="slug">
        ///     The URL segment of the resource. It uses lowercase letters and digits, optionally
        ///     separated by single hyphens.
        /// </param>
        /// <returns>A builder that configures the resource.</returns>
        /// <exception cref="ArgumentNullException">
        ///     <paramref name="slug" /> is <see langword="null" />.
        /// </exception>
        /// <exception cref="ArgumentException">
        ///     <paramref name="slug" /> is empty, white space or not a valid slug.
        /// </exception>
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
                            && paging.PageSizes is { Count: > 0 }
                            && paging.PageSizes.All(size => size > 0)
                            && paging.PageSizes.Distinct().Count() == paging.PageSizes.Count
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
                            column.Sortable && column.SortField == sort.Field
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
                    options => options.View is null || options.View.Fields.Count > 0,
                    "The view page requires at least one field."
                )
                .Validate(
                    options =>
                        options.View is null
                        || options.ViewLoader is not null
                        || options.DataSourceType is { } type
                            && typeof(IResourceViewHandler<TResource>).IsAssignableFrom(type),
                    "The view page requires a data source implementing IResourceViewHandler or an explicit view handler."
                )
                .Validate(
                    options =>
                        options.Delete is null
                        || options.DataSourceType is { } type
                            && typeof(IResourceDeleteHandler<TResource>).IsAssignableFrom(type),
                    "Delete configuration requires a data source implementing IResourceDeleteHandler."
                )
                .ValidateOnStart();
            builder.Services.TryAddEnumerable(
                ServiceDescriptor.Singleton<
                    IValidateOptions<ResourceOptions<TResource>>,
                    InlineRowOptionsValidator<TResource>
                >()
            );
            builder.Services.TryAddEnumerable(
                ServiceDescriptor.Singleton<
                    IValidateOptions<ResourceOptions<TResource>>,
                    AddAnotherOptionsValidator<TResource>
                >()
            );
            builder.Services.TryAddEnumerable(
                ServiceDescriptor.Singleton<
                    IValidateOptions<ResourceOptions<TResource>>,
                    ImageEditorOptionsValidator<TResource>
                >()
            );
            builder.Services.TryAddEnumerable(
                ServiceDescriptor.Singleton<
                    IValidateOptions<ResourceOptions<TResource>>,
                    IconEditorOptionsValidator<TResource>
                >()
            );
            builder.Services.TryAddEnumerable(
                ServiceDescriptor.Singleton<
                    IValidateOptions<ResourceOptions<TResource>>,
                    FieldBinderOptionsValidator<TResource>
                >()
            );
            builder.Services.TryAddEnumerable(
                ServiceDescriptor.Singleton<
                    IValidateOptions<ResourceOptions<TResource>>,
                    BadgeDisplayOptionsValidator<TResource>
                >()
            );
            builder.Services.TryAddEnumerable(
                ServiceDescriptor.Singleton<
                    IValidateOptions<ResourceOptions<TResource>>,
                    ValueTextOptionsValidator<TResource>
                >()
            );
            builder.Services.TryAddEnumerable(
                ServiceDescriptor.Singleton<
                    IValidateOptions<ResourceOptions<TResource>>,
                    ComputedColumnOptionsValidator<TResource>
                >()
            );
            builder.Services.TryAddEnumerable(
                ServiceDescriptor.Singleton<
                    IValidateOptions<ResourceOptions<TResource>>,
                    ComputedDisplayFieldOptionsValidator<TResource>
                >()
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
                        builder.Options.NextSidebarPosition(),
                        services =>
                        {
                            var options = services
                                .GetRequiredService<IOptions<ResourceOptions<TResource>>>()
                                .Value;
                            var sidebarItem = options.SidebarItem;

                            return new ResourceSidebarItem(
                                sidebarItem.Label ?? options.PluralLabel,
                                sidebarItem.Group,
                                sidebarItem.Icon,
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
        /// <typeparam name="TResource">The type of the resource.</typeparam>
        /// <param name="configure">A delegate that configures the resource.</param>
        /// <returns>The builder, so that calls can be chained.</returns>
        /// <exception cref="ArgumentNullException">
        ///     <paramref name="configure" /> is <see langword="null" />.
        /// </exception>
        /// <remarks>
        ///     The URL slug of the resource is the kebab-case plural of the resource type's name,
        ///     such as <c>order-items</c> for <c>OrderItem</c>.
        /// </remarks>
        public StellarAdminBuilder AddResource<TResource>(
            Action<ResourceBuilder<TResource>> configure
        )
        {
            ArgumentNullException.ThrowIfNull(builder);
            ArgumentNullException.ThrowIfNull(configure);

            configure(builder.AddResource<TResource>());

            return builder;
        }

        /// <summary>
        ///     Adds and configures a resource under a URL slug.
        /// </summary>
        /// <typeparam name="TResource">The type of the resource.</typeparam>
        /// <param name="slug">
        ///     The URL segment of the resource. It uses lowercase letters and digits, optionally
        ///     separated by single hyphens.
        /// </param>
        /// <param name="configure">A delegate that configures the resource.</param>
        /// <returns>The builder, so that calls can be chained.</returns>
        /// <exception cref="ArgumentNullException">
        ///     <paramref name="slug" /> or <paramref name="configure" /> is
        ///     <see langword="null" />.
        /// </exception>
        /// <exception cref="ArgumentException">
        ///     <paramref name="slug" /> is empty, white space or not a valid slug.
        /// </exception>
        public StellarAdminBuilder AddResource<TResource>(
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
        ///     Configures the default page text for all resources.
        /// </summary>
        /// <param name="configure">A delegate that configures the page text.</param>
        /// <returns>The builder, so that calls can be chained.</returns>
        /// <exception cref="ArgumentNullException">
        ///     <paramref name="configure" /> is <see langword="null" />.
        /// </exception>
        public StellarAdminBuilder ConfigureResourceLabels(Action<ResourceLabelsBuilder> configure)
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
