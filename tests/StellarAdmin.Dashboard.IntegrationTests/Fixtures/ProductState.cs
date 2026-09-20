using StellarAdmin.Dashboard.Resources;

namespace StellarAdmin.Dashboard.IntegrationTests.Fixtures;

public sealed class ProductState(IReadOnlyList<Product> products)
{
    public ResourceOperationResult? CreateResult { get; set; }
    public ResourceOperationResult? DeleteResult { get; set; }
    public ResourceOperationResult? UpdateResult { get; set; }

    public int DeleteCalls { get; set; }

    public bool DisappearOnUpdate { get; set; }

    public List<Product> Products { get; } = [.. products];
    public List<Guid> Requests { get; } = [];
    public int? SubmittedId { get; set; }
    public int UpdateCalls { get; set; }
}
