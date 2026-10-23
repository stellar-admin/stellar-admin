using System.Net;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using StellarAdmin.IntegrationTests.Fixtures;
using StellarAdmin.IntegrationTests.Infrastructure;
using StellarAdmin.Resources.Builders;
using StellarAdmin.Resources.Editors;
using static StellarAdmin.IntegrationTests.Infrastructure.FormTestHelpers;

namespace StellarAdmin.IntegrationTests.Resources;

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
            fields.Add(model => model.CategoryIds).UseMultiLookupSheetEditor(UseCategories)
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
            fields.Add(model => model.CategoryIds).UseMultiLookupSheetEditor(UseCategories)
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
                .UseMultiLookupSheetEditor(lookup => UseCategories(lookup, create: false))
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
            fields.Add(model => model.CategoryIds).UseMultiLookupSheetEditor(UseCategories)
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
            fields.Add(model => model.CategoryIds).UseMultiLookupSheetEditor(UseCategories)
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
    public async Task LookupSheet_NoResourceForItems_LogsProblem()
    {
        // Arrange
        await using var sut = await CreateHost(
            fields =>
                fields
                    .Add(model => model.CategoryIds)
                    .UseMultiLookupSheetEditor(UseCategories),
            addCategoryResource: false
        );
        using var client = sut.GetTestClient();

        // Act
        using var response = await client.GetAsync(LookupSheetUrl);

        // Assert
        var logs = sut.Services.GetRequiredService<RecordingLoggerProvider>();
        await Assert.That(response.StatusCode).IsEqualTo(HttpStatusCode.OK);
        await Assert
            .That(
                logs.ConfigurationProblems.Any(log =>
                    log.Message.Contains(
                        "The MultiLookupSheetEditor on CategoryIds has no New button because no resource is registered for Category."
                    )
                )
            )
            .IsTrue();
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
                    .UseMultiLookupSheetEditor(UseCategories),
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
        StellarAdminTestHost.CreateAsync(
            new([new(7, "Notebook", 8.50m)]),
            resource =>
            {
                resource.UseKey(product => product.Id);
                resource.AllowEdit<MultiLookupFieldsModel, MultiLookupFieldsHandler>(edit =>
                    edit.Fields(configureFields)
                );
            },
            admin =>
            {
                if (addCategoryResource)
                {
                    admin.AddCategoryResource(configureCategories);
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

    private static void UseCategories<TModel>(MultiLookupSheetEditor<TModel> lookup) =>
        UseCategories(lookup, create: true);

    private static void UseCategories<TModel>(MultiLookupSheetEditor<TModel> lookup, bool create)
    {
        lookup.UseSource<CategoryLookupSource>();
        if (create)
        {
            lookup.EnableCreate();
        }
    }
}
