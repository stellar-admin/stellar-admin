namespace StellarAdmin.Diagnostics;

// A configuration mistake found while a page renders, which the page catches around the field it was rendering:
// the field is left out, and the problem is reported. Other exceptions, such as an app's handler failing, still fail
// the request.
internal sealed class ConfigurationException(ConfigurationProblemKind kind, string message)
    : InvalidOperationException(message)
{
    public ConfigurationProblem ToProblem(string outcome) =>
        new(kind, ConfigurationProblemEffect.TurnedOff, Message, outcome);
}
