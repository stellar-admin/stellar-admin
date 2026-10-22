using System.Net;
using System.Text.Json;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using StellarAdmin.IntegrationTests.Fixtures;
using StellarAdmin.IntegrationTests.Infrastructure;
using StellarAdmin.Resources.Builders;
using StellarAdmin.Resources.Editors;
using TUnit.Assertions.Enums;
using static StellarAdmin.IntegrationTests.Infrastructure.FormTestHelpers;

namespace StellarAdmin.IntegrationTests.Resources;

public class DependentFieldTests
{
    private const string DependentsUrl =
        "/stellaradmin/products/dependents/7?form=edit&prefix=Entity";
    private const string EditUrl = "/stellaradmin/products/edit/7";

    [Test]
    public async Task EditForm_DependentField_RendersInElementThatNamesItsParents()
    {
        // Arrange
        await using var sut = await CreateHost(new() { CountryId = 1 });
        using var client = sut.GetTestClient();

        // Act
        var document = await client.GetDocumentAsync(EditUrl);

        // Assert
        var field = document.RequiredElement("admin-dependent-field[name='Entity.CityId']");
        await Assert.That(field.GetAttribute("refresh-url")).IsEqualTo(DependentsUrl);
        using var parents = JsonDocument.Parse(field.GetAttribute("parents")!);
        var parent = parents.RootElement.EnumerateArray().Single();
        await Assert.That(parent.GetProperty("name").GetString()).IsEqualTo("Entity.CountryId");
        await Assert.That(parent.GetProperty("field").GetString()).IsEqualTo("CountryId");
        await Assert.That(parent.GetProperty("label").GetString()).IsEqualTo("Country");
        await Assert
            .That(document.QuerySelector("admin-dependent-field[name='Entity.CountryId']"))
            .IsNull();
    }

    [Test]
    public async Task EditForm_ParentHasValue_LoadsChoicesFromTheForm()
    {
        // Arrange
        await using var sut = await CreateHost(new() { CountryId = 1, CityId = 11 });
        using var client = sut.GetTestClient();

        // Act
        var document = await client.GetDocumentAsync(EditUrl);

        // Assert
        var select = document.RequiredElement("select[name='Entity.CityId']");
        await Assert.That(select.HasAttribute("disabled")).IsFalse();
        await Assert
            .That(select.TextContents("option"))
            .IsEquivalentTo(["Not set", "Kyoto", "Osaka"], CollectionOrdering.Matching);
    }

    [Test]
    public async Task EditForm_ParentEmpty_DisablesSelectWithGeneratedTextAndClearsValue()
    {
        // Arrange
        await using var sut = await CreateHost(new() { CityId = 11 });
        using var client = sut.GetTestClient();

        // Act
        var document = await client.GetDocumentAsync(EditUrl);

        // Assert
        var select = document.RequiredElement("select[name='Entity.CityId']");
        await Assert.That(select.HasAttribute("disabled")).IsTrue();
        await Assert.That(select.TextContents("option")).IsEquivalentTo(["Select Country first"]);
        await Assert
            .That(
                document
                    .RequiredElement("input[type='hidden'][name='Entity.CityId']")
                    .GetAttribute("value")
            )
            .IsEqualTo("");
    }

    [Test]
    public async Task EditForm_ParentEmpty_ShowsEmptyTextInPlaceOfTheChoiceGroup()
    {
        // Arrange
        await using var sut = await CreateHost(new() { CountryId = 1, StopIds = [101] });
        using var client = sut.GetTestClient();

        // Act
        var document = await client.GetDocumentAsync(EditUrl);

        // Assert
        var field = document.RequiredElement("admin-dependent-field[name='Entity.StopIds']");
        await Assert
            .That(field.RequiredElement(".sa-dependent-empty").TextContent.Trim())
            .IsEqualTo("Choose a city to see its stops");
        await Assert.That(field.QuerySelector("input[type='checkbox']")).IsNull();
        await Assert
            .That(
                field
                    .RequiredElement("input[name='__sa_checkbox_group.Entity.StopIds']")
                    .GetAttribute("value")
            )
            .IsEqualTo("true");
    }

    [Test]
    public async Task EditForm_ParentEmpty_DisablesLookupButton()
    {
        // Arrange
        await using var sut = await CreateHost(new());
        using var client = sut.GetTestClient();

        // Act
        var document = await client.GetDocumentAsync(EditUrl);

        // Assert
        var choose = document.RequiredElement("#Entity_PlaceId-choose");
        await Assert.That(choose.HasAttribute("disabled")).IsTrue();
        await Assert.That(choose.TextContent.Trim()).IsEqualTo("Select Country first");
    }

    [Test]
    public async Task Dependents_ParentChanged_RendersDependentsWithClearedValues()
    {
        // Arrange
        await using var sut = await CreateHost(new() { CountryId = 1, CityId = 11 });
        using var client = sut.GetTestClient();
        var values = await PrepareForm(client, EditUrl);
        values["changed"] = "CountryId";
        values["Model.CountryId"] = "2";
        values["Model.CityId"] = "11";
        using var content = new FormUrlEncodedContent(values);

        // Act
        using var response = await client.PostAsync(DependentsUrl, content);

        // Assert
        await Assert.That(response.StatusCode).IsEqualTo(HttpStatusCode.OK);
        var document = await response.ReadDocumentAsync();
        await Assert
            .That(
                document
                    .QuerySelectorAll("admin-dependent-field")
                    .Select(field => field.GetAttribute("name")!)
            )
            .IsEquivalentTo(
                ["Entity.CityId", "Entity.PlaceId", "Entity.StopIds"],
                CollectionOrdering.Matching
            );
        var select = document.RequiredElement("select[name='Entity.CityId']");
        await Assert
            .That(select.TextContents("option"))
            .IsEquivalentTo(["Not set", "Valparaíso"], CollectionOrdering.Matching);
        await Assert.That(select.QuerySelector("option[selected]")).IsNull();
        await Assert
            .That(
                document
                    .RequiredElement(
                        "admin-dependent-field[name='Entity.StopIds'] .sa-dependent-empty"
                    )
                    .TextContent.Trim()
            )
            .IsEqualTo("Choose a city to see its stops");
    }

