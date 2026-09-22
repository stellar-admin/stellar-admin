using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using StellarAdmin.Dashboard.Resources.Builders;

namespace StellarAdmin.Dashboard.EntityFrameworkCore;

/// <summary>
///     Configures an EF Core resource's index page.
/// </summary>
public sealed class EfCoreResourceIndexBuilder<TContext, TEntity>
    where TContext : DbContext
    where TEntity : class
{
    private readonly ResourceIndexBuilder<TEntity> _index;

    /// <summary>
    ///     The page title.
    /// </summary>
    public string? Title
    {
        set => _index.Title = value;
    }

    internal EfCoreResourceIndexBuilder(ResourceIndexBuilder<TEntity> index) => _index = index;

    /// <summary>
    ///     Configures the index columns.
    /// </summary>
    public EfCoreResourceIndexBuilder<TContext, TEntity> Columns(
        Action<ResourceColumnsBuilder<TEntity>> configure
    )
    {
        _index.Columns(configure);

        return this;
    }

    /// <summary>
    ///     Orders resources by the selected column by default.
    /// </summary>
    public EfCoreResourceIndexBuilder<TContext, TEntity> DefaultSortBy<TProperty>(
        Expression<Func<TEntity, TProperty>> field
    )
    {
        _index.DefaultSortBy(field);

        return this;
    }

    /// <summary>
    ///     Orders resources by the selected column in descending order by default.
    /// </summary>
    public EfCoreResourceIndexBuilder<TContext, TEntity> DefaultSortByDescending<TProperty>(
        Expression<Func<TEntity, TProperty>> field
    )
    {
        _index.DefaultSortByDescending(field);

        return this;
    }

    /// <summary>
    ///     Enables index paging.
    /// </summary>
    public ResourcePagingBuilder<TEntity> EnablePaging() => _index.EnablePaging();

    /// <summary>
    ///     Enables and configures index paging.
    /// </summary>
    public EfCoreResourceIndexBuilder<TContext, TEntity> EnablePaging(
        Action<ResourcePagingBuilder<TEntity>> configure
    )
    {
        _index.EnablePaging(configure);

        return this;
    }
}
