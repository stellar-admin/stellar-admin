using StellarAdmin.Dashboard.Resources.Options;

namespace StellarAdmin.Dashboard.Resources.Builders;

/// <summary>
///     Configures a single scope of the index page.
/// </summary>
/// <typeparam name="TEntity">The entity type.</typeparam>
public class IndexPageScopeBuilder<TEntity>
    where TEntity : class
{
    private readonly IndexPageOptions<TEntity> _options;
    private readonly IndexPageScopeOptions<TEntity> _scope;

    internal IndexPageScopeBuilder(
        IndexPageOptions<TEntity> options,
        IndexPageScopeOptions<TEntity> scope
    )
    {
        _options = options;
        _scope = scope;
    }

    /// <summary>
    ///     Makes this the scope applied when the request does not specify one.
    /// </summary>
    /// <remarks>The first configured scope is the default when no scope is marked.</remarks>
    public IndexPageScopeBuilder<TEntity> Default()
    {
        foreach (var scope in _options.Scopes)
        {
            scope.IsDefault = false;
        }

        _scope.IsDefault = true;

        return this;
    }

    /// <summary>
    ///     Sets the value that identifies the scope in the query string, overriding the slug
    ///     derived from the title.
    /// </summary>
    public IndexPageScopeBuilder<TEntity> Slug(string slug)
    {
        ArgumentNullException.ThrowIfNull(slug);

        _scope.Slug = slug;

        return this;
    }
}
