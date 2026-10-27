namespace StellarAdmin.Scenarios.Tests.Specs;

// Points at the spec file and line, so a failure reads like a compiler error
internal sealed class SpecException(string path, int line, string message, Exception? inner = null)
    : Exception($"{path}:{line}: {message}", inner);
