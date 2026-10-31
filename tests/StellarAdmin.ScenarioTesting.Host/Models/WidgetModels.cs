namespace StellarAdmin.ScenarioTesting.Host.Models;

// The models of the widgets in the docs' widget examples

public record BookingStats(int Count);

public record Revenue(int Days, decimal Total);

public record BookingTotals(int Count, decimal Amount);

public record UpcomingDeparture(string Trip, DateOnly StartDate, int Booked, int Capacity);

public record OtherBooking(string Reference, string Trip);
