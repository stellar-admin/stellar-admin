namespace StellarAdmin.ScenarioTesting.Hosting;

// Creates one scenario per attribute, passing the values to its constructor; the values are appended to the
// default name, e.g. [ScenarioArguments(WordmarkLayout.Start)] gives sidebar-branding-wordmark-layout-start
[AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
public sealed class ScenarioArgumentsAttribute(params object?[] values) : Attribute
{
    public object?[] Values { get; } = values;
}
