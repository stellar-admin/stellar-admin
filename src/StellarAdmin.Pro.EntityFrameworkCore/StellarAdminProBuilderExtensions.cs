using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ApplicationModels;
using Microsoft.AspNetCore.Mvc.Authorization;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using StellarAdmin.Pro.Sidebar;

namespace StellarAdmin.Pro.EntityFrameworkCore;

/// <summary>
///     Registers EF Core resource screens.
/// </summary>
public static class StellarAdminProBuilderExtensions
{
    /// <summary>
    ///     Adds CRUD screens for an entity in the application's DbContext.
    /// </summary>
    public static StellarAdminProBuilder AddEfCoreResource<TContext, TEntity>(
        this StellarAdminProBuilder builder,
        string name,
        Action<EfCoreResourceBuilder<TContext, TEntity>> configure
    )
        where TContext : DbContext
        where TEntity : class
    {
        ArgumentNullException.ThrowIfNull(builder);
        ArgumentNullException.ThrowIfNull(configure);

        ArgumentException.ThrowIfNullOrWhiteSpace(name);

        if (!System.Text.RegularExpressions.Regex.IsMatch(name, "^[a-zA-Z][a-zA-Z0-9-]*$"))
        {
            throw new ArgumentException(
                "Resource names must start with a letter and contain only letters, digits, or hyphens.",
                nameof(name)
            );
        }

        if (
            builder.Services.Any(d =>
                d.ServiceType == typeof(EfCoreResourceOptions<TContext, TEntity>)
            )
        )
        {
            throw new InvalidOperationException(
                "This entity and DbContext already have a resource registration."
            );
        }

        var options = new EfCoreResourceOptions<TContext, TEntity>(name);
        configure(new EfCoreResourceBuilder<TContext, TEntity>(options));
        foreach (var reference in options.References)
        {
            foreach (
                var column in options.IndexPage.Columns.Where(column =>
                    column.FieldName == reference.FieldName
                )
            )
            {
                column.DisplayExpression = reference.DisplayExpression;
            }

            if (options.IndexPage.DefaultSort is { } sort && sort.FieldName == reference.FieldName)
            {
                options.IndexPage.DefaultSort =
                    new StellarAdmin.Pro.Resources.Options.DataGridDefaultSortOptions(
                        reference.DisplayExpression,
                        sort.FieldName,
                        sort.Descending
                    );
            }
        }

        builder.Services.AddSingleton(options);

        builder.Services.AddSingleton<ISidebarItemsProvider>(
            new ResourceSidebarProvider(name, () => options.IndexPage.EffectiveTitle)
        );

        var controller = typeof(EfCoreResourceController<TContext, TEntity>);
        builder.AddController(controller, name);

        builder.Services.Configure<MvcOptions>(mvc =>
        {
            mvc.Conventions.Add(
                new ResourceAuthorizationConvention(controller, options.AuthorizationPolicy)
            );
        });

        return builder;
    }

    private sealed class ResourceAuthorizationConvention(Type type, string? policy)
        : IControllerModelConvention
    {
        public void Apply(ControllerModel controller)
        {
            if (controller.ControllerType == type && !string.IsNullOrWhiteSpace(policy))
            {
                controller.Filters.Add(new AuthorizeFilter(policy));
            }
        }
    }

    private sealed class ResourceSidebarProvider(string name, Func<string> title)
        : ISidebarItemsProvider
    {
        public SidebarItem[] GetItems() =>
            [new SidebarActionLinkItem(title(), name, "Index", "StellarAdmin")];
    }
}
