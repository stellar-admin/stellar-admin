using System.Net;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using StellarAdmin.Dashboard.IntegrationTests.Fixtures;
using StellarAdmin.Dashboard.IntegrationTests.Infrastructure;
using StellarAdmin.Dashboard.Resources.Builders;
using StellarAdmin.Dashboard.Resources.Editors;
using static StellarAdmin.Dashboard.IntegrationTests.Infrastructure.FormTestHelpers;

namespace StellarAdmin.Dashboard.IntegrationTests.Resources;

public class MultiLookupCreateTests
{
    private const string CreateSheetUrl =
        "/stellaradmin/categories/createsheet?for=Entity_CategoryIds&level=1";

    private const string LookupSheetUrl =
        "/stellaradmin/products/lookupsheet?form=edit&field=CategoryIds&for=Entity_CategoryIds";

    [Test]
    public async Task LookupSheet_EnableCreate_ShowsNewBesideDone()
    {
        // Arrange
        await using var sut = await CreateHost(fields =>
            fields.Add(model => model.CategoryIds).UseEditor<MultiLookupSheetEditor>(UseCategories)
        );
        using var client = sut.GetTestClient();

        // Act
        var document = await client.GetDocumentAsync(LookupSheetUrl);

        // Assert
        var create = document.RequiredElement("[data-slot='sheet-footer'] [data-lookup='create']");
        await Assert.That(create.GetAttribute("data-sheet-open")).IsEqualTo(CreateSheetUrl);
        await Assert.That(create.GetAttribute("aria-keyshortcuts")).IsEqualTo("Alt+N");
        await Assert.That(create.TextContent.Trim()).IsEqualTo("New");
        await Assert.That(document.QuerySelector("[data-lookup='new-key']")).IsNotNull();
        await Assert
            .That(document.RequiredElement("[data-lookup='search']").GetAttribute("hx-get"))
            .IsEqualTo(
                "/stellaradmin/products/lookup?form=edit&field=CategoryIds&for=Entity_CategoryIds"
            );
    }

    [Test]
    public async Task LookupSheet_FieldInSheet_OpensCreateFormAboveTheLookup()
    {
        // Arrange
        await using var sut = await CreateHost(fields =>
            fields.Add(model => model.CategoryIds).UseEditor<MultiLookupSheetEditor>(UseCategories)
        );
        using var client = sut.GetTestClient();

        // Act
        var document = await client.GetDocumentAsync($"{LookupSheetUrl}&level=1");

        // Assert
        await Assert
            .That(
                document.RequiredElement("[data-lookup='create']").GetAttribute("data-sheet-open")
            )
            .IsEqualTo("/stellaradmin/categories/createsheet?for=Entity_CategoryIds&level=2");
        await Assert
            .That(document.RequiredElement("[data-lookup='search']").GetAttribute("hx-get"))
            .IsEqualTo(
                "/stellaradmin/products/lookup?form=edit&field=CategoryIds&for=Entity_CategoryIds&level=1"
            );
    }

    [Test]
    public async Task LookupSheet_WithoutCreate_HasNoNew()
    {
        // Arrange
        await using var sut = await CreateHost(fields =>
            fields
                .Add(model => model.CategoryIds)
                .UseEditor<MultiLookupSheetEditor>(lookup => UseCategories(lookup, create: false))
        );
        using var client = sut.GetTestClient();

        // Act
        var document = await client.GetDocumentAsync(LookupSheetUrl);

        // Assert
        await Assert.That(document.QuerySelector("[data-lookup='create']")).IsNull();
        await Assert.That(document.QuerySelector("[data-lookup='new-key']")).IsNull();
        await Assert.That(document.QuerySelector("[data-lookup='done']")).IsNotNull();
    }

    [Test]
    public async Task Lookup_NoResults_ShowsNew()
    {
        // Arrange
        await using var sut = await CreateHost(fields =>
            fields.Add(model => model.CategoryIds).UseEditor<MultiLookupSheetEditor>(UseCategories)
        );
        using var client = sut.GetTestClient();

        // Act
        var document = await client.GetDocumentAsync(
            "/stellaradmin/products/lookup?form=edit&field=CategoryIds&for=Entity_CategoryIds&term=lenses"
        );

        // Assert
        await Assert
            .That(
                document
                    .RequiredElement("[data-lookup='message'] [data-lookup='create']")
                    .GetAttribute("data-sheet-open")
            )
            .IsEqualTo(CreateSheetUrl);
    }

