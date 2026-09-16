using StellarAdmin.Pro.Resources.Options;

namespace StellarAdmin.Pro.Resources.Builders;

/// <summary>
///     Configures the search settings of the index page.
/// </summary>
/// <typeparam name="TEntity">The entity type.</typeparam>
public class IndexPageSearchBuilder<TEntity>
    where TEntity : class
{
    private readonly IndexPageSearchOptions<TEntity> _options;

    /// <summary>Sets the search box placeholder text.</summary>
    /// <remarks>Defaults to "Search...".</remarks>
    public string? Placeholder
    {
        get => _options.Placeholder;
        set => _options.Placeholder = value;
    }

    internal IndexPageSearchBuilder(IndexPageSearchOptions<TEntity> options)
    {
        _options = options;
    }
}
