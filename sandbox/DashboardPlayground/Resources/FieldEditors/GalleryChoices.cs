using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace DashboardPlayground.Resources.FieldEditors;

// Choices shared by the choice editor galleries
public enum GalleryCabin
{
    Economy,

    [Display(Name = "Premium economy", Description = "Extra legroom and priority boarding.")]
    PremiumEconomy,

    [Display(Description = "Lie-flat seats and lounge access.")]
    Business,

    First,
}

[Flags]
public enum GalleryExtras
{
    None = 0,
    Meals = 1,
    Lounge = 2,
    Insurance = 4,
}

internal static class GallerySeats
{
    public static IReadOnlyList<SelectListItem> Items { get; } =
    [new("Aisle", "aisle"), new("Middle", "middle"), new("Window", "window")];
}

internal static class GalleryAmenities
{
    public static IReadOnlyList<SelectListItem> Items { get; } =
    [
        new("Wi-Fi on board", "wifi"),
        new("Lounge access", "lounge"),
        new("Priority boarding", "priority"),
        new("Extra baggage", "baggage"),
        new("Seat selection", "seat"),
        new("Travel insurance", "insurance"),
        new("Airport transfer", "transfer"),
    ];
}

internal static class GalleryDays
{
    public static IReadOnlyList<SelectListItem> Items { get; } =
    [
        new("Mon", "mon"),
        new("Tue", "tue"),
        new("Wed", "wed"),
        new("Thu", "thu"),
        new("Fri", "fri"),
        new("Sat", "sat"),
        new("Sun", "sun"),
    ];
}

internal static class GalleryMeals
{
    public static IReadOnlyList<SelectListItem> Items { get; } =
    [new("Breakfast", "breakfast"), new("Lunch", "lunch"), new("Dinner", "dinner")];
}
