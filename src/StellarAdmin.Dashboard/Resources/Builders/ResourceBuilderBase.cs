using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using StellarAdmin.Dashboard.Resources.Options;

namespace StellarAdmin.Dashboard.Resources.Builders;

/// <summary>
///     Configures a resource.
/// </summary>
public abstract class ResourceBuilderBase<TResource, TBuilder>
    where TBuilder : ResourceBuilderBase<TResource, TBuilder>
{
    /// <summary>
    ///     The plural resource label.
    /// </summary>
    public string PluralLabel
    {
        set =>
            Services.Configure<ResourceOptions<TResource>>(options => options.PluralLabel = value);
    }

    /// <summary>
    ///     The singular resource label.
    /// </summary>
    public string SingularLabel
    {
        set =>
            Services.Configure<ResourceOptions<TResource>>(options =>
                options.SingularLabel = value
            );
    }

    internal IServiceCollection Services { get; }

    internal ResourceBuilderBase(IServiceCollection services) => Services = services;

    /// <summary>
    ///     Enables and configures the create page.
    /// </summary>
    public ResourceCreateBuilder<TResource> AllowCreate()
    {
        Services.Configure<ResourceOptions<TResource>>(options =>
        {
            options.Create = new ResourceCreateOptions<TResource>();
            options.CreateHandler = null;
        });

        return CreateBuilder<TResource>();
    }

    /// <summary>
    ///     Enables and configures the create page.
    /// </summary>
    public TBuilder AllowCreate(Action<ResourceCreateBuilder<TResource>> configure)
    {
        ArgumentNullException.ThrowIfNull(configure);
        configure(AllowCreate());

        return (TBuilder)this;
    }

    /// <summary>
    ///     Enables and configures a create form with a custom model and handler.
    /// </summary>
    public ResourceCreateBuilder<TModel> AllowCreate<TModel, THandler>()
        where THandler : class, IResourceCreateHandler<TModel>
    {
        Services.TryAddScoped<THandler>();
        Services.Configure<ResourceOptions<TResource>>(options =>
        {
            options.Create = new ResourceCreateOptions<TModel>();
            options.CreateHandler = (services, model, cancellationToken) =>
                services
                    .GetRequiredService<THandler>()
                    .CreateAsync((TModel)model, cancellationToken);
        });

        return CreateBuilder<TModel>();
    }

    /// <summary>
    ///     Enables and configures a create form with a custom model and handler.
    /// </summary>
    public TBuilder AllowCreate<TModel, THandler>(Action<ResourceCreateBuilder<TModel>> configure)
        where THandler : class, IResourceCreateHandler<TModel>
    {
        ArgumentNullException.ThrowIfNull(configure);
        configure(AllowCreate<TModel, THandler>());

        return (TBuilder)this;
    }

    /// <summary>
    ///     Enables and configures resource deletion.
    /// </summary>
    public ResourceDeleteBuilder<TResource> AllowDelete()
    {
        Services.Configure<ResourceOptions<TResource>>(options => options.Delete = new());

        return new(Services);
    }

    /// <summary>
    ///     Enables and configures resource deletion.
    /// </summary>
    public TBuilder AllowDelete(Action<ResourceDeleteBuilder<TResource>> configure)
    {
        ArgumentNullException.ThrowIfNull(configure);

        configure(AllowDelete());

        return (TBuilder)this;
    }

    /// <summary>
    ///     Enables and configures the edit page.
    /// </summary>
    public ResourceEditBuilder<TResource> AllowEdit()
    {
        Services.Configure<ResourceOptions<TResource>>(options =>
        {
            options.Edit = new ResourceEditOptions<TResource>();
            options.EditLoader = null;
            options.EditHandler = null;
        });

        return EditBuilder<TResource>();
    }

    /// <summary>
    ///     Enables and configures the edit page.
    /// </summary>
    public TBuilder AllowEdit(Action<ResourceEditBuilder<TResource>> configure)
    {
        ArgumentNullException.ThrowIfNull(configure);

        configure(AllowEdit());

        return (TBuilder)this;
    }

    /// <summary>
    ///     Enables and configures an edit form with a custom model and handler.
    /// </summary>
    public ResourceEditBuilder<TModel> AllowEdit<TModel, THandler>()
        where THandler : class, IResourceEditHandler<TModel>
    {
        Services.TryAddScoped<THandler>();
        Services.Configure<ResourceOptions<TResource>>(options =>
        {
            options.Edit = new ResourceEditOptions<TModel>();
            options.EditLoader = async (services, id, cancellationToken) =>
                await services.GetRequiredService<THandler>().FindAsync(id, cancellationToken);
            options.EditHandler = (services, id, model, cancellationToken) =>
                services
                    .GetRequiredService<THandler>()
                    .UpdateAsync(id, (TModel)model, cancellationToken);
        });

        return EditBuilder<TModel>();
    }

    /// <summary>
    ///     Enables and configures an edit form with a custom model and handler.
    /// </summary>
    public TBuilder AllowEdit<TModel, THandler>(Action<ResourceEditBuilder<TModel>> configure)
        where THandler : class, IResourceEditHandler<TModel>
    {
        ArgumentNullException.ThrowIfNull(configure);
        configure(AllowEdit<TModel, THandler>());

        return (TBuilder)this;
    }

    /// <summary>
    ///     Configures the resource's sidebar item.
    /// </summary>
    public TBuilder SidebarItem(Action<ResourceSidebarItemBuilder<TResource>> configure)
    {
        ArgumentNullException.ThrowIfNull(configure);

        configure(new(Services));

        return (TBuilder)this;
    }

    private ResourceCreateBuilder<TModel> CreateBuilder<TModel>() =>
        new(configure =>
            Services.Configure<ResourceOptions<TResource>>(options =>
            {
                if (options.Create is not ResourceCreateOptions<TModel> create)
                {
                    throw new InvalidOperationException(
                        "The create builder's model no longer matches the selected create model."
                    );
                }

                configure(create);
            })
        );

    private ResourceEditBuilder<TModel> EditBuilder<TModel>() =>
        new(configure =>
            Services.Configure<ResourceOptions<TResource>>(options =>
            {
                if (options.Edit is not ResourceEditOptions<TModel> edit)
                {
                    throw new InvalidOperationException(
                        "The edit builder's model no longer matches the selected edit model."
                    );
                }

                configure(edit);
            })
        );
}
