namespace StellarAdmin.Scenarios.Fixtures;

public sealed class Trip
{
    public DateOnly DepartsOn { get; set; }

    public string Destination { get; set; } = "";

    public int Id { get; set; }

    public string Name { get; set; } = "";

    public decimal Price { get; set; }
}
