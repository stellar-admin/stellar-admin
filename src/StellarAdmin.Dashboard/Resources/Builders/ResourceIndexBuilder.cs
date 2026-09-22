using System.Linq.Expressions;
using Microsoft.Extensions.DependencyInjection;
using StellarAdmin.Dashboard.Resources.Options;

namespace StellarAdmin.Dashboard.Resources.Builders;

/// <summary>
///     Configures a resource's index page.
/// </summary>
public sealed class ResourceIndexBuilder<TResource>
{
    private readonly IServiceCollection _services;

    /// <summary>
    ///     The create button label.
    /// </summary>
    public string? CreateLabel
    {
        set =>
            _services.Configure<ResourceOptions<TResource>>(options =>
                options.Index.CreateLabel = value
            );
    }

    /// <summary>
    ///     The delete button label.
    /// </summary>
    public string? DeleteLabel
    {
        set =>
            _services.Configure<ResourceOptions<TResource>>(options =>
                options.Index.DeleteLabel = value
            );
    }

    /// <summary>
    ///     The edit link label.
    /// </summary>
    public string? EditLabel
    {
        set =>
            _services.Configure<ResourceOptions<TResource>>(options =>
                options.Index.EditLabel = value
            );
    }

    /// <summary>
    ///     The page title.
    /// </summary>
    public string? Title
    {
        set =>
            _services.Configure<ResourceOptions<TResource>>(options => options.Index.Title = value);
    }

    internal ResourceIndexBuilder(IServiceCollection services) => _services = services;

    /// <summary>
    ///     Configures the index columns.
    /// </summary>
    public ResourceIndexBuilder<TResource> Columns(
        Action<ResourceColumnsBuilder<TResource>> configure
    )
    {
        ArgumentNullException.ThrowIfNull(configure);

        configure(new(_services));

        return this;
    }

    /// <summary>
    ///     Orders resources by the selected column by default.
    /// </summary>
    public ResourceIndexBuilder<TResource> DefaultSortBy<TProperty>(
        Expression<Func<TResource, TProperty>> field
    ) => ConfigureDefaultSort(field, ResourceSortDirection.Ascending);

    /// <summary>
    ///     Orders resources by the selected column in descending order by default.
    /// </summary>
    public ResourceIndexBuilder<TResource> DefaultSortByDescending<TProperty>(
        Expression<Func<TResource, TProperty>> field
    ) => ConfigureDefaultSort(field, ResourceSortDirection.Descending);

    /// <summary>
    ///     Enables index paging.
    /// </summary>
    public ResourcePagingBuilder<TResource> EnablePaging()
    {
        _services.Configure<ResourceOptions<TResource>>(options => options.Index.Paging = new());

        return new(_services);
    }

    /// <summary>
    ///     Enables and configures index paging.
    /// </summary>
    public ResourceIndexBuilder<TResource> EnablePaging(
        Action<ResourcePagingBuilder<TResource>> configure
    )
    {
        ArgumentNullException.ThrowIfNull(configure);
        configure(EnablePaging());

        return this;
    }

    /// <summary>
    ///     Enables index scopes.
    /// </summary>
    public ResourceScopesBuilder<TResource> EnableScopes()
    {
        _services.Configure<ResourceOptions<TResource>>(options => options.Index.Scopes = new());

        return new(_services);
    }

    /// <summary>
    ///     Enables and configures index scopes.
    /// </summary>
    public ResourceIndexBuilder<TResource> EnableScopes(
        Action<ResourceScopesBuilder<TResource>> configure
    )
    {
        ArgumentNullException.ThrowIfNull(configure);
        configure(EnableScopes());

        return this;
    }

    /// <summary>
    ///     Enables index searching.
    /// </summary>
    public ResourceSearchBuilder<TResource> EnableSearch()
    {
        _services.Configure<ResourceOptions<TResource>>(options => options.Index.Search = new());

        return new(_services);
    }

    /// <summary>
    ///     Enables and configures index searching.
    /// </summary>
    public ResourceIndexBuilder<TResource> EnableSearch(
        Action<ResourceSearchBuilder<TResource>> configure
    )
    {
        ArgumentNullException.ThrowIfNull(configure);
        configure(EnableSearch());

        return this;
    }

    private ResourceIndexBuilder<TResource> ConfigureDefaultSort(
        LambdaExpression field,
        ResourceSortDirection direction
    )
    {
        ArgumentNullException.ThrowIfNull(field);
        if (field.Body is not MemberExpression { Expression: ParameterExpression } member)
        {
            throw new ArgumentException(
                "Select a resource property for the default sort.",
                nameof(field)
            );
        }

        _services.Configure<ResourceOptions<TResource>>(options =>
            options.Index.DefaultSort = new(member.Member.Name, direction)
        );

        return this;
    }
}
