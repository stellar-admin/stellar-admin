using System.Net;
using AngleSharp.Html.Dom;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using StellarAdmin.Dashboard.IntegrationTests.Fixtures;
using StellarAdmin.Dashboard.IntegrationTests.Infrastructure;
using StellarAdmin.Dashboard.Resources.Builders;
using StellarAdmin.Dashboard.Resources.Editors;
using StellarAdmin.TagHelpers;
using static StellarAdmin.Dashboard.IntegrationTests.Infrastructure.FormTestHelpers;

namespace StellarAdmin.Dashboard.IntegrationTests.Resources;

public class LookupCreateTests
{
    private const string CreateSheetUrl =
        "/stellaradmin/categories/createsheet?for=Entity_CategoryId&level=1";

    [Test]
    public async Task NewButton_LoadsReferencedResourceCreateFormIntoSheet()
    {
        // Arrange
        await using var sut = await CreateHost(fields =>
            fields.Add(model => model.CategoryId).UseEditor<LookupSheetEditor>(UseCategories)
        );
        using var client = sut.GetTestClient();

        // Act
        var document = await client.GetDocumentAsync("/stellaradmin/products/create");

        // Assert
        var create = document.RequiredElement("#Entity_CategoryId-create");
        await Assert.That(create.GetAttribute("data-lookup")).IsEqualTo("create");
        await Assert.That(create.GetAttribute("data-sheet-open")).IsEqualTo(CreateSheetUrl);
        await Assert.That(create.HasAttribute("hx-get")).IsFalse();
        await Assert
            .That(
                document
                    .RequiredElement("dashboard-lookup-sheet-editor")
                    .GetAttribute("selection-url")
            )
            .IsEqualTo("/stellaradmin/products/lookupselection?form=create&field=CategoryId");
    }

    [Test]
    public async Task NewButton_ExplicitResource_UsesThatResource()
    {
        // Arrange
        await using var sut = await CreateHost(fields =>
            fields
                .Add(model => model.CategoryId)
                .UseEditor<LookupSheetEditor>(lookup =>
                {
                    lookup.UseItems(new UntypedItems());
                    lookup.EnableCreate<Category>();
                })
        );
        using var client = sut.GetTestClient();

        // Act
        var document = await client.GetDocumentAsync("/stellaradmin/products/create");

        // Assert
        await Assert
            .That(
                document
                    .RequiredElement("#Entity_CategoryId-create")
                    .GetAttribute("data-sheet-open")
            )
            .IsEqualTo(CreateSheetUrl);
    }

    [Test]
    public async Task EnableCreate_UntypedItems_Throws()
    {
        // Arrange
        await using var sut = await CreateHost(fields =>
            fields
                .Add(model => model.CategoryId)
                .UseEditor<LookupSheetEditor>(lookup =>
                {
                    lookup.UseItems(new UntypedItems());
                    lookup.EnableCreate();
                })
        );
        using var client = sut.GetTestClient();

        // Act
        using var response = await client.GetAsync("/stellaradmin/products/create");

        // Assert
        await Assert.That(response.StatusCode).IsEqualTo(HttpStatusCode.InternalServerError);
        await Assert
            .That(await response.Content.ReadAsStringAsync())
            .Contains("Use EnableCreate<TResource>() to select the resource.");
    }

    [Test]
    public async Task EnableCreate_NoResourceForItems_Throws()
    {
        // Arrange
        await using var sut = await CreateHost(
            fields =>
                fields.Add(model => model.CategoryId).UseEditor<LookupSheetEditor>(UseCategories),
            addCategoryResource: false
        );
        using var client = sut.GetTestClient();

        // Act
        using var response = await client.GetAsync("/stellaradmin/products/create");

        // Assert
        await Assert.That(response.StatusCode).IsEqualTo(HttpStatusCode.InternalServerError);
        await Assert
            .That(await response.Content.ReadAsStringAsync())
            .Contains("no resource is registered for Category.");
    }

    [Test]
    public async Task EnableCreate_ResourceWithoutCreateForm_Throws()
    {
        // Arrange
        await using var sut = await CreateHost(
            fields =>
                fields.Add(model => model.CategoryId).UseEditor<LookupSheetEditor>(UseCategories),
            dashboard =>
                dashboard.AddResource<Category>(resource =>
                    resource.UseDataSource<CategoryDataSource>()
                ),
            addCategoryResource: false
        );
        using var client = sut.GetTestClient();

        // Act
        using var response = await client.GetAsync("/stellaradmin/products/create");

        // Assert
        await Assert.That(response.StatusCode).IsEqualTo(HttpStatusCode.InternalServerError);
        await Assert
            .That(await response.Content.ReadAsStringAsync())
            .Contains("the categories resource has no create form.");
    }

