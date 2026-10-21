namespace AdminPlayground.Resources.Excursions;

// Not an entity of ApplicationDbContext, so its EF resource is unavailable
public sealed class ExcursionBooking
{
    public int Id { get; set; }

    public string Traveller { get; set; } = "";
}
