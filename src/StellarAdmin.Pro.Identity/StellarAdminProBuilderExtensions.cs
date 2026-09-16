using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using StellarAdmin.Pro.Identity.Areas.StellarAdmin.Controllers;
using StellarAdmin.Pro.Identity.Builders;
using StellarAdmin.Pro.Identity.Infrastructure;
using StellarAdmin.Pro.Identity.Options;
using StellarAdmin.Pro.Identity.Sidebar;
using StellarAdmin.Pro.Sidebar;

namespace StellarAdmin.Pro.Identity;

public static class StellarAdminProBuilderExtensions
{
    extension(StellarAdminProBuilder builder)
    {
        /// <summary>
        ///     Adds the ASP.NET Core Identity user and role management screens to StellarAdmin.
        /// </summary>
        /// <typeparam name="TUser">
        ///     The Identity user type the host registered with <c>AddIdentity</c>.
        /// </typeparam>
        /// <typeparam name="TRole">
        ///     The Identity role type the host registered with <c>AddIdentity</c> or <c>AddRoles</c>.
        /// </typeparam>
        public StellarAdminIdentityBuilder<TUser, TRole, string> AddIdentity<TUser, TRole>()
            where TUser : IdentityUser<string>
            where TRole : IdentityRole<string>
        {
            return AddIdentity<TUser, TRole, string>(builder);
        }

        /// <summary>
        ///     Adds the ASP.NET Core Identity user and role management screens to StellarAdmin.
        /// </summary>
        /// <param name="configuration">
        ///     The configuration delegate used to configure the StellarAdmin Identity services.
        /// </param>
        /// <typeparam name="TUser">
        ///     The Identity user type the host registered with <c>AddIdentity</c>.
        /// </typeparam>
        /// <typeparam name="TRole">
        ///     The Identity role type the host registered with <c>AddIdentity</c> or <c>AddRoles</c>.
        /// </typeparam>
        public StellarAdminProBuilder AddIdentity<TUser, TRole>(
            Action<StellarAdminIdentityBuilder<TUser, TRole, string>> configuration
        )
            where TUser : IdentityUser<string>
            where TRole : IdentityRole<string>
        {
            ArgumentNullException.ThrowIfNull(configuration);

            configuration(builder.AddIdentity<TUser, TRole>());

            return builder;
        }

        /// <summary>
        ///     Adds the ASP.NET Core Identity user and role management screens to StellarAdmin.
        /// </summary>
        /// <typeparam name="TUser">
        ///     The Identity user type the host registered with <c>AddIdentity</c>.
        /// </typeparam>
        /// <typeparam name="TRole">
        ///     The Identity role type the host registered with <c>AddIdentity</c> or <c>AddRoles</c>.
        /// </typeparam>
        /// <typeparam name="TKey">
        ///     The type of the primary key for a user and a role.
        /// </typeparam>
        public StellarAdminIdentityBuilder<TUser, TRole, TKey> AddIdentity<TUser, TRole, TKey>()
            where TUser : IdentityUser<TKey>
            where TRole : IdentityRole<TKey>
            where TKey : IEquatable<TKey>
        {
            ArgumentNullException.ThrowIfNull(builder);

            var (options, usersOptions) = AddIdentityCore<TUser, TKey>(builder);
            var rolesOptions = AddRolesCore<TRole, TKey>(builder, options);

            return new StellarAdminIdentityBuilder<TUser, TRole, TKey>(
                options,
                usersOptions,
                rolesOptions
            );
        }

        /// <summary>
        ///     Adds the ASP.NET Core Identity user and role management screens to StellarAdmin.
        /// </summary>
        /// <param name="configuration">
        ///     The configuration delegate used to configure the StellarAdmin Identity services.
        /// </param>
        /// <typeparam name="TUser">
        ///     The Identity user type the host registered with <c>AddIdentity</c>.
        /// </typeparam>
        /// <typeparam name="TRole">
        ///     The Identity role type the host registered with <c>AddIdentity</c> or <c>AddRoles</c>.
        /// </typeparam>
        /// <typeparam name="TKey">
        ///     The type of the primary key for a user and a role.
        /// </typeparam>
        public StellarAdminProBuilder AddIdentity<TUser, TRole, TKey>(
            Action<StellarAdminIdentityBuilder<TUser, TRole, TKey>> configuration
        )
            where TUser : IdentityUser<TKey>
            where TRole : IdentityRole<TKey>
            where TKey : IEquatable<TKey>
        {
            ArgumentNullException.ThrowIfNull(configuration);

            configuration(builder.AddIdentity<TUser, TRole, TKey>());

            return builder;
        }
    }