    [Test]
    [Arguments(false)]
    [Arguments(true)]
    public async Task EnableCreate_ResourceRequiresAuthorization_ShowsNewButtonToAuthorizedUsers(
        bool authorized
    )
    {
        // Arrange
        await using var sut = await CreateHost(
            fields =>
                fields.Add(model => model.CategoryId).UseEditor<LookupSheetEditor>(UseCategories),
            configureCategories: resource =>
                resource.RequireAuthorization(policy => policy.RequireRole("Catalog")),
            configureServices: TestAuthenticationHandler.Register
        );
        using var client = sut.GetTestClient();
        TestAuthenticationHandler.SignIn(client, "ada", authorized ? ["Catalog"] : []);

        // Act
        var document = await client.GetDocumentAsync("/stellaradmin/products/create");

        // Assert
        await Assert
            .That(document.QuerySelector("[data-lookup='create']") is not null)
            .IsEqualTo(authorized);
        await Assert
            .That(
                document
                    .RequiredElement("dashboard-lookup-sheet-editor")
                    .HasAttribute("selection-url")
            )
            .IsEqualTo(authorized);
    }

    [Test]
    public async Task CreateSheet_RendersCreateFormWithSheetPrefix()
    {
        // Arrange
        await using var sut = await CreateHost(fields => fields.Add(model => model.CategoryId));
        using var client = sut.GetTestClient();

        // Act
        var document = await client.GetDocumentAsync(CreateSheetUrl);

        // Assert
        var sheet = document.RequiredElement("dashboard-create-sheet");
        await Assert.That(sheet.GetAttribute("for")).IsEqualTo("Entity_CategoryId");
        await Assert
            .That(sheet.RequiredElement("[data-slot='sheet-title']").TextContent)
            .IsEqualTo("Create Category");
        var form = sheet.RequiredElement("form");
        await Assert.That(form.GetAttribute("hx-post")).IsEqualTo(CreateSheetUrl);
        await Assert
            .That(form.GetAttribute("hx-target"))
            .IsEqualTo("closest [data-sheet='content']");
        await Assert
            .That(form.QuerySelector("input[name='__RequestVerificationToken']"))
            .IsNotNull();
        await Assert
            .That(form.RequiredElement("input[name='Sheet.Name']").Id)
            .IsEqualTo("Sheet_Name");
        await Assert
            .That(form.RequiredElement("input[name='Sheet.Code']").Id)
            .IsEqualTo("Sheet_Code");
        await Assert.That(document.QuerySelector("[id^='Entity_']")).IsNull();
        await Assert
            .That(form.RequiredElement("[data-create-sheet='error']").HasAttribute("hidden"))
            .IsTrue();
    }

    [Test]
    public async Task CreateSheet_SectionWithLayout_StacksSection()
    {
        // Arrange
        await using var sut = await CreateHost(
            fields => fields.Add(model => model.CategoryId),
            configureCategories: resource =>
                resource.AllowCreate(create =>
                    create.Fields(fields =>
                    {
                        fields.Clear();
                        fields.AddSection(
                            "Details",
                            section =>
                            {
                                section.Layout = FormSectionLayout.Split;
                                section.Add(category => category.Name);
                            }
                        );
                    })
                )
        );
        using var client = sut.GetTestClient();

        // Act
        var document = await client.GetDocumentAsync(CreateSheetUrl);

        // Assert
        await Assert
            .That(
                document.RequiredElement("[data-slot='form-section']").GetAttribute("data-layout")
            )
            .IsEqualTo("stacked");
    }

