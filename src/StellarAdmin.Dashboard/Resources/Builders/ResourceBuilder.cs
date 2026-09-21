using System.Globalization;
using System.Linq.Expressions;
using System.Reflection;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using StellarAdmin.Dashboard.Resources.Options;

namespace StellarAdmin.Dashboard.Resources.Builders;

/// <summary>
///     Configures a resource.
/// </summary>
public sealed class ResourceBuilder<TResource>
{
    private readonly IServiceCollection _services;

    /// <summary>
    ///     The plural resource label.
    /// </summary>
    public string PluralLabel
    {
        set =>
            _services.Configure<ResourceOptions<TResource>>(options => options.PluralLabel = value);
    }

    /// <summary>
    ///     The singular resource label.
    /// </summary>
    public string SingularLabel
    {
        set =>
            _services.Configure<ResourceOptions<TResource>>(options =>
                options.SingularLabel = value
            );
    }

    internal ResourceBuilder(IServiceCollection services) => _services = services;

    /// <summary>
    ///     Enables and configures the create page.
    /// </summary>
    public ResourceCreateBuilder<TResource> AllowCreate()
    {
        _services.Configure<ResourceOptions<TResource>>(options =>
        {
            options.Create = new ResourceCreateOptions<TResource>();
            options.CreateHandler = null;
        });

        return CreateBuilder<TResource>();
    }

    /// <summary>
    ///     Enables and configures the create page.
    /// </summary>
    public ResourceBuilder<TResource> AllowCreate(
        Action<ResourceCreateBuilder<TResource>> configure
    )
    {
        ArgumentNullException.ThrowIfNull(configure);
        configure(AllowCreate());

        return this;
    }

    /// <summary>
    ///     Enables and configures a create form with a custom model and handler.
    /// </summary>
    public ResourceCreateBuilder<TModel> AllowCreate<TModel, THandler>()
        where THandler : class, IResourceCreateHandler<TModel>
    {
        _services.TryAddScoped<THandler>();
        _services.Configure<ResourceOptions<TResource>>(options =>
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
    public ResourceBuilder<TResource> AllowCreate<TModel, THandler>(
        Action<ResourceCreateBuilder<TModel>> configure
    )
        where THandler : class, IResourceCreateHandler<TModel>
    {
        ArgumentNullException.ThrowIfNull(configure);
        configure(AllowCreate<TModel, THandler>());

        return this;
    }

    /// <summary>
    ///     Enables and configures resource deletion.
    /// </summary>
    public ResourceDeleteBuilder<TResource> AllowDelete()
    {
        _services.Configure<ResourceOptions<TResource>>(options => options.Delete = new());

        return new(_services);
    }

    /// <summary>
    ///     Enables and configures resource deletion.
    /// </summary>
    public ResourceBuilder<TResource> AllowDelete(
        Action<ResourceDeleteBuilder<TResource>> configure
    )
    {
        ArgumentNullException.ThrowIfNull(configure);

        configure(AllowDelete());

        return this;
    }

    /// <summary>
    ///     Enables and configures the edit page.
    /// </summary>
    public ResourceEditBuilder<TResource> AllowEdit()
    {
        _services.Configure<ResourceOptions<TResource>>(options =>
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
    public ResourceBuilder<TResource> AllowEdit(Action<ResourceEditBuilder<TResource>> configure)
    {
        ArgumentNullException.ThrowIfNull(configure);

        configure(AllowEdit());

        return this;
    }

    /// <summary>
    ///     Enables and configures an edit form with a custom model and handler.
    /// </summary>
    public ResourceEditBuilder<TModel> AllowEdit<TModel, THandler>()
        where THandler : class, IResourceEditHandler<TModel>
    {
        _services.TryAddScoped<THandler>();
        _services.Configure<ResourceOptions<TResource>>(options =>
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
    public ResourceBuilder<TResource> AllowEdit<TModel, THandler>(
        Action<ResourceEditBuilder<TModel>> configure
    )
        where THandler : class, IResourceEditHandler<TModel>
    {
        ArgumentNullException.ThrowIfNull(configure);
        configure(AllowEdit<TModel, THandler>());

        return this;
    }

    /// <summary>
    ///     Configures the index page.
    /// </summary>
    public ResourceBuilder<TResource> Index(Action<ResourceIndexBuilder<TResource>> configure)
    {
        ArgumentNullException.ThrowIfNull(configure);

        configure(new(_services));

        return this;
    }

    /// <summary>
    ///     Specifies the data source used for the resource.
    /// </summary>
    public ResourceBuilder<TResource> UseDataSource<TDataSource>()
        where TDataSource : class, IResourceDataSource<TResource>
    {
        _services.Configure<ResourceOptions<TResource>>(options =>
            options.DataSourceType = typeof(TDataSource)
        );
        _services.TryAddScoped<TDataSource>();
        _services.AddScoped<IResourceDataSource<TResource>>(services =>
            services.GetRequiredService<TDataSource>()
        );

        return this;
    }

    /// <summary>
    ///     Specifies the property that identifies a resource.
    /// </summary>
    public ResourceBuilder<TResource> UseKey<TKey>(Expression<Func<TResource, TKey>> key)
    {
        ArgumentNullException.ThrowIfNull(key);
        if (
            key.Body is not MemberExpression { Member: PropertyInfo property } member
            || member.Expression != key.Parameters[0]
            || property.GetMethod?.IsPublic != true
        )
        {
            throw new ArgumentException(
                "Select a direct property with a public getter.",
                nameof(key)
            );
        }

        var selector = key.Compile();
        _services.Configure<ResourceOptions<TResource>>(options =>
        {
            options.KeySelector = resource =>
                Convert.ToString(selector(resource), CultureInfo.InvariantCulture)!;
            options.KeyPropertyName = property.Name;
        });

        return this;
    }

    private ResourceCreateBuilder<TModel> CreateBuilder<TModel>() =>
        new(configure =>
            _services.Configure<ResourceOptions<TResource>>(options =>
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
            _services.Configure<ResourceOptions<TResource>>(options =>
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
