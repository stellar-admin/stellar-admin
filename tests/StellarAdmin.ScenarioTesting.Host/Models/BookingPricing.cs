using System.ComponentModel.DataAnnotations;

namespace StellarAdmin.ScenarioTesting.Host.Models;

// The model of the _BookingPricing row details in the docs' row details example
public record BookingPricing(
    decimal Total,
    int Travelers,
    [property: Display(Name = "Trip price")] decimal TripPrice
);