    [Test]
    public async Task CreateSheet_LookupField_OpensNextLevel()
    {
        // Arrange
        await using var sut = await CreateHost(fields =>
            fields.Add(model => model.CategoryId).UseEditor<LookupSheetEditor>(UseCategories)
        );
        using var client = sut.GetTestClient();

        // Act
        var document = await client.GetDocumentAsync(
            "/stellaradmin/products/createsheet?for=Entity_ProductId&level=1"
        );

        // Assert
        var open = document.RequiredElement("#Sheet_CategoryId-choose");
        await Assert
            .That(open.GetAttribute("data-sheet-open"))
            .IsEqualTo(
                "/stellaradmin/products/lookupsheet?form=create&field=CategoryId&for=Sheet_CategoryId"
            );
        await Assert
            .That(
                document.RequiredElement("#Sheet_CategoryId-create").GetAttribute("data-sheet-open")
            )
            .IsEqualTo("/stellaradmin/categories/createsheet?for=Sheet_CategoryId&level=2");
        await Assert
            .That(
                document
                    .RequiredElement("dashboard-lookup-sheet-editor")
                    .GetAttribute("selection-url")
            )
            .IsEqualTo("/stellaradmin/products/lookupselection?form=create&field=CategoryId");
        var picker = await client.GetDocumentAsync(open.GetAttribute("data-sheet-open")!);
        await Assert
            .That(picker.RequiredElement("dashboard-lookup-picker").GetAttribute("for"))
            .IsEqualTo("Sheet_CategoryId");
    }

    [Test]
    public async Task CreateSheet_SecondLevel_BindsWithLevelPrefix()
    {
        // Arrange
        await using var sut = await CreateHost(fields => fields.Add(model => model.CategoryId));
        using var client = sut.GetTestClient();
        const string url = "/stellaradmin/categories/createsheet?for=Sheet_CategoryId&level=2";

        // Act
        var document = await client.GetDocumentAsync(url);

        // Assert
        var form = document.RequiredElement("dashboard-create-sheet form");
        await Assert.That(form.GetAttribute("hx-post")).IsEqualTo(url);
        await Assert
            .That(form.RequiredElement("input[name='Sheet2.Name']").Id)
            .IsEqualTo("Sheet2_Name");
        await Assert.That(document.QuerySelector("[id^='Sheet_']")).IsNull();
    }

    [Test]
    public async Task CreateSheetPost_SecondLevel_BindsWithLevelPrefix()
    {
        // Arrange
        await using var sut = await CreateHost(fields => fields.Add(model => model.CategoryId));
        using var client = sut.GetTestClient();
        const string url = "/stellaradmin/categories/createsheet?for=Sheet_CategoryId&level=2";
        var values = await PrepareForm(client, url);
        values["Sheet2.Name"] = "Lenses";
        values["Sheet2.Code"] = "LEN";
        using var content = new FormUrlEncodedContent(values);

        // Act
        using var response = await client.PostAsync(url, content);

        // Assert
        var document = await response.ReadDocumentAsync();
        var created = document.RequiredElement("dashboard-lookup-created");
        await Assert.That(created.GetAttribute("for")).IsEqualTo("Sheet_CategoryId");
        await Assert.That(created.GetAttribute("value")).IsEqualTo("3");
    }

    [Test]
    public async Task CreateSheet_ConfiguredLabels_ReplaceSaveErrorText()
    {
        // Arrange
        await using var sut = await CreateHost(
            fields => fields.Add(model => model.CategoryId),
            dashboard =>
                dashboard.ConfigureResourceLabels(labels =>
                    labels.Sheet(sheet => sheet.SaveErrorDescription = "Nothing was saved.")
                )
        );
        using var client = sut.GetTestClient();

        // Act
        var document = await client.GetDocumentAsync(CreateSheetUrl);

        // Assert
        await Assert
            .That(
                document
                    .RequiredElement("[data-create-sheet='error'] [data-slot='alert-description']")
                    .TextContent
            )
            .IsEqualTo("Nothing was saved.");
    }

    [Test]
    [Arguments("/stellaradmin/categories/createsheet?level=1")]
    [Arguments("/stellaradmin/categories/createsheet?for=Entity_CategoryId")]
    [Arguments("/stellaradmin/categories/createsheet?for=Entity_CategoryId&level=0")]
    [Arguments("/stellaradmin/inventory-items/createsheet?for=Entity_CategoryId&level=1")]
    public async Task CreateSheet_WithoutLookupOrCreateForm_ReturnsNotFound(string url)
    {
        // Arrange
        await using var sut = await CreateHost(
            fields => fields.Add(model => model.CategoryId),
            dashboard =>
                dashboard.AddResource<InventoryItem>(resource =>
                    resource.UseDataSource<InventoryItemDataSource>()
                ),
            configureServices: services => services.AddSingleton(new List<InventoryItem>())
        );
        using var client = sut.GetTestClient();

        // Act
        using var response = await client.GetAsync(url);

        // Assert
        await Assert.That(response.StatusCode).IsEqualTo(HttpStatusCode.NotFound);
    }

