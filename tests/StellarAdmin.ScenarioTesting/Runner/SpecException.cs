namespace StellarAdmin.ScenarioTesting.Runner;

// Points at the spec file and line, so a failure reads like a compiler error
public sealed class SpecException(string path, int line, string message, Exception? inner = null)
    : Exception($"{path}:{line}: {message}", inner);
