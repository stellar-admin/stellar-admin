namespace StellarAdmin.Diagnostics;

// A configuration mistake found while a page renders, which the page catches around the field it was rendering:
// the field is left out, and the problem is reported. Other exceptions, such as an app's handler failing, still fail
// the request. The cause is a clause that follows "because", such as "`Booking` has no `Code` property".
internal sealed class ConfigurationException(ConfigurationProblemKind kind, string cause, string fix)
    : InvalidOperationException(cause)
{
    public string Fix => fix;

    // The field's configuration is where the app made the mistake, not the page that found it
    public ConfigurationProblem ToProblem(string fieldName, ConfigurationSource? source) =>
        new(kind, $"The `{fieldName}` field isn't shown because {Message}.", fix) { Source = source };
}
