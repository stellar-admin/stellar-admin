namespace StellarAdmin.Diagnostics;

// A configuration mistake found while a page renders, which the page catches around the field it was rendering:
// the field is left out, and the problem is reported. Other exceptions, such as an app's handler failing, still fail
// the request.
internal sealed class ConfigurationException(ConfigurationProblemKind kind, string message)
    : InvalidOperationException(message)
{
    // The field's configuration is where the app made the mistake, not the page that found it
    public ConfigurationProblem ToProblem(string outcome, ConfigurationSource? source) =>
        new(kind, Message, outcome) { Source = source };
}
