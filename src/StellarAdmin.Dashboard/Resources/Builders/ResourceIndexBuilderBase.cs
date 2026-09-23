using System.Linq.Expressions;
using Microsoft.Extensions.DependencyInjection;
using StellarAdmin.Dashboard.Resources.Options;

namespace StellarAdmin.Dashboard.Resources.Builders;

/// <summary>
///     Configures a resource's index page.
/// </summary>
public abstract class ResourceIndexBuilderBase<TResource, TBuilder>
    where TBuilder : ResourceIndexBuilderBase<TResource, TBuilder>
{
    /// <summary>
    ///     The create button label.
    /// </summary>
    public string? CreateLabel
    {
        set =>
            Services.Configure<ResourceOptions<TResource>>(options =>
                options.Index.CreateLabel = value
            );
    }

    /// <summary>
    ///     The delete button label.
    /// </summary>
    public string? DeleteLabel
    {
        set =>
            Services.Configure<ResourceOptions<TResource>>(options =>
                options.Index.DeleteLabel = value
            );
    }

    /// <summary>
    ///     The edit link label.
    /// </summary>
    public string? EditLabel
    {
        set =>
            Services.Configure<ResourceOptions<TResource>>(options =>
                options.Index.EditLabel = value
            );
    }

    /// <summary>
    ///     The page title.
    /// </summary>
    public string? Title
    {
        set =>
            Services.Configure<ResourceOptions<TResource>>(options => options.Index.Title = value);
    }

    internal IServiceCollection Services { get; }

    internal ResourceIndexBuilderBase(IServiceCollection services) => Services = services;

    /// <summary>
    ///     Configures the index columns.
    /// </summary>
    public TBuilder Columns(Action<ResourceColumnsBuilder<TResource>> configure)
    {
        ArgumentNullException.ThrowIfNull(configure);

        configure(new(Services));

        return (TBuilder)this;
    }

    /// <summary>
    ///     Orders resources by the selected column by default.
    /// </summary>
    public TBuilder DefaultSortBy<TProperty>(Expression<Func<TResource, TProperty>> field) =>
        ConfigureDefaultSort(field, ResourceSortDirection.Ascending);

    /// <summary>
    ///     Orders resources by the selected column in descending order by default.
    /// </summary>
    public TBuilder DefaultSortByDescending<TProperty>(
        Expression<Func<TResource, TProperty>> field
    ) => ConfigureDefaultSort(field, ResourceSortDirection.Descending);

    /// <summary>
    ///     Enables index paging.
    /// </summary>
    public ResourcePagingBuilder<TResource> EnablePaging()
    {
        Services.Configure<ResourceOptions<TResource>>(options => options.Index.Paging = new());

        return new(Services);
    }

    /// <summary>
    ///     Enables and configures index paging.
    /// </summary>
    public TBuilder EnablePaging(Action<ResourcePagingBuilder<TResource>> configure)
    {
        ArgumentNullException.ThrowIfNull(configure);
        configure(EnablePaging());

        return (TBuilder)this;
    }

    private TBuilder ConfigureDefaultSort(LambdaExpression field, ResourceSortDirection direction)
    {
        ArgumentNullException.ThrowIfNull(field);
        if (ResourcePropertyPath.GetProperties(field) is not { } properties)
        {
            throw new ArgumentException(
                "Select a resource property path for the default sort.",
                nameof(field)
            );
        }

        Services.Configure<ResourceOptions<TResource>>(options =>
            options.Index.DefaultSort = new(ResourcePropertyPath.GetName(properties), direction)
        );

        return (TBuilder)this;
    }
}