    // The registration every AddIdentity overload shares: the MVC wiring, the sidebar
    // provider, the root and users options, and the users resource.
    private static (
        StellarAdminIdentityOptions Options,
        IdentityUsersOptions<TUser, TKey> UsersOptions
    ) AddIdentityCore<TUser, TKey>(StellarAdminProBuilder builder)
        where TUser : IdentityUser<TKey>
        where TKey : IEquatable<TKey>
    {
        // This assembly opts out of MVC's automatic application part discovery (see
        // AssemblyInfo.cs), and MVC's built-in controller discovery ignores open generic
        // types, so the generic controllers are closed over the Identity types and
        // registered explicitly.
        builder.AddController(
            typeof(UsersController<,>).MakeGenericType(typeof(TUser), typeof(TKey)),
            "Users"
        );

        builder.Services.TryAddEnumerable(
            ServiceDescriptor.Singleton<ISidebarItemsProvider, SidebarItemsProvider>()
        );

        var options = GetOrAddOptions<StellarAdminIdentityOptions>(builder.Services);
        var usersOptions = GetOrAddOptions<IdentityUsersOptions<TUser, TKey>>(builder.Services);

        AddResource(
            builder.Services,
            new IdentityResourceRegistration
            {
                ControllerName = "Users",
                IndexTitle = () => usersOptions.IndexPage.EffectiveTitle,
                Order = 0,
                SidebarItem = options.Sidebar.UsersItem,
            }
        );

        return (options, usersOptions);
    }

    // Registers the resource unless one with the same controller name is already registered,
    // so a repeat AddIdentity call does not duplicate the sidebar item.
    private static void AddResource(
        IServiceCollection services,
        IdentityResourceRegistration registration
    )
    {
        var alreadyRegistered = services.Any(descriptor =>
            descriptor.ServiceType == typeof(IdentityResourceRegistration)
            && descriptor.ImplementationInstance is IdentityResourceRegistration existing
            && existing.ControllerName == registration.ControllerName
        );
        if (alreadyRegistered)
        {
            return;
        }

        services.AddSingleton(registration);
    }

    // The registration the role-typed AddIdentity overloads add on top of AddIdentityCore:
    // the roles options, the roles controller, and the roles resource.
    private static IdentityRolesOptions<TRole, TKey> AddRolesCore<TRole, TKey>(
        StellarAdminProBuilder builder,
        StellarAdminIdentityOptions options
    )
        where TRole : IdentityRole<TKey>
        where TKey : IEquatable<TKey>
    {
        // The role management screens need RoleManager<TRole>, which the host only
        // registers when it adds Identity with role support. Failing here turns a
        // confusing run-time DI error into a clear configuration error.
        if (
            builder.Services.All(descriptor => descriptor.ServiceType != typeof(RoleManager<TRole>))
        )
        {
            throw new InvalidOperationException(
                $"No RoleManager<{typeof(TRole).Name}> is registered. Register ASP.NET Core "
                    + $"Identity with role support - for example services.AddIdentity<TUser, "
                    + $"{typeof(TRole).Name}>() or AddDefaultIdentity<TUser>()"
                    + $".AddRoles<{typeof(TRole).Name}>() - before calling AddIdentity."
            );
        }

        builder.AddController(
            typeof(RolesController<,>).MakeGenericType(typeof(TRole), typeof(TKey)),
            "Roles"
        );

        var rolesOptions = GetOrAddOptions<IdentityRolesOptions<TRole, TKey>>(builder.Services);

        AddResource(
            builder.Services,
            new IdentityResourceRegistration
            {
                ControllerName = "Roles",
                IndexTitle = () => rolesOptions.IndexPage.EffectiveTitle,
                Order = 1,
                SidebarItem = options.Sidebar.RolesItem,
            }
        );

        return rolesOptions;
    }

    // The configure delegate runs eagerly inside AddIdentity, so the options are built
    // up-front and registered as an instance singleton rather than through a lazy
    // services.Configure callback. Repeat AddIdentity calls hand back a builder over the
    // already-registered instance, so every call configures the same options.
    private static TOptions GetOrAddOptions<TOptions>(IServiceCollection services)
        where TOptions : class, new()
    {
        if (
            services
                .FirstOrDefault(descriptor => descriptor.ServiceType == typeof(TOptions))
                ?.ImplementationInstance
            is TOptions existing
        )
        {
            return existing;
        }

        var options = new TOptions();
        services.AddSingleton(options);

        return options;
    }
}
