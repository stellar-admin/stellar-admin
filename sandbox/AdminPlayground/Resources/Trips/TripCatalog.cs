using System.Globalization;
using StellarAdmin.Resources.Editors;

namespace AdminPlayground.Resources.Trips;

public sealed record TripCountry(int Id, string Name);

public sealed record TripCity(int Id, int CountryId, string Name);

public sealed record TripPlace(int Id, int CityId, string Name);

public sealed record TripAirport(string Code, int CountryId, string Name);

// The places trips go to: each city is in a country, and each neighbourhood and pickup point in a city
public sealed class TripCatalog
{
    public static readonly TripCountry[] Countries =
    [
        new(1, "Chile"),
        new(2, "Japan"),
        new(3, "Portugal"),
        new(4, "South Africa"),
    ];

    public static readonly TripCity[] Cities =
    [
        new(11, 1, "Santiago"),
        new(12, 1, "Valparaíso"),
        new(21, 2, "Kyoto"),
        new(22, 2, "Osaka"),
        new(23, 2, "Tokyo"),
        new(31, 3, "Lisbon"),
        new(32, 3, "Porto"),
        new(41, 4, "Cape Town"),
    ];

    public static readonly TripPlace[] Neighbourhoods =
    [
        new(111, 11, "Lastarria"),
        new(112, 11, "Bellavista"),
        new(121, 12, "Cerro Alegre"),
        new(122, 12, "Cerro Concepción"),
        new(211, 21, "Gion"),
        new(212, 21, "Arashiyama"),
        new(213, 21, "Higashiyama"),
        new(221, 22, "Namba"),
        new(222, 22, "Umeda"),
        new(231, 23, "Asakusa"),
        new(232, 23, "Shibuya"),
        new(233, 23, "Shinjuku"),
        new(311, 31, "Alfama"),
        new(312, 31, "Bairro Alto"),
        new(313, 31, "Belém"),
        new(321, 32, "Ribeira"),
        new(322, 32, "Foz do Douro"),
        new(411, 41, "Bo-Kaap"),
        new(412, 41, "Camps Bay"),
        new(413, 41, "Woodstock"),
    ];

    public static readonly TripPlace[] PickupPoints =
    [
        new(1101, 11, "Plaza de Armas"),
        new(1201, 12, "Muelle Prat"),
        new(2101, 21, "Kyoto Station, central exit"),
        new(2102, 21, "Gion-Shijō Station"),
        new(2201, 22, "Shin-Ōsaka Station"),
        new(2301, 23, "Tokyo Station, Marunouchi side"),
        new(2302, 23, "Shinjuku Station, south exit"),
        new(3101, 31, "Praça do Comércio"),
        new(3102, 31, "Cais do Sodré"),
        new(3201, 32, "São Bento Station"),
        new(4101, 41, "V&A Waterfront"),
    ];

    public static readonly TripAirport[] Airports =
    [
        new("SCL", 1, "Santiago"),
        new("KIX", 2, "Osaka Kansai"),
        new("ITM", 2, "Osaka Itami"),
        new("HND", 2, "Tokyo Haneda"),
        new("NRT", 2, "Tokyo Narita"),
        new("LIS", 3, "Lisbon"),
        new("OPO", 3, "Porto"),
        new("CPT", 4, "Cape Town"),
    ];

    public static string? CityName(int? id) => Cities.FirstOrDefault(city => city.Id == id)?.Name;

    public static string? CountryName(int? id) =>
        Countries.FirstOrDefault(country => country.Id == id)?.Name;

    public Task<IReadOnlyList<ChoiceItem>> CitiesAsync(
        int countryId,
        CancellationToken cancellationToken
    ) =>
        Task.FromResult<IReadOnlyList<ChoiceItem>>(
            Cities
                .Where(city => city.CountryId == countryId)
                .Select(city => Choice(city.Id, city.Name))
                .ToArray()
        );

    public static IReadOnlyList<ChoiceItem> CountryChoices() =>
        Countries.Select(country => Choice(country.Id, country.Name)).ToArray();

    public Task<IReadOnlyList<ChoiceItem>> NeighbourhoodsAsync(
        int cityId,
        CancellationToken cancellationToken
    ) => PlacesAsync(Neighbourhoods, cityId);

    public Task<IReadOnlyList<ChoiceItem>> PickupPointsAsync(
        int cityId,
        CancellationToken cancellationToken
    ) => PlacesAsync(PickupPoints, cityId);

    private static ChoiceItem Choice(int id, string name) =>
        new(id.ToString(CultureInfo.CurrentCulture), name);

    private static Task<IReadOnlyList<ChoiceItem>> PlacesAsync(TripPlace[] places, int cityId) =>
        Task.FromResult<IReadOnlyList<ChoiceItem>>(
            places
                .Where(place => place.CityId == cityId)
                .Select(place => Choice(place.Id, place.Name))
                .ToArray()
        );
}

// The airports a trip arrives at, searched within the trip's country
public sealed class TripAirportSource : ILookupSource<TripAirport, string, int>
{
    public Task<IReadOnlyCollection<TripAirport>> FindAsync(
        IReadOnlyCollection<string> values,
        CancellationToken cancellationToken
    ) =>
        Task.FromResult<IReadOnlyCollection<TripAirport>>(
            TripCatalog.Airports.Where(airport => values.Contains(airport.Code)).ToArray()
        );

    public Task<LookupPage<TripAirport>> SearchAsync(
        int countryId,
        LookupQuery query,
        CancellationToken cancellationToken
    )
    {
        var matches = TripCatalog
            .Airports.Where(airport =>
                airport.CountryId == countryId
                && (
                    query.Term is null
                    || airport.Name.Contains(query.Term, StringComparison.CurrentCultureIgnoreCase)
                    || airport.Code.Contains(query.Term, StringComparison.OrdinalIgnoreCase)
                )
            )
            .ToArray();

        return Task.FromResult(
            new LookupPage<TripAirport>(
                matches.Skip(query.Skip).Take(query.Take).ToArray(),
                matches.Length > query.Skip + query.Take
            )
        );
    }
}
