using System.Linq.Expressions;
using System.Text;
using StellarAdmin.Dashboard.Resources.Options;

namespace StellarAdmin.Dashboard.Resources.Builders;

/// <summary>
///     Configures the scopes of the index page.
/// </summary>
/// <typeparam name="TEntity">The entity type.</typeparam>
public class IndexPageScopesBuilder<TEntity>
    where TEntity : class
{
    private readonly IndexPageOptions<TEntity> _options;

    internal IndexPageScopesBuilder(IndexPageOptions<TEntity> options)
    {
        _options = options;
    }

    /// <summary>
    ///     Adds a scope that filters the list by <paramref name="predicate" />, after the
    ///     scopes already configured.
    /// </summary>
    /// <param name="title">The tab label. The query-string slug is derived from it.</param>
    /// <param name="predicate">
    ///     An expression that filters the list, like <c>x => x.EmailConfirmed</c>. Omit it
    ///     for an unfiltered scope.
    /// </param>
    public IndexPageScopeBuilder<TEntity> Add(
        string title,
        Expression<Func<TEntity, bool>>? predicate = null
    )
    {
        ArgumentNullException.ThrowIfNull(title);

        var scope = new IndexPageScopeOptions<TEntity>(title, DeriveSlug(title), predicate);
        _options.AddScope(scope);

        return new IndexPageScopeBuilder<TEntity>(_options, scope);
    }

    /// <summary>Removes all scopes.</summary>
    /// <remarks>With no scopes the page renders no tabs and lists everything.</remarks>
    public IndexPageScopesBuilder<TEntity> Clear()
    {
        _options.ClearScopes();

        return this;
    }

    private static string DeriveSlug(string title)
    {
        var slug = new StringBuilder(title.Length);
        foreach (var character in title.ToLowerInvariant())
        {
            if (char.IsAsciiLetterOrDigit(character))
            {
                slug.Append(character);
            }
            else if (slug.Length > 0 && slug[^1] != '-')
            {
                slug.Append('-');
            }
        }

        return slug.ToString().TrimEnd('-');
    }
}
