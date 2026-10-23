namespace AdminPlayground.Resources.Excursions;

// No registered context maps it, so its EF resource is unavailable
public sealed class ExcursionBooking
{
    public int Id { get; set; }

    public string Traveller { get; set; } = "";
}