    [Test]
    public async Task CreateSheetPost_Rejected_ReturnsFormWithErrors()
    {
        // Arrange
        await using var sut = await CreateHost(fields => fields.Add(model => model.CategoryId));
        using var client = sut.GetTestClient();
        var values = await PrepareForm(client, CreateSheetUrl);
        values["Sheet.Name"] = "Compact cameras";
        values["Sheet.Code"] = "CAM";
        using var content = new FormUrlEncodedContent(values);

        // Act
        using var response = await client.PostAsync(CreateSheetUrl, content);

        // Assert
        await Assert.That(response.StatusCode).IsEqualTo(HttpStatusCode.OK);
        var document = await response.ReadDocumentAsync();
        var sheet = document.RequiredElement("dashboard-create-sheet");
        await Assert.That(sheet.GetAttribute("for")).IsEqualTo("Entity_CategoryId");
        await Assert
            .That(sheet.RequiredElement("input[name='Sheet.Name']").GetAttribute("value"))
            .IsEqualTo("Compact cameras");
        await Assert
            .That(sheet.RequiredElement("[data-valmsg-for='Sheet.Code']").TextContent)
            .Contains("The code is already used.");
        await Assert
            .That(sheet.RequiredElement("input[name='Sheet.Code']").GetAttribute("value"))
            .IsEqualTo("CAM");
        await Assert
            .That(sut.Services.GetRequiredService<CategoryStore>().Categories.Count)
            .IsEqualTo(2);
    }

    [Test]
    public async Task CreateSheetPost_InvalidModel_ReturnsFormWithoutCreating()
    {
        // Arrange
        await using var sut = await CreateHost(fields => fields.Add(model => model.CategoryId));
        using var client = sut.GetTestClient();
        var values = await PrepareForm(client, CreateSheetUrl);
        values["Sheet.Code"] = "LEN";
        using var content = new FormUrlEncodedContent(values);

        // Act
        using var response = await client.PostAsync(CreateSheetUrl, content);

        // Assert
        var document = await response.ReadDocumentAsync();
        await Assert
            .That(document.RequiredElement("[data-valmsg-for='Sheet.Name']").TextContent)
            .IsNotEmpty();
        await Assert.That(document.QuerySelector("dashboard-lookup-created")).IsNull();
        await Assert
            .That(sut.Services.GetRequiredService<CategoryStore>().Categories.Count)
            .IsEqualTo(2);
    }

    [Test]
    public async Task CreateSheetPost_Valid_ReturnsKeyFromResource()
    {
        // Arrange
        await using var sut = await CreateHost(fields => fields.Add(model => model.CategoryId));
        using var client = sut.GetTestClient();
        var values = await PrepareForm(client, CreateSheetUrl);
        values["Sheet.Name"] = "Lenses";
        values["Sheet.Code"] = "LEN";
        using var content = new FormUrlEncodedContent(values);

        // Act
        using var response = await client.PostAsync(CreateSheetUrl, content);

        // Assert
        await Assert.That(response.StatusCode).IsEqualTo(HttpStatusCode.OK);
        var document = await response.ReadDocumentAsync();
        var created = document.RequiredElement("dashboard-lookup-created");
        await Assert.That(created.GetAttribute("for")).IsEqualTo("Entity_CategoryId");
        await Assert.That(created.GetAttribute("value")).IsEqualTo("3");
        var category = sut.Services.GetRequiredService<CategoryStore>().Categories[^1];
        await Assert.That(category.Name).IsEqualTo("Lenses");
        await Assert.That(category.Code).IsEqualTo("LEN");
    }

    [Test]
    public async Task CreateSheetPost_CustomModel_ReturnsKeyFromHandler()
    {
        // Arrange
        await using var sut = await CreateHost(
            fields => fields.Add(model => model.CategoryId),
            dashboard =>
                dashboard.AddResource<Category>(resource =>
                {
                    resource.UseDataSource<CategoryDataSource>();
                    resource.AllowCreate<CreateCategoryModel, CreateCategoryHandler>(create =>
                        create.Fields(fields => fields.Add(model => model.Name))
                    );
                }),
            addCategoryResource: false
        );
        using var client = sut.GetTestClient();
        var values = await PrepareForm(client, CreateSheetUrl);
        values["Sheet.Name"] = "Tripods";
        using var content = new FormUrlEncodedContent(values);

        // Act
        using var response = await client.PostAsync(CreateSheetUrl, content);

        // Assert
        var document = await response.ReadDocumentAsync();
        await Assert
            .That(document.RequiredElement("dashboard-lookup-created").GetAttribute("value"))
            .IsEqualTo("3");
    }

