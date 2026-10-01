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
