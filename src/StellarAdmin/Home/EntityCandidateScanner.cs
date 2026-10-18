using System.Reflection;
using Microsoft.Extensions.DependencyInjection;

namespace StellarAdmin.Home;

// Finds the app's EF Core contexts and their entity types without referencing EF Core: a context
// registered with AddDbContext has a DbContextOptions<TContext> registration, and its entities
// are its DbSet<T> properties. The types are matched by name, and the registrations are read
// on first use so contexts registered after AddStellarAdmin are found too.
internal sealed class EntityCandidateScanner(IServiceCollection services)
{
    private const string DbContextOptionsTypeName =
        "Microsoft.EntityFrameworkCore.DbContextOptions`1";
    private const string DbSetTypeName = "Microsoft.EntityFrameworkCore.DbSet`1";
    private const string EfCorePackageTypeName =
        "StellarAdmin.EntityFrameworkCore.StellarAdminBuilderExtensions, StellarAdmin.EntityFrameworkCore";

    private readonly Lazy<EntityCandidates> _candidates = new(() => Scan(services));

    public EntityCandidates Candidates => _candidates.Value;

    private static EntityCandidates Scan(IServiceCollection services)
    {
        var contexts = services
            .Select(descriptor => descriptor.ServiceType)
            .Where(type => IsGenericTypeNamed(type, DbContextOptionsTypeName))
            .Select(type => type.GetGenericArguments()[0])
            .Distinct()
            .Select(context => new DbContextCandidate(context, GetEntityTypes(context)))
            .ToArray();

        var hasEfCorePackage = Type.GetType(EfCorePackageTypeName, throwOnError: false) is not null;

        return new(contexts, hasEfCorePackage);
    }

    private static IReadOnlyList<Type> GetEntityTypes(Type context)
    {
        // DbSets declared by framework contexts, such as Identity's claims and tokens tables,
        // are left out so the suggestions are the app's own entities
        return context
            .GetProperties(BindingFlags.Public | BindingFlags.Instance)
            .Where(property => IsGenericTypeNamed(property.PropertyType, DbSetTypeName))
            .Where(property =>
                property.DeclaringType?.Namespace?.StartsWith(
                    "Microsoft.",
                    StringComparison.Ordinal
                ) != true
            )
            .Select(property => property.PropertyType.GetGenericArguments()[0])
            .Distinct()
            .ToArray();
    }

    private static bool IsGenericTypeNamed(Type type, string genericTypeDefinitionName)
    {
        return type.IsConstructedGenericType
            && type.GetGenericTypeDefinition().FullName == genericTypeDefinitionName;
    }
}
