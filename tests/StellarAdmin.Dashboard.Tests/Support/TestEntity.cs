namespace StellarAdmin.Dashboard.Tests.Support;

internal sealed class TestEntity
{
    public int Condition { get; set; }
    public DateTime CreatedAt { get; set; }
    public string Name { get; set; } = "";
    public decimal Price { get; set; }
    public string Sku { get; set; } = "";
}
