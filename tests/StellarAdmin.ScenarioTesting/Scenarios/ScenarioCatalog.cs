using StellarAdmin.ScenarioTesting.Hosting;

namespace StellarAdmin.ScenarioTesting.Scenarios;

public static class ScenarioCatalog
{
    public static IReadOnlyList<Scenario> All { get; } =
    [new DefaultScenario(), new BrandingScenario(), new StaffOnlyScenario()];

    public static Scenario? Find(string name) =>
        All.FirstOrDefault(scenario =>
            string.Equals(scenario.Name, name, StringComparison.OrdinalIgnoreCase)
        );
}
