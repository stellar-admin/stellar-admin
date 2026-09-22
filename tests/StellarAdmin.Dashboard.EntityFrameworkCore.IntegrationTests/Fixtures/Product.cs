namespace StellarAdmin.Dashboard.EntityFrameworkCore.IntegrationTests.Fixtures;

public sealed class Product
{
    public int Number { get; set; }
    public string Name { get; set; } = "";
    public int Price { get; set; }
    public bool Hidden { get; set; }
}
