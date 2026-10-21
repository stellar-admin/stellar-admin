using Microsoft.Extensions.Logging;

namespace StellarAdmin.Diagnostics;

// A mistake in the admin panel's configuration that the panel worked around. The message says what is wrong and how
// to fix it, the outcome what the panel did instead.
internal sealed record ConfigurationProblem(
    ConfigurationProblemKind Kind,
    string Message,
    string Outcome
)
{
    public const string UnavailableOutcome =
        "The resource is hidden from the sidebar, and its pages return 404.";

    // The log event: the kind's, or the one an integration reported the problem with
    public EventId EventId => IntegrationEventId ?? new((int)Kind, Kind.ToString());

    public EventId? IntegrationEventId { get; init; }

    // The resource the problem belongs to, as the sheet groups problems and the log names them
    public string? Resource { get; init; }

    // A problem that another package logs, so the admin panel only shows it
    public bool IsLoggedElsewhere { get; init; }

    // A problem that makes the whole resource unavailable: it is hidden from the sidebar and its pages return 404
    public bool MakesResourceUnavailable { get; init; }

    // The slug of the resource, when it has pages to link to
    public string? ResourceSlug { get; init; }

    // Where the app configured what is wrong. By default the app's code that called the admin panel method reporting
    // the problem; a check that runs later names the configuration it found the problem in.
    public ConfigurationSource? Source { get; init; } = ConfigurationSource.Capture();

    public static ConfigurationProblem Unavailable(ConfigurationProblemKind kind, string message) =>
        new(kind, message, UnavailableOutcome) { MakesResourceUnavailable = true };
}
