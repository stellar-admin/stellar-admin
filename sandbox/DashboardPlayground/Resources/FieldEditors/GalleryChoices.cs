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

// Choices with each kind of media, for the media sections of the choice editor galleries
internal static class GalleryTransport
{
    public static IReadOnlyList<ChoiceItem> Items { get; } =
    [
        new("flight", "Flight")
        {
            Description = "Fastest between cities.",
            Media = new ItemMedia.Icon("plane"),
        },
        new("train", "Train")
        {
            Description = "City centre to city centre.",
            Media = new ItemMedia.Icon("train-front"),
        },
        new("ferry", "Ferry")
        {
            Description = "Island hops and coastal routes.",
            Media = new ItemMedia.Icon("ship"),
        },
        new("car", "Car hire")
        {
            Description = "Pick up at the airport.",
            Media = new ItemMedia.Icon("car"),
        },
    ];
}

internal static class GalleryGuides
{
    public static IReadOnlyList<ChoiceItem> Items { get; } =
    [
        new("ana", "Ana Ribeiro")
        {
            Description = "Lisbon walking tours.",
            Media = new ItemMedia.Avatar("/images/avatar-1.jpg"),
        },
        new("thabo", "Thabo Nkosi")
        {
            Description = "Cape Peninsula drives.",
            Media = new ItemMedia.Avatar("/images/avatar-2.jpg"),
        },
        new("yuki", "Yuki Tanaka")
        {
            Description = "Kyoto temples and gardens.",
            Media = new ItemMedia.Avatar("/images/avatar-3.jpg"),
        },
        new("lena", "Lena Fischer")
        {
            Description = "No photo yet, so her initials show.",
            Media = new ItemMedia.Avatar(null),
        },
    ];
}

internal static class GalleryCities
{
    public static IReadOnlyList<ChoiceItem> Items { get; } =
    [
        new("lisbon", "Lisbon")
        {
            Description = "Trams, tiles and pastéis.",
            Media = new ItemMedia.Image("/images/lisbon.jpg"),
        },
        new("cape-town", "Cape Town")
        {
            Description = "Table Mountain views.",
            Media = new ItemMedia.Image("/images/cape-town.jpg"),
        },
        new("kyoto", "Kyoto")
        {
            Description = "Temples and tea houses.",
            Media = new ItemMedia.Image("/images/kyoto.jpg"),
        },
        new("chiang-mai", "Chiang Mai")
        {
            Description = "Night markets and hills.",
            Media = new ItemMedia.Image("/images/chiang-mai.jpg"),
        },
    ];
}

internal static class GalleryAirportCodes
{
    public static IReadOnlyList<ChoiceItem> Items { get; } =
    [
        new("LIS", "Lisbon")
        {
            Description = "Humberto Delgado.",
            Media = new ItemMedia.Code("LIS"),
        },
        new("CPT", "Cape Town")
        {
            Description = "Cape Town International.",
            Media = new ItemMedia.Code("CPT"),
        },
        new("KIX", "Osaka")
        {
            Description = "Kansai International.",
            Media = new ItemMedia.Code("KIX"),
        },
        new("CNX", "Chiang Mai")
        {
            Description = "Chiang Mai International.",
            Media = new ItemMedia.Code("CNX"),
        },
    ];
}
