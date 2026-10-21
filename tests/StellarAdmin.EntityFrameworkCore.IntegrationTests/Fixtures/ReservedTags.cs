namespace StellarAdmin.EntityFrameworkCore.IntegrationTests.Fixtures;

// The tags NotReservedTagsAttribute rejects, registered by the tests that need it
public sealed class ReservedTags(params int[] ids)
{
    public IReadOnlySet<int> Ids { get; } = ids.ToHashSet();
}
