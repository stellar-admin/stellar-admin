namespace StellarAdmin.ScenarioTesting.Host.Models;

// The model of the _BookingHistory row details in the docs' row details example
public record BookingHistory(IReadOnlyList<BookingChange> Changes);

public record BookingChange(DateTime At, string Description);
