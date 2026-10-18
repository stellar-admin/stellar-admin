namespace StellarAdmin.Home;

internal sealed record EntityCandidates(
    IReadOnlyList<DbContextCandidate> Contexts,
    bool HasEfCorePackage
);

internal sealed record DbContextCandidate(Type ContextType, IReadOnlyList<Type> EntityTypes);
