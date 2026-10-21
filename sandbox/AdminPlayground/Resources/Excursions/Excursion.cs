namespace AdminPlayground.Resources.Excursions;

public sealed class Excursion
{
    public int Id { get; set; }

    public string Name { get; set; } = "";

    public string Destination { get; set; } = "";

    public decimal Price { get; set; }
}
