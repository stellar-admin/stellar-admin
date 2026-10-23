using System.ComponentModel.DataAnnotations;
using StellarAdmin.Resources;
using StellarAdmin.Resources.Editors;

namespace StellarAdmin.IntegrationTests.Fixtures;

// A trip whose city depends on its country, and whose stops depend on its city
public sealed class TripFieldsModel
{
    public int? CityId { get; set; }

    [Display(Name = "Country")]
    public int? CountryId { get; set; }

    public int Id { get; set; }

    public int? PlaceId { get; set; }

    public List<int> StopIds { get; set; } = [];
}

// The record the dependent field tests edit; a successful save replaces it
public sealed class TripFieldsState
{
    public TripFieldsModel Model { get; set; } = new();
}

public sealed class TripFieldsHandler(TripFieldsState state) : IResourceEditHandler<TripFieldsModel>
{
    public Task<TripFieldsModel?> FindAsync(string id, CancellationToken cancellationToken) =>
        Task.FromResult<TripFieldsModel?>(
            id == "7"
                ? new()
                {
                    Id = 7,
                    CityId = state.Model.CityId,
                    CountryId = state.Model.CountryId,
                    PlaceId = state.Model.PlaceId,
                    StopIds = [.. state.Model.StopIds],
                }
                : null
        );

    public Task<ResourceOperationResult> UpdateAsync(
        string id,
        TripFieldsModel model,
        CancellationToken cancellationToken
    )
    {
        state.Model = model;

        return Task.FromResult(ResourceOperationResult.Success());
    }
}

public sealed record Place(int Id, string Name, int CountryId);

// Places in Japan (1) and Chile (2), searched by country
public sealed class PlaceLookupSource : LookupSource<Place, int, int>
{
    public static readonly Place[] Places =
    [
        new(1, "Kyoto", 1),
        new(2, "Osaka", 1),
        new(3, "Valparaíso", 2),
    ];

    public List<int> SearchedCountries { get; } = [];

    public override Task<IReadOnlyCollection<Place>> FindAsync(
        IReadOnlyCollection<int> values,
        CancellationToken cancellationToken
    ) =>
        Task.FromResult<IReadOnlyCollection<Place>>(
            Places.Where(place => values.Contains(place.Id)).ToArray()
        );

    public override string GetTitle(Place item) => item.Name;

    public override int GetValue(Place item) => item.Id;

    public override Task<LookupPage<Place>> SearchAsync(
        int countryId,
        LookupQuery query,
        CancellationToken cancellationToken
    )
    {
        SearchedCountries.Add(countryId);

        return Task.FromResult(
            new LookupPage<Place>(
                Places.Where(place => place.CountryId == countryId).ToArray(),
                false
            )
        );
    }
}
