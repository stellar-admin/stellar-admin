using StellarAdmin.Resources.Displays;

namespace StellarAdmin.ScenarioTesting.Host.Models;

// The docs' custom display with a handler, which shows an airport's name for a stored airport code. Its
// template, DisplayTemplates/Airport.cshtml, is in this host, so the display type is too.
public class AirportDisplay : FieldDisplay, IFieldDisplay<AirportDisplayHandler>
{
    public override string TemplateName => "Airport";
}

public class AirportDisplayHandler(AirportDisplay display, IAirportDirectory airports)
    : FieldDisplayHandler<AirportDisplay>(display)
{
    public override async Task<object?> PrepareAsync(
        FieldDisplayContext context,
        CancellationToken cancellationToken
    ) => context.Value is string code ? await airports.FindNameAsync(code, cancellationToken) : null;
}

public interface IAirportDirectory
{
    Task<string?> FindNameAsync(string code, CancellationToken cancellationToken);
}

// The seeded guides' home airports
public class AirportDirectory : IAirportDirectory
{
    private static readonly Dictionary<string, string> Names = new()
    {
        ["AMS"] = "Amsterdam Schiphol",
        ["CDG"] = "Paris Charles de Gaulle",
        ["CPT"] = "Cape Town International",
        ["FCO"] = "Rome Fiumicino",
        ["FRA"] = "Frankfurt",
        ["JFK"] = "New York JFK",
        ["LHR"] = "London Heathrow",
        ["LIS"] = "Lisbon Humberto Delgado",
        ["MAD"] = "Madrid Barajas",
        ["SYD"] = "Sydney Kingsford Smith",
    };

    public Task<string?> FindNameAsync(string code, CancellationToken cancellationToken) =>
        Task.FromResult(Names.GetValueOrDefault(code));
}
