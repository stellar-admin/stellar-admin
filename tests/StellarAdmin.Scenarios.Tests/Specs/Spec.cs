namespace StellarAdmin.Scenarios.Tests.Specs;

// A scenario to start and the steps to run against it, loaded from a .spec.yaml file
internal sealed record Spec(string Path, Scenario Scenario, IReadOnlyList<SpecStep> Steps);
