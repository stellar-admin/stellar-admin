using System.Linq.Expressions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using StellarAdmin.Diagnostics;
using StellarAdmin.Infrastructure.Authorization;
using StellarAdmin.Resources.Options;

namespace StellarAdmin.Resources.Builders;

/// <summary>
///     Provides the configuration methods that every resource builder shares.
/// </summary>
/// <typeparam name="TResource">The type of the resource.</typeparam>
/// <typeparam name="TBuilder">
///     The type of the derived builder, which the methods return.
/// </typeparam>
public abstract class ResourceBuilderBase<TResource, TBuilder>
    where TBuilder : ResourceBuilderBase<TResource, TBuilder>
{
    /// <summary>
    ///     Sets the plural resource label.
    /// </summary>
    /// <value>
    ///     The default is the English plural of the singular label.
    /// </value>
    public string PluralLabel
    {
        set =>
            Services.Configure<ResourceOptions<TResource>>(options => options.PluralLabel = value);
    }

    /// <summary>
    ///     Sets the singular resource label.
    /// </summary>
    /// <value>
    ///     The default is the resource type's name, split into words.
    /// </value>
    public string SingularLabel
    {
        set =>
            Services.Configure<ResourceOptions<TResource>>(options =>
                options.SingularLabel = value
            );
    }

    /// <summary>
    ///     Gets the service collection the resource's configuration is added to.
    /// </summary>
    protected IServiceCollection Services { get; }

    /// <summary>
    ///     Initializes a new instance of the
    ///     <see cref="ResourceBuilderBase{TResource, TBuilder}" /> class.
    /// </summary>
    /// <param name="services">
    ///     The service collection the resource's configuration is added to.
    /// </param>
    protected ResourceBuilderBase(IServiceCollection services) => Services = services;

    /// <summary>
    ///     Enables the create page.
    /// </summary>
    /// <returns>A builder that configures the create page.</returns>
    /// <remarks>
    ///     The resource's data source must implement
    ///     <see cref="IResourceCreateHandler{TResource}" />.
    /// </remarks>
    public ResourceCreateBuilder<TResource> AllowCreate()
    {
        var source = ConfigurationSource.Capture();
        Services.Configure<ResourceOptions<TResource>>(options =>
        {
            options.Create = new ResourceCreateOptions<TResource> { Source = source };
            options.CreateHandler = null;
        });

        return CreateBuilder<TResource>(source);
    }

    /// <summary>
    ///     Enables and configures the create page.
    /// </summary>
    /// <param name="configure">A delegate that configures the create page.</param>
    /// <returns>The builder, so that calls can be chained.</returns>
    /// <exception cref="ArgumentNullException">
    ///     <paramref name="configure" /> is <see langword="null" />.
    /// </exception>
    /// <remarks>
    ///     The resource's data source must implement
    ///     <see cref="IResourceCreateHandler{TResource}" />.
    /// </remarks>
    public TBuilder AllowCreate(Action<ResourceCreateBuilder<TResource>> configure)
    {
        ArgumentNullException.ThrowIfNull(configure);
        configure(AllowCreate());

        return (TBuilder)this;
    }

    /// <summary>
    ///     Enables the create page with its own model and handler.
    /// </summary>
    /// <typeparam name="TModel">The type of the model the create form binds to.</typeparam>
    /// <typeparam name="THandler">
    ///     The type of the handler that creates the resource from the model.
    /// </typeparam>
    /// <returns>A builder that configures the create page.</returns>
    /// <remarks>
    ///     Registers <typeparamref name="THandler" /> as a scoped service if it isn't registered
    ///     already.
    /// </remarks>
    public ResourceCreateBuilder<TModel> AllowCreate<TModel, THandler>()
        where THandler : class, IResourceCreateHandler<TModel>
    {
        Services.TryAddScoped<THandler>();
        var source = ConfigurationSource.Capture();
        Services.Configure<ResourceOptions<TResource>>(options =>
        {
            options.Create = new ResourceCreateOptions<TModel> { Source = source };
            options.CreateHandler = (services, model, cancellationToken) =>
                services
                    .GetRequiredService<THandler>()
                    .CreateAsync((TModel)model, cancellationToken);
        });

        return CreateBuilder<TModel>(source);
    }

    /// <summary>
    ///     Enables and configures the create page with its own model and handler.
    /// </summary>
    /// <typeparam name="TModel">The type of the model the create form binds to.</typeparam>
    /// <typeparam name="THandler">
    ///     The type of the handler that creates the resource from the model.
    /// </typeparam>
    /// <param name="configure">A delegate that configures the create page.</param>
    /// <returns>The builder, so that calls can be chained.</returns>
    /// <exception cref="ArgumentNullException">
    ///     <paramref name="configure" /> is <see langword="null" />.
    /// </exception>
    /// <remarks>
    ///     Registers <typeparamref name="THandler" /> as a scoped service if it isn't registered
    ///     already.
    /// </remarks>
    public TBuilder AllowCreate<TModel, THandler>(Action<ResourceCreateBuilder<TModel>> configure)
        where THandler : class, IResourceCreateHandler<TModel>
    {
        ArgumentNullException.ThrowIfNull(configure);
        configure(AllowCreate<TModel, THandler>());

        return (TBuilder)this;
    }

    /// <summary>
    ///     Enables deletion of the resource.
    /// </summary>
    /// <returns>A builder that configures deletion.</returns>
    /// <remarks>
    ///     The resource's data source must implement
    ///     <see cref="IResourceDeleteHandler{TResource}" />.
    /// </remarks>
    public ResourceDeleteBuilder<TResource> AllowDelete()
    {
        var source = ConfigurationSource.Capture();
        Services.Configure<ResourceOptions<TResource>>(options =>
            options.Delete = new() { Source = source }
        );

        return new(Services);
    }

    /// <summary>
    ///     Enables and configures deletion of the resource.
    /// </summary>
    /// <param name="configure">A delegate that configures deletion.</param>
    /// <returns>The builder, so that calls can be chained.</returns>
    /// <exception cref="ArgumentNullException">
    ///     <paramref name="configure" /> is <see langword="null" />.
    /// </exception>
    /// <remarks>
    ///     The resource's data source must implement
    ///     <see cref="IResourceDeleteHandler{TResource}" />.
    /// </remarks>
    public TBuilder AllowDelete(Action<ResourceDeleteBuilder<TResource>> configure)
    {
        ArgumentNullException.ThrowIfNull(configure);

        configure(AllowDelete());

        return (TBuilder)this;
    }

    /// <summary>
    ///     Enables the edit page.
    /// </summary>
    /// <returns>A builder that configures the edit page.</returns>
    /// <remarks>
    ///     The resource's data source must implement
    ///     <see cref="IResourceEditHandler{TResource}" />.
    /// </remarks>
    public ResourceEditBuilder<TResource> AllowEdit()
    {
        var source = ConfigurationSource.Capture();
        Services.Configure<ResourceOptions<TResource>>(options =>
        {
            options.Edit = new ResourceEditOptions<TResource> { Source = source };
            options.EditLoader = null;
            options.EditHandler = null;
        });

        return EditBuilder<TResource>(source);
    }

    /// <summary>
    ///     Enables and configures the edit page.
    /// </summary>
    /// <param name="configure">A delegate that configures the edit page.</param>
    /// <returns>The builder, so that calls can be chained.</returns>
    /// <exception cref="ArgumentNullException">
    ///     <paramref name="configure" /> is <see langword="null" />.
    /// </exception>
    /// <remarks>
    ///     The resource's data source must implement
    ///     <see cref="IResourceEditHandler{TResource}" />.
    /// </remarks>
    public TBuilder AllowEdit(Action<ResourceEditBuilder<TResource>> configure)
    {
        ArgumentNullException.ThrowIfNull(configure);

        configure(AllowEdit());

        return (TBuilder)this;
    }

    /// <summary>
    ///     Enables the edit page with its own model and handler.
    /// </summary>
    /// <typeparam name="TModel">The type of the model the edit form binds to.</typeparam>
    /// <typeparam name="THandler">
    ///     The type of the handler that loads the model and saves the changes.
    /// </typeparam>
    /// <returns>A builder that configures the edit page.</returns>
    /// <remarks>
    ///     Registers <typeparamref name="THandler" /> as a scoped service if it isn't registered
    ///     already.
    /// </remarks>
    public ResourceEditBuilder<TModel> AllowEdit<TModel, THandler>()
        where THandler : class, IResourceEditHandler<TModel>
    {
        Services.TryAddScoped<THandler>();
        var source = ConfigurationSource.Capture();
        Services.Configure<ResourceOptions<TResource>>(options =>
        {
            options.Edit = new ResourceEditOptions<TModel> { Source = source };
            options.EditLoader = async (services, id, cancellationToken) =>
                await services.GetRequiredService<THandler>().FindAsync(id, cancellationToken);
            options.EditHandler = (services, id, model, cancellationToken) =>
                services
                    .GetRequiredService<THandler>()
                    .UpdateAsync(id, (TModel)model, cancellationToken);
        });

        return EditBuilder<TModel>(source);
    }

    /// <summary>
    ///     Enables and configures the edit page with its own model and handler.
    /// </summary>
    /// <typeparam name="TModel">The type of the model the edit form binds to.</typeparam>
    /// <typeparam name="THandler">
    ///     The type of the handler that loads the model and saves the changes.
    /// </typeparam>
    /// <param name="configure">A delegate that configures the edit page.</param>
    /// <returns>The builder, so that calls can be chained.</returns>
    /// <exception cref="ArgumentNullException">
    ///     <paramref name="configure" /> is <see langword="null" />.
    /// </exception>
    /// <remarks>
    ///     Registers <typeparamref name="THandler" /> as a scoped service if it isn't registered
    ///     already.
    /// </remarks>
    public TBuilder AllowEdit<TModel, THandler>(Action<ResourceEditBuilder<TModel>> configure)
        where THandler : class, IResourceEditHandler<TModel>
    {
        ArgumentNullException.ThrowIfNull(configure);
        configure(AllowEdit<TModel, THandler>());

        return (TBuilder)this;
    }

    /// <summary>
    ///     Enables the view page.
    /// </summary>
    /// <returns>A builder that configures the view page.</returns>
    /// <remarks>
    ///     The resource's data source must implement
    ///     <see cref="IResourceViewHandler{TResource}" />. The view page needs at least one field.
    /// </remarks>
    public ResourceViewBuilder<TResource> AllowView()
    {
        var source = ConfigurationSource.Capture();
        Services.Configure<ResourceOptions<TResource>>(options =>
        {
            options.View = new ResourceViewOptions<TResource> { Source = source };
            options.ViewLoader = null;
        });

        return ViewBuilder<TResource>(source);
    }

    /// <summary>
    ///     Enables and configures the view page.
    /// </summary>
    /// <param name="configure">A delegate that configures the view page.</param>
    /// <returns>The builder, so that calls can be chained.</returns>
    /// <exception cref="ArgumentNullException">
    ///     <paramref name="configure" /> is <see langword="null" />.
    /// </exception>
    /// <remarks>
    ///     The resource's data source must implement
    ///     <see cref="IResourceViewHandler{TResource}" />. The view page needs at least one field.
    /// </remarks>
    public TBuilder AllowView(Action<ResourceViewBuilder<TResource>> configure)
    {
        ArgumentNullException.ThrowIfNull(configure);

        configure(AllowView());

        return (TBuilder)this;
    }

    /// <summary>
    ///     Enables the view page with its own model and handler.
    /// </summary>
    /// <typeparam name="TModel">The type of the model the view page shows.</typeparam>
    /// <typeparam name="THandler">The type of the handler that loads the model.</typeparam>
    /// <returns>A builder that configures the view page.</returns>
    /// <remarks>
    ///     Registers <typeparamref name="THandler" /> as a scoped service if it isn't registered
    ///     already. The view page needs at least one field.
    /// </remarks>
    public ResourceViewBuilder<TModel> AllowView<TModel, THandler>()
        where THandler : class, IResourceViewHandler<TModel>
    {
        Services.TryAddScoped<THandler>();
        var source = ConfigurationSource.Capture();
        Services.Configure<ResourceOptions<TResource>>(options =>
        {
            options.View = new ResourceViewOptions<TModel> { Source = source };
            options.ViewLoader = async (services, id, cancellationToken) =>
                await services.GetRequiredService<THandler>().FindAsync(id, cancellationToken);
        });

        return ViewBuilder<TModel>(source);
    }

    /// <summary>
    ///     Enables and configures the view page with its own model and handler.
    /// </summary>
    /// <typeparam name="TModel">The type of the model the view page shows.</typeparam>
    /// <typeparam name="THandler">The type of the handler that loads the model.</typeparam>
    /// <param name="configure">A delegate that configures the view page.</param>
    /// <returns>The builder, so that calls can be chained.</returns>
    /// <exception cref="ArgumentNullException">
    ///     <paramref name="configure" /> is <see langword="null" />.
    /// </exception>
    /// <remarks>
    ///     Registers <typeparamref name="THandler" /> as a scoped service if it isn't registered
    ///     already. The view page needs at least one field.
    /// </remarks>
    public TBuilder AllowView<TModel, THandler>(Action<ResourceViewBuilder<TModel>> configure)
        where THandler : class, IResourceViewHandler<TModel>
    {
        ArgumentNullException.ThrowIfNull(configure);
        configure(AllowView<TModel, THandler>());

        return (TBuilder)this;
    }

    /// <summary>
    ///     Requires an authenticated user for the resource's pages.
    /// </summary>
    /// <returns>The builder, so that calls can be chained.</returns>
    public TBuilder RequireAuthorization()
    {
        return RequireAuthorization(Array.Empty<IAuthorizeData>());
    }

    /// <summary>
    ///     Requires the resource's pages to satisfy an authorization policy.
    /// </summary>
    /// <param name="policy">The policy to satisfy.</param>
    /// <returns>The builder, so that calls can be chained.</returns>
    /// <exception cref="ArgumentNullException">
    ///     <paramref name="policy" /> is <see langword="null" />.
    /// </exception>
    public TBuilder RequireAuthorization(AuthorizationPolicy policy)
    {
        AddAuthorizationMetadata(AuthorizationMetadata.Create(policy));

        return (TBuilder)this;
    }

    /// <summary>
    ///     Requires the resource's pages to satisfy an authorization policy that a delegate builds.
    /// </summary>
    /// <param name="configurePolicy">A delegate that builds the policy.</param>
    /// <returns>The builder, so that calls can be chained.</returns>
    /// <exception cref="ArgumentNullException">
    ///     <paramref name="configurePolicy" /> is <see langword="null" />.
    /// </exception>
    public TBuilder RequireAuthorization(Action<AuthorizationPolicyBuilder> configurePolicy)
    {
        AddAuthorizationMetadata(AuthorizationMetadata.Create(configurePolicy));

        return (TBuilder)this;
    }

    /// <summary>
    ///     Requires the resource's pages to satisfy the specified authorization data.
    /// </summary>
    /// <param name="authorizeData">
    ///     The authorization data to satisfy. Pass none to require only an authenticated user.
    /// </param>
    /// <returns>The builder, so that calls can be chained.</returns>
    /// <exception cref="ArgumentNullException">
    ///     <paramref name="authorizeData" /> is <see langword="null" />.
    /// </exception>
    public TBuilder RequireAuthorization(params IAuthorizeData[] authorizeData)
    {
        AddAuthorizationMetadata(AuthorizationMetadata.Create(authorizeData));

        return (TBuilder)this;
    }

    /// <summary>
    ///     Requires the resource's pages to satisfy the named authorization policies.
    /// </summary>
    /// <param name="policyNames">
    ///     The names of the policies to satisfy. Pass none to require only an authenticated user.
    /// </param>
    /// <returns>The builder, so that calls can be chained.</returns>
    /// <exception cref="ArgumentNullException">
    ///     <paramref name="policyNames" /> is <see langword="null" />.
    /// </exception>
    public TBuilder RequireAuthorization(params string[] policyNames)
    {
        AddAuthorizationMetadata(AuthorizationMetadata.Create(policyNames));

        return (TBuilder)this;
    }

    /// <summary>
    ///     Configures the resource's sidebar item.
    /// </summary>
    /// <param name="configure">A delegate that configures the sidebar item.</param>
    /// <returns>The builder, so that calls can be chained.</returns>
    /// <exception cref="ArgumentNullException">
    ///     <paramref name="configure" /> is <see langword="null" />.
    /// </exception>
    public TBuilder SidebarItem(Action<ResourceSidebarItemBuilder<TResource>> configure)
    {
        ArgumentNullException.ThrowIfNull(configure);

        configure(new(Services));

        return (TBuilder)this;
    }

    /// <summary>
    ///     Sets the expression that selects a record's title, which names the record wherever
    ///     another record refers to it, such as in a lookup.
    /// </summary>
    /// <param name="title">An expression that selects a record's title.</param>
    /// <returns>The builder, so that calls can be chained.</returns>
    /// <exception cref="ArgumentNullException">
    ///     <paramref name="title" /> is <see langword="null" />.
    /// </exception>
    /// <remarks>
    ///     By default, the title is the <see langword="string" /> property the resource's
    ///     <c>[DisplayColumn]</c> names, then its <c>Name</c> or <c>Title</c> property, then its
    ///     key. A record whose title is empty is named by its key.
    /// </remarks>
    public TBuilder UseTitle(Expression<Func<TResource, string?>> title)
    {
        ArgumentNullException.ThrowIfNull(title);

        Services.Configure<ResourceOptions<TResource>>(options => options.TitleExpression = title);

        return (TBuilder)this;
    }

    private void AddAuthorizationMetadata(object[] metadata)
    {
        Services.Configure<ResourceAuthorizationOptions<TResource>>(options =>
            options.Metadata.AddRange(metadata)
        );
    }

    private ResourceCreateBuilder<TModel> CreateBuilder<TModel>(ConfigurationSource? source) =>
        new(configure =>
            Services.Configure<ResourceOptions<TResource>>(options =>
            {
                // The create model was selected again since this builder was created
                if (options.Create is not ResourceCreateOptions<TModel> create)
                {
                    options.Report(
                        ConfigurationProblemKind.StaleModelBuilder,
                        $"This create configuration for `{typeof(TModel).Name}` isn't applied because another create model was selected later.",
                        "Remove this configuration, or configure the create page with the model it uses.",
                        source
                    );

                    return;
                }

                configure(create);
            })
        );

    private ResourceEditBuilder<TModel> EditBuilder<TModel>(ConfigurationSource? source) =>
        new(configure =>
            Services.Configure<ResourceOptions<TResource>>(options =>
            {
                // The edit model was selected again since this builder was created
                if (options.Edit is not ResourceEditOptions<TModel> edit)
                {
                    options.Report(
                        ConfigurationProblemKind.StaleModelBuilder,
                        $"This edit configuration for `{typeof(TModel).Name}` isn't applied because another edit model was selected later.",
                        "Remove this configuration, or configure the edit page with the model it uses.",
                        source
                    );

                    return;
                }

                configure(edit);
            })
        );

    private ResourceViewBuilder<TModel> ViewBuilder<TModel>(ConfigurationSource? source) =>
        new(configure =>
            Services.Configure<ResourceOptions<TResource>>(options =>
            {
                // The view model was selected again since this builder was created
                if (options.View is not ResourceViewOptions<TModel> view)
                {
                    options.Report(
                        ConfigurationProblemKind.StaleModelBuilder,
                        $"This view configuration for `{typeof(TModel).Name}` isn't applied because another view model was selected later.",
                        "Remove this configuration, or configure the view page with the model it uses.",
                        source
                    );

                    return;
                }

                configure(view);
            })
        );
}
