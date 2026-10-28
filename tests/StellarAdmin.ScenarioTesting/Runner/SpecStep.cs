namespace StellarAdmin.ScenarioTesting.Runner;

public sealed record SpecStep(string Kind, int Line, Func<SpecRun, Task> RunAsync);
