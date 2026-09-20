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
    public ResourceCreateBuilder<TResource> Create()
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
    public ResourceBuilder<TResource> Create(Action<ResourceCreateBuilder<TResource>> configure)
    {
        ArgumentNullException.ThrowIfNull(configure);
        configure(Create());

        return this;
    }

    /// <summary>
    ///     Enables and configures a create form with a custom model and handler.
    /// </summary>
    public ResourceCreateBuilder<TModel> Create<TModel, THandler>()
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
    public ResourceBuilder<TResource> Create<TModel, THandler>(
        Action<ResourceCreateBuilder<TModel>> configure
    )
        where THandler : class, IResourceCreateHandler<TModel>
    {
        ArgumentNullException.ThrowIfNull(configure);
        configure(Create<TModel, THandler>());

        return this;
    }

    /// <summary>
    ///     Enables and configures resource deletion.
    /// </summary>
    public ResourceDeleteBuilder<TResource> Delete()
    {
        _services.Configure<ResourceOptions<TResource>>(options => options.Delete = new());

        return new(_services);
    }

    /// <summary>
    ///     Enables and configures resource deletion.
    /// </summary>
    public ResourceBuilder<TResource> Delete(Action<ResourceDeleteBuilder<TResource>> configure)
    {
        ArgumentNullException.ThrowIfNull(configure);

        configure(Delete());

        return this;
    }

    /// <summary>
    ///     Enables and configures the edit page.
    /// </summary>
    public ResourceEditBuilder<TResource> Edit()
    {
        _services.Configure<ResourceOptions<TResource>>(options => options.Edit = new());

        return new(_services);
    }

    /// <summary>
    ///     Enables and configures the edit page.
    /// </summary>
    public ResourceBuilder<TResource> Edit(Action<ResourceEditBuilder<TResource>> configure)
    {
        ArgumentNullException.ThrowIfNull(configure);

        configure(Edit());

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
}
