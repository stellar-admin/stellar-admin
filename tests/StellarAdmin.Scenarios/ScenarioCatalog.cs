namespace StellarAdmin.Scenarios;

public static class ScenarioCatalog
{
    public static IReadOnlyList<Scenario> All { get; } =
    [new DefaultScenario(), new BrandingScenario()];

    public static Scenario? Find(string name) =>
        All.FirstOrDefault(scenario =>
            string.Equals(scenario.Name, name, StringComparison.OrdinalIgnoreCase)
        );
}
