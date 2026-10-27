using StellarAdmin.Scenarios.Fixtures;

namespace StellarAdmin.Scenarios;

public sealed class StaffOnlyScenario : Scenario
{
    public override string Description =>
        "Only signed-in Admin and Agent staff can open the admin panel; the sidebar shows who is signed in.";

    public override string Name => "staff-only";

    public override void ConfigureAdmin(StellarAdminBuilder admin)
    {
        admin.RequireAuthorization(policy => policy.RequireRole("Admin", "Agent"));
        admin.AddVoyagerTravelResources();
    }
}
