using StellarAdmin.Dashboard.Resources.Options;

namespace StellarAdmin.Dashboard.EntityFrameworkCore;

/// <summary>
///     Configures an EF Core resource's index page.
/// </summary>
public sealed class EfCoreResourceIndexOptions<TEntity> : ResourceIndexOptions
    where TEntity : class;