    [Test]
    public async Task Dependents_UnknownField_ReturnsNotFound()
    {
        // Arrange
        await using var sut = await CreateHost(new());
        using var client = sut.GetTestClient();
        var values = await PrepareForm(client, EditUrl);
        values["changed"] = "PlaceId";
        using var content = new FormUrlEncodedContent(values);

        // Act
        using var response = await client.PostAsync(DependentsUrl, content);

        // Assert
        await Assert.That(response.StatusCode).IsEqualTo(HttpStatusCode.NotFound);
    }

    [Test]
    public async Task LookupSheet_ParentValues_PassToTheSearch()
    {
        // Arrange
        await using var sut = await CreateHost(new());
        using var client = sut.GetTestClient();

        // Act
        var document = await client.GetDocumentAsync(
            "/stellaradmin/products/lookupsheet/7?form=edit&field=PlaceId&for=Entity_PlaceId&Model.CountryId=2"
        );

        // Assert
        await Assert
            .That(document.RequiredElement("[data-lookup='search']").GetAttribute("hx-get"))
            .IsEqualTo("/stellaradmin/products/lookup/7?form=edit&field=PlaceId&Model.CountryId=2");
    }

    [Test]
    public async Task Lookup_ParentValues_SearchWithTheArgument()
    {
        // Arrange
        await using var sut = await CreateHost(new());
        using var client = sut.GetTestClient();
        var source = sut.Services.GetRequiredService<PlaceLookupSource>();

        // Act
        var document = await client.GetDocumentAsync(
            "/stellaradmin/products/lookup/7?form=edit&field=PlaceId&Model.CountryId=1"
        );

        // Assert
        await Assert.That(source.SearchedCountries).IsEquivalentTo([1]);
        await Assert
            .That(
                document
                    .QuerySelectorAll("[data-lookup='item']")
                    .Select(item => item.GetAttribute("data-value")!)
            )
            .IsEquivalentTo(["1", "2"], CollectionOrdering.Matching);
    }

    [Test]
    public async Task Lookup_ParentEmpty_ReturnsNotFound()
    {
        // Arrange
        await using var sut = await CreateHost(new());
        using var client = sut.GetTestClient();

        // Act
        using var response = await client.GetAsync(
            "/stellaradmin/products/lookup/7?form=edit&field=PlaceId"
        );

        // Assert
        await Assert.That(response.StatusCode).IsEqualTo(HttpStatusCode.NotFound);
    }

    private static IReadOnlyList<ChoiceItem> Cities(int countryId) =>
        countryId == 1 ? [new("11", "Kyoto"), new("12", "Osaka")] : [new("21", "Valparaíso")];

    private static void ConfigureFields(ResourceFieldsBuilder<TripFieldsModel> fields)
    {
        fields.Add(trip => trip.CountryId).UseEditor<SelectEditor>(UseCountries);
        fields
            .Add(trip => trip.CityId)
            .DependsOn(trip => trip.CountryId)
            .UseEditor<SelectEditor>(select =>
                select.UseItems(
                    (TripFieldsModel trip, IServiceProvider _, CancellationToken _) =>
                        Task.FromResult(Cities(trip.CountryId!.Value))
                )
            );
        fields
            .Add(trip => trip.PlaceId)
            .DependsOn(trip => trip.CountryId)
            .UseEditor<LookupSheetEditor>(lookup =>
                lookup.UseItems<PlaceLookupSource, Place, int, int, TripFieldsModel>(
                    place => place.Id,
                    place => place.Name,
                    trip => trip.CountryId!.Value
                )
            );
        fields
            .Add(trip => trip.StopIds)
            .DependsOn(
                trip => trip.CityId,
                city => city.EmptyText = "Choose a city to see its stops"
            )
            .UseEditor<CheckboxGroupEditor>(group =>
                group.UseItems(
                    (TripFieldsModel trip, IServiceProvider _, CancellationToken _) =>
                        Task.FromResult<IReadOnlyList<ChoiceItem>>([
                            new($"{trip.CityId}1", "Station"),
                            new($"{trip.CityId}2", "Harbour"),
                        ])
                )
            );
    }

    private static Task<WebApplication> CreateHost(
        TripFieldsModel model,
        Action<ResourceFieldsBuilder<TripFieldsModel>>? configureFields = null
    ) =>
        StellarAdminTestHost.CreateAsync(
            new([new(7, "Notebook", 8.50m)]),
            resource =>
            {
                resource.UseKey(product => product.Id);
                resource.AllowEdit<TripFieldsModel, TripFieldsHandler>(edit =>
                    edit.Fields(configureFields ?? ConfigureFields)
                );
            },
            configureServices: services =>
                services
                    .AddSingleton(new TripFieldsState { Model = model })
                    .AddSingleton<PlaceLookupSource>()
        );

    private static void UseCountries(SelectEditor select) =>
        select.UseItems([new ChoiceItem("1", "Japan"), new ChoiceItem("2", "Chile")]);
}
