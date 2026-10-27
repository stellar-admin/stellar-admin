namespace StellarAdmin.Scenarios.Specs;

// A scenario to start and the steps to run against it, loaded from a .spec.yaml file
public sealed record Spec(string Path, Scenario Scenario, IReadOnlyList<SpecStep> Steps);