    [Test]
    [Arguments("term=cam")]
    [Arguments("term=lenses&selectedOnly=true")]
    public async Task Lookup_ResultsOrSelectedView_HasNoNew(string query)
    {
        // Arrange
        await using var sut = await CreateHost(fields =>
            fields.Add(model => model.CategoryIds).UseEditor<MultiLookupSheetEditor>(UseCategories)
        );
        using var client = sut.GetTestClient();

        // Act
        var document = await client.GetDocumentAsync(
            $"/stellaradmin/products/lookup?form=edit&field=CategoryIds&for=Entity_CategoryIds&{query}"
        );

        // Assert
        await Assert.That(document.QuerySelector("[data-lookup='create']")).IsNull();
    }

    [Test]
    public async Task LookupSheet_NoResourceForItems_Throws()
    {
        // Arrange
        await using var sut = await CreateHost(
            fields =>
                fields
                    .Add(model => model.CategoryIds)
                    .UseEditor<MultiLookupSheetEditor>(UseCategories),
            addCategoryResource: false
        );
        using var client = sut.GetTestClient();

        // Act
        using var response = await client.GetAsync(LookupSheetUrl);

        // Assert
        await Assert.That(response.StatusCode).IsEqualTo(HttpStatusCode.InternalServerError);
        await Assert
            .That(await response.Content.ReadAsStringAsync())
            .Contains(
                "MultiLookupSheetEditor on CategoryIds enables create, but no resource is registered for Category."
            );
    }

    [Test]
    [Arguments(false)]
    [Arguments(true)]
    public async Task LookupSheet_ResourceRequiresAuthorization_ShowsNewToAuthorizedUsers(
        bool authorized
    )
    {
        // Arrange
        await using var sut = await CreateHost(
            fields =>
                fields
                    .Add(model => model.CategoryIds)
                    .UseEditor<MultiLookupSheetEditor>(UseCategories),
            resource => resource.RequireAuthorization(policy => policy.RequireRole("Catalog")),
            configureServices: TestAuthenticationHandler.Register
        );
        using var client = sut.GetTestClient();
        TestAuthenticationHandler.SignIn(client, "ada", authorized ? ["Catalog"] : []);

        // Act
        var document = await client.GetDocumentAsync(LookupSheetUrl);

        // Assert
        await Assert
            .That(document.QuerySelector("[data-lookup='create']") is not null)
            .IsEqualTo(authorized);
    }

    private static Task<WebApplication> CreateHost(
        Action<ResourceFieldsBuilder<MultiLookupFieldsModel>> configureFields,
        Action<ResourceBuilder<Category>>? configureCategories = null,
        Action<IServiceCollection>? configureServices = null,
        bool addCategoryResource = true
    ) =>
        DashboardTestHost.CreateAsync(
            new([new(7, "Notebook", 8.50m)]),
            resource =>
            {
                resource.UseKey(product => product.Id);
                resource.AllowEdit<MultiLookupFieldsModel, MultiLookupFieldsHandler>(edit =>
                    edit.Fields(configureFields)
                );
            },
            dashboard =>
            {
                if (addCategoryResource)
                {
                    dashboard.AddCategoryResource(configureCategories);
                }
            },
            services =>
            {
                services
                    .AddSingleton(new MultiLookupFieldsState())
                    .AddSingleton<CategoryStore>()
                    .AddScoped<CategoryLookupSource>();
                configureServices?.Invoke(services);
            }
        );

    private static void UseCategories(MultiLookupSheetEditor lookup) =>
        UseCategories(lookup, create: true);

    private static void UseCategories(MultiLookupSheetEditor lookup, bool create)
    {
        lookup.UseItems<CategoryLookupSource, Category, int>(
            category => category.Id,
            category => category.Name
        );
        if (create)
        {
            lookup.EnableCreate();
        }
    }
}
