using System.Collections.Concurrent;
using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore.Metadata;

namespace StellarAdmin.EntityFrameworkCore;

// The navigations each EF Core resource's title reads, such as Parent for c => c.Parent!.Name + " / " + c.Name, which
// a display that shows the resource's records loads with them
internal sealed class EfCoreResourceTitles(
    IServiceProvider services,
    IEnumerable<EfCoreResourceTitle> titles
)
{
    private readonly ConcurrentDictionary<Type, string[]> _includePaths = new();

    public IReadOnlyList<string> FindIncludePaths(IEntityType entityType) =>
        _includePaths.GetOrAdd(
            entityType.ClrType,
            type =>
                titles.FirstOrDefault(title => title.EntityType == type)?.GetTitle(services)
                    is { } title
                    ? [.. EfCorePropertyPaths.FindDisplayIncludePaths(entityType, title)]
                    : []
        );
}

internal sealed record EfCoreResourceTitle(
    Type EntityType,
    Func<IServiceProvider, LambdaExpression?> GetTitle
);
