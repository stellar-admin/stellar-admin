using System.ComponentModel.DataAnnotations;
using StellarAdmin.Dashboard.Resources.Editors;

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

public enum GalleryFare
{
    [Display(GroupName = "Saver")]
    Basic,

    [Display(GroupName = "Saver")]
    Classic,

    [Display(GroupName = "Flexible")]
    Flex,

    [Display(Name = "Flex plus", GroupName = "Flexible")]
    FlexPlus,
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
    public static IReadOnlyList<ChoiceItem> Items { get; } =
    [new("aisle", "Aisle"), new("middle", "Middle"), new("window", "Window")];
}

internal static class GalleryDescribedSeats
{
    public static IReadOnlyList<ChoiceItem> Items { get; } =
    [
        new("aisle", "Aisle") { Description = "Easy to stretch and get up." },
        new("middle", "Middle"),
        new("window", "Window") { Description = "A view and somewhere to lean." },
    ];
}

internal static class GalleryAmenities
{
    public static IReadOnlyList<ChoiceItem> Items { get; } =
    [
        new("wifi", "Wi-Fi on board"),
        new("lounge", "Lounge access"),
        new("priority", "Priority boarding"),
        new("baggage", "Extra baggage"),
        new("seat", "Seat selection"),
        new("insurance", "Travel insurance"),
        new("transfer", "Airport transfer"),
    ];
}

internal static class GalleryDays
{
    public static IReadOnlyList<ChoiceItem> Items { get; } =
    [
        new("mon", "Mon"),
        new("tue", "Tue"),
        new("wed", "Wed"),
        new("thu", "Thu"),
        new("fri", "Fri"),
        new("sat", "Sat"),
        new("sun", "Sun"),
    ];
}

internal static class GalleryMeals
{
    public static IReadOnlyList<ChoiceItem> Items { get; } =
    [new("breakfast", "Breakfast"), new("lunch", "Lunch"), new("dinner", "Dinner")];
}
