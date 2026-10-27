namespace StellarAdmin.Scenarios.Specs;

public sealed record SpecStep(string Kind, int Line, Func<SpecRun, Task> RunAsync);
