using StellarAdmin.Scenarios.Fixtures;

namespace StellarAdmin.Scenarios;

public sealed class DefaultScenario : Scenario
{
    public override string Description => "A trips resource with no other configuration.";

    public override string Name => "default";

    public override void ConfigureAdmin(StellarAdminBuilder admin)
    {
        admin.AddTripResource();
    }
}
