using System.Text.RegularExpressions;
using Humanizer;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;
using StellarAdmin.Areas.StellarAdmin.Controllers;
using StellarAdmin.Diagnostics;
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
        /// <remarks>
        ///     A slug that isn't valid, or that another resource uses, is a configuration problem,
        ///     and the resource is unavailable. A second slug for a resource that is already added is
        ///     ignored.
        /// </remarks>
        public ResourceBuilder<TResource> AddResource<TResource>(string slug)
        {
            ArgumentNullException.ThrowIfNull(builder);
            ArgumentNullException.ThrowIfNull(slug);

            builder.Services.AddOptions<ResourceLabelOptions>();
            builder.Services.AddOptions<ResourceOptions<TResource>>();
            builder.Services.TryAddTransient<
                IOptionsFactory<ResourceOptions<TResource>>,
                ResourceOptionsFactory<TResource>
            >();

            // A resource that can't have its URL is left out, and its configuration goes nowhere
            if (FindSlugProblem<TResource>(builder, slug) is { } problem)
            {
                builder.Options.Problems.Add(problem);
                if (problem.MakesResourceUnavailable)
                {
                    return new(builder.Services);
                }
            }
            else
            {
                builder.AddController(typeof(ResourceController<TResource>), slug);
            }

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
                                sidebarItem.Visible && !options.IsUnavailable
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
                                is not null,
                        services =>
                            services
                                .GetRequiredService<IOptions<ResourceOptions<TResource>>>()
                                .Value.Problems
                    )
                    {
                        Source = ConfigurationSource.Capture(),
                    }
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
        /// <remarks>
        ///     A slug that isn't valid, or that another resource uses, is a configuration problem,
        ///     and the resource is unavailable. A second slug for a resource that is already added is
        ///     ignored.
        /// </remarks>
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

    // A second slug for the same resource is ignored, and the resource keeps its first. An invalid slug, or one
    // another controller has, makes the resource unavailable.
    private static ConfigurationProblem? FindSlugProblem<TResource>(
        StellarAdminBuilder builder,
        string slug
    )
    {
        var controllerType = typeof(ResourceController<TResource>);
        var conflict = builder.FindControllerConflict(controllerType, slug);
        var resource = typeof(TResource).Name.Split('`')[0];
        if (conflict is { } repeated && repeated.ControllerType == controllerType)
        {
            return new(
                ConfigurationProblemKind.ResourceSlugRepeated,
                $"The resource is added again under the slug '{slug}', but it already has the slug '{repeated.ControllerName}'. Add each resource once.",
                $"The resource keeps the slug '{repeated.ControllerName}'."
            )
            {
                Resource = resource,
            };
        }

        if (!SlugPattern().IsMatch(slug))
        {
            return ConfigurationProblem.Unavailable(
                ConfigurationProblemKind.InvalidSlug,
                $"The slug '{slug}' isn't valid. Use lowercase letters and digits, optionally separated by single hyphens."
            ) with
            {
                Resource = resource,
            };
        }

        return conflict is { } inUse
            ? ConfigurationProblem.Unavailable(
                ConfigurationProblemKind.SlugInUse,
                $"The slug '{slug}' is already used by {DescribeController(inUse.ControllerType)}. Give the resource another slug."
            ) with
            {
                Resource = resource,
            }
            : null;
    }

    private static string DescribeController(Type controllerType) =>
        controllerType.IsGenericType
        && controllerType.GetGenericTypeDefinition() == typeof(ResourceController<>)
            ? $"the {controllerType.GetGenericArguments()[0].Name.Split('`')[0]} resource"
            : controllerType.Name;

    [GeneratedRegex("^[a-z0-9]+(?:-[a-z0-9]+)*$")]
    private static partial Regex SlugPattern();
}
