namespace StellarAdmin.Scenarios.Tests.Specs;

internal sealed record SpecStep(string Kind, int Line, Func<SpecRun, Task> RunAsync);
