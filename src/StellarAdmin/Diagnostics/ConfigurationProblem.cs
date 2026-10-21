using Microsoft.Extensions.Logging;

namespace StellarAdmin.Diagnostics;

// A mistake in the admin panel's configuration that the panel worked around. The message says what is wrong and how
// to fix it, the outcome what the panel did instead.
internal sealed record ConfigurationProblem(
    ConfigurationProblemKind Kind,
    ConfigurationProblemEffect Effect,
    string Message,
    string Outcome
)
{
    // The log event: the kind's, or the one an integration reported the problem with
    public EventId EventId => IntegrationEventId ?? new((int)Kind, Kind.ToString());

    public EventId? IntegrationEventId { get; init; }

    // The resource the problem belongs to, as the sheet groups problems and the log names them
    public string? Resource { get; init; }

    // A problem that another package logs, so the admin panel only shows it
    public bool IsLoggedElsewhere { get; init; }

    // The slug of the resource, when it has pages to link to
    public string? ResourceSlug { get; init; }
}