    [Test]
    public async Task CreateSheetPost_NoKey_ReturnsCreatedWithoutValue()
    {
        // Arrange
        await using var sut = await CreateHost(fields => fields.Add(model => model.CategoryId));
        using var client = sut.GetTestClient();
        var url = "/stellaradmin/products/createsheet?for=Entity_ProductId&level=1";
        var values = await PrepareForm(client, url);
        using var content = new FormUrlEncodedContent(values);

        // Act
        using var response = await client.PostAsync(url, content);

        // Assert
        var document = await response.ReadDocumentAsync();
        var created = document.RequiredElement("dashboard-lookup-created");
        await Assert.That(created.GetAttribute("for")).IsEqualTo("Entity_ProductId");
        await Assert.That(created.HasAttribute("value")).IsFalse();
    }

    [Test]
    public async Task LookupSelection_Value_RendersItemDisplay()
    {
        // Arrange
        await using var sut = await CreateHost(fields =>
            fields.Add(model => model.CategoryId).UseEditor<LookupSheetEditor>(UseCategories)
        );
        using var client = sut.GetTestClient();

        // Act
        var document = await client.GetDocumentAsync(
            "/stellaradmin/products/lookupselection?form=create&field=CategoryId&value=2"
        );

        // Assert
        var template = (IHtmlTemplateElement)
            document.RequiredElement("template[data-lookup='selection']");
        await Assert.That(template.GetAttribute("data-value")).IsEqualTo("2");
        await Assert
            .That(template.Content.QuerySelector("[data-lookup='title']")?.TextContent)
            .IsEqualTo("Notebooks");
        await Assert
            .That(template.Content.QuerySelector("[data-slot='item-description']")?.TextContent)
            .IsEqualTo("NTB");
    }

    [Test]
    [Arguments("form=create&field=CategoryId")]
    [Arguments("form=create&field=CategoryId&value=abc")]
    [Arguments("form=create&field=PrimaryCategoryId&value=2")]
    [Arguments("form=edit&field=CategoryId&value=2")]
    public async Task LookupSelection_UnknownFieldOrValue_ReturnsNotFound(string query)
    {
        // Arrange
        await using var sut = await CreateHost(fields =>
        {
            fields.Add(model => model.CategoryId).UseEditor<LookupSheetEditor>(UseCategories);
            fields.Add(model => model.PrimaryCategoryId);
        });
        using var client = sut.GetTestClient();

        // Act
        using var response = await client.GetAsync(
            $"/stellaradmin/products/lookupselection?{query}"
        );

        // Assert
        await Assert.That(response.StatusCode).IsEqualTo(HttpStatusCode.NotFound);
    }

    private static Task<WebApplication> CreateHost(
        Action<ResourceFieldsBuilder<LookupFieldsModel>> configureFields,
        Action<StellarAdminDashboardBuilder>? configureDashboard = null,
        Action<ResourceBuilder<Category>>? configureCategories = null,
        Action<IServiceCollection>? configureServices = null,
        bool addCategoryResource = true
    ) =>
        DashboardTestHost.CreateAsync(
            new([]),
            resource =>
                resource.AllowCreate<LookupFieldsModel, LookupFieldsHandler>(create =>
                    create.Fields(configureFields)
                ),
            dashboard =>
            {
                if (addCategoryResource)
                {
                    dashboard.AddCategoryResource(configureCategories);
                }

                configureDashboard?.Invoke(dashboard);
            },
            services =>
            {
                services.AddSingleton<CategoryStore>().AddScoped<CategoryLookupSource>();
                configureServices?.Invoke(services);
            }
        );

    private static void UseCategories(LookupSheetEditor lookup)
    {
        lookup.UseItems<CategoryLookupSource, Category, int>(
            category => category.Id,
            category => category.Name,
            items => items.UseDescription(category => category.Code)
        );
        lookup.EnableCreate();
    }

    // Items that don't report their type, as a custom integration's might not
    private sealed class UntypedItems : LookupItems
    {
        public override bool HasDescription => false;

        public override Type? MediaType => null;

        public override Task<IReadOnlyList<ChoiceItem>> FindAsync(
            IServiceProvider services,
            FieldEditorContext context,
            CancellationToken cancellationToken
        ) => Task.FromResult<IReadOnlyList<ChoiceItem>>([]);

        public override Task<LookupResults> SearchAsync(
            IServiceProvider services,
            LookupQuery query,
            CancellationToken cancellationToken
        ) => Task.FromResult(new LookupResults([], false));
    }
}
