using StellarAdmin.Scenarios.Fixtures;

namespace StellarAdmin.Scenarios;

public sealed class DefaultScenario : Scenario
{
    public override string Description =>
        "Trips, destinations and countries on the Voyager Travel data, with no other configuration.";

    public override string Name => "default";

    public override void ConfigureAdmin(StellarAdminBuilder admin)
    {
        admin.AddVoyagerTravelResources();
    }
}
