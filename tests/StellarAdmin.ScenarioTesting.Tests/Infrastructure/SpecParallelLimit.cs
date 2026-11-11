using TUnit.Core.Interfaces;

namespace StellarAdmin.ScenarioTesting.Tests.Infrastructure;

// Every spec starts its own app and browser page. Run all at once on a CI runner, which also runs the other
// test assemblies, and pages wait seconds for their first frames, so clicks time out; half the processors
// keeps them busy without starving them
public sealed class SpecParallelLimit : IParallelLimit
{
    public int Limit { get; } = Math.Max(2, Environment.ProcessorCount / 2);
}
