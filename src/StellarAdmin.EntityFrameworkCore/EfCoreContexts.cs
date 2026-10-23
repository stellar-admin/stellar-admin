using System.Collections.Concurrent;
using System.Runtime.ExceptionServices;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace StellarAdmin.EntityFrameworkCore;

// Finds the registered context whose model maps an entity type, so resources and item sources don't name it. Every
// context registered with AddDbContext, AddDbContextPool or AddDbContextFactory has a DbContextOptions registration
// that names its type; a candidate must also be registered itself, to be resolved from a request's services. Each
// candidate's model is read once, and each entity's contexts are found once.
internal sealed class EfCoreContexts(
    IServiceScopeFactory scopeFactory,
    IServiceProviderIsService isService
)
{
    private readonly ConcurrentDictionary<Type, IReadOnlyList<Type>> _matches = new();
    private readonly ConcurrentDictionary<Type, IModel> _models = new();
    private IReadOnlyList<Type>? _candidates;

    public IReadOnlyList<Type> Candidates => _candidates ??= FindCandidates();

    // Why an entity has no single context, to follow "because"
    public string DescribeMissing(Type entityType, IReadOnlyList<Type> matches) =>
        matches.Count > 1 ? $"more than one DbContext maps `{entityType.Name}`: {Join(matches)}"
        : Candidates.Count == 0 ? "no DbContext is registered"
        : $"no registered DbContext maps `{entityType.Name}`";

    // The app's contexts. An app without EF Core resources hasn't registered them, so an item source finds its
    // context without the cache.
    public static EfCoreContexts For(IServiceProvider services) =>
        services.GetService<EfCoreContexts>()
        ?? new(
            services.GetRequiredService<IServiceScopeFactory>(),
            services.GetRequiredService<IServiceProviderIsService>()
        );

    // The candidates whose model maps the entity. A candidate that can't be created is passed over unless no other
    // maps the entity, so one context's mistake doesn't hide the resources of another
    public IReadOnlyList<Type> Find(Type entityType) =>
        _matches.GetOrAdd(
            entityType,
            static (entityType, contexts) =>
            {
                var matches = new List<Type>();
                ExceptionDispatchInfo? failure = null;
                foreach (var contextType in contexts.Candidates)
                {
                    try
                    {
                        if (contexts.Maps(contextType, entityType))
                        {
                            matches.Add(contextType);
                        }
                    }
                    catch (Exception exception)
                    {
                        failure ??= ExceptionDispatchInfo.Capture(exception);
                    }
                }

                if (matches.Count == 0)
                {
                    failure?.Throw();
                }

                return matches;
            },
            this
        );

    // The request's context for an entity. A pinned context isn't checked here: a resource's pin is checked when its
    // options are built, and EF Core reports an item source's when its query runs.
    public DbContext Get(IServiceProvider services, Type entityType, Type? pinned)
    {
        if (pinned is not null)
        {
            return (DbContext)services.GetRequiredService(pinned);
        }

        var matches = Find(entityType);
        if (matches is not [var contextType])
        {
            var message =
                $"`{entityType.Name}` items can't load because {DescribeMissing(entityType, matches)}. "
                + (
                    matches.Count > 1
                        ? "Call `UseDbContext<TContext>()` on the items to pick one."
                        : $"Add a `DbSet<{entityType.Name}>` to a DbContext registered with `AddDbContext`."
                );

            throw new InvalidOperationException(message.Replace("`", ""));
        }

        return (DbContext)services.GetRequiredService(contextType);
    }

    // The request's context for a resource's entity
    public DbContext Get<TEntity>(IServiceProvider services)
        where TEntity : class =>
        Get(
            services,
            typeof(TEntity),
            services
                .GetRequiredService<IOptions<EfCoreResourceOptions<TEntity>>>()
                .Value.ContextType
        );

    public bool Maps(Type contextType, Type entityType) =>
        _models
            .GetOrAdd(
                contextType,
                static (contextType, scopeFactory) =>
                {
                    // EF Core caches models, so one outlives its context
                    using var scope = scopeFactory.CreateScope();

                    return ((DbContext)scope.ServiceProvider.GetRequiredService(contextType)).Model;
                },
                scopeFactory
            )
            .FindEntityType(entityType)
            is not null;

    private IReadOnlyList<Type> FindCandidates()
    {
        using var scope = scopeFactory.CreateScope();

        return scope
            .ServiceProvider.GetServices<DbContextOptions>()
            .Select(options => options.ContextType)
            .Distinct()
            .Where(isService.IsService)
            .ToArray();
    }

    private static string Join(IReadOnlyList<Type> types) =>
        string.Join(", ", types.Take(types.Count - 1).Select(type => $"`{type.Name}`"))
        + $" and `{types[^1].Name}`";
}
