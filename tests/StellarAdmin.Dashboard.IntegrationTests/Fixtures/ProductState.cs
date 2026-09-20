namespace StellarAdmin.Dashboard.IntegrationTests.Fixtures;

public sealed class ProductState(IReadOnlyList<Product> products)
{
    public List<Product> Products { get; } = [.. products];
    public List<Guid> Requests { get; } = [];
    public int? SubmittedId { get; set; }
}
