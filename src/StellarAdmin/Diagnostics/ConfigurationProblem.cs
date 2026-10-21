using Microsoft.Extensions.Logging;

namespace StellarAdmin.Diagnostics;

// A mistake in the admin panel's configuration that the panel worked around. The message says what the developer
// notices, why, and what happens instead; the fix what to change. Backticks mark code in both.
internal sealed record ConfigurationProblem(
    ConfigurationProblemKind Kind,
    string Message,
    string Fix
)
{
    // The log event: the kind's, or the one an integration reported the problem with
    public EventId EventId => IntegrationEventId ?? new((int)Kind, Kind.ToString());

    public EventId? IntegrationEventId { get; init; }

    // A problem that another package logs, so the admin panel only shows it
    public bool IsLoggedElsewhere { get; init; }

    public ConfigurationProblemLevel Level { get; init; } = ConfigurationProblemLevel.Warning;

    // A problem that makes the whole resource unavailable: it is hidden from the sidebar and its pages return 404
    public bool MakesResourceUnavailable => Level == ConfigurationProblemLevel.Error;

    // The resource the problem belongs to, as the log names it
    public string? Resource { get; init; }

    // The slug of the resource, when it has pages to link to
    public string? ResourceSlug { get; init; }

    // Where the app configured what is wrong. By default the app's code that called the admin panel method reporting
    // the problem; a check that runs later names the configuration it found the problem in.
    public ConfigurationSource? Source { get; init; } = ConfigurationSource.Capture();

    public static ConfigurationProblem Unavailable(
        ConfigurationProblemKind kind,
        string message,
        string fix
    ) => new(kind, message, fix) { Level = ConfigurationProblemLevel.Error };

    // The message and the fix as one line of plain text, for the log and the exception
    public string Describe() => $"{Plain(Message)} Fix: {Plain(Fix)}";

    private static string Plain(string text) => text.Replace("`", "", StringComparison.Ordinal);
}
