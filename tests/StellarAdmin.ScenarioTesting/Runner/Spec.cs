using StellarAdmin.ScenarioTesting.Hosting;

namespace StellarAdmin.ScenarioTesting.Runner;

// A scenario to start and the steps to run against it, loaded from a .spec.yaml file
public sealed record Spec(string Path, Scenario Scenario, IReadOnlyList<SpecStep> Steps);
