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
        "/stellaradmin/categories/createsheet?for=Entity_CategoryId";

    [Test]
    public async Task NewButton_LoadsReferencedResourceCreateFormIntoSheet()
    {
        // Arrange
        await using var sut = await CreateHost(fields =>
            fields.Add(model => model.CategoryId).UseEditor<LookupEditor>(UseCategories)
        );
        using var client = sut.GetTestClient();

        // Act
        var document = await client.GetDocumentAsync("/stellaradmin/products/create");

        // Assert
        var create = document.RequiredElement("#Entity_CategoryId-create");
        await Assert.That(create.GetAttribute("data-lookup")).IsEqualTo("create");
        await Assert.That(create.GetAttribute("commandfor")).IsEqualTo("dashboard-sheet");
        await Assert.That(create.GetAttribute("command")).IsEqualTo("show-modal");
        await Assert.That(create.GetAttribute("hx-get")).IsEqualTo(CreateSheetUrl);
        await Assert.That(create.GetAttribute("hx-target")).IsEqualTo("#dashboard-sheet-content");
        await Assert
            .That(document.RequiredElement("dashboard-lookup-editor").GetAttribute("selection-url"))
            .IsEqualTo("/stellaradmin/products/lookupselection?form=create&field=CategoryId");
    }

    [Test]
    public async Task NewButton_ExplicitResource_UsesThatResource()
    {
        // Arrange
        await using var sut = await CreateHost(fields =>
            fields
                .Add(model => model.CategoryId)
                .UseEditor<LookupEditor>(lookup =>
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
            .That(document.RequiredElement("#Entity_CategoryId-create").GetAttribute("hx-get"))
            .IsEqualTo(CreateSheetUrl);
    }

    [Test]
    public async Task EnableCreate_UntypedItems_Throws()
    {
        // Arrange
        await using var sut = await CreateHost(fields =>
            fields
                .Add(model => model.CategoryId)
                .UseEditor<LookupEditor>(lookup =>
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
            fields => fields.Add(model => model.CategoryId).UseEditor<LookupEditor>(UseCategories),
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
            fields => fields.Add(model => model.CategoryId).UseEditor<LookupEditor>(UseCategories),
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
            fields => fields.Add(model => model.CategoryId).UseEditor<LookupEditor>(UseCategories),
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
            .That(document.RequiredElement("dashboard-lookup-editor").HasAttribute("selection-url"))
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
        await Assert.That(form.GetAttribute("hx-target")).IsEqualTo("#dashboard-sheet-content");
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
    public async Task CreateSheet_LookupField_OpensNestedSheetWithoutNew()
    {
        // Arrange
        await using var sut = await CreateHost(fields =>
            fields.Add(model => model.CategoryId).UseEditor<LookupEditor>(UseCategories)
        );
        using var client = sut.GetTestClient();

        // Act
        var document = await client.GetDocumentAsync(
            "/stellaradmin/products/createsheet?for=Entity_ProductId"
        );

        // Assert
        var open = document.RequiredElement("#Sheet_CategoryId-choose");
        await Assert.That(open.GetAttribute("commandfor")).IsEqualTo("dashboard-nested-sheet");
        await Assert
            .That(open.GetAttribute("hx-target"))
            .IsEqualTo("#dashboard-nested-sheet-content");
        await Assert
            .That(open.GetAttribute("hx-get"))
            .IsEqualTo(
                "/stellaradmin/products/lookupsheet?form=create&field=CategoryId&for=Sheet_CategoryId"
            );
        await Assert.That(document.QuerySelector("[data-lookup='create']")).IsNull();
        await Assert
            .That(document.RequiredElement("dashboard-lookup-editor").HasAttribute("selection-url"))
            .IsFalse();
        var picker = await client.GetDocumentAsync(open.GetAttribute("hx-get")!);
        await Assert
            .That(picker.RequiredElement("dashboard-lookup-picker").GetAttribute("for"))
            .IsEqualTo("Sheet_CategoryId");
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
    [Arguments("/stellaradmin/categories/createsheet")]
    [Arguments("/stellaradmin/inventory-items/createsheet?for=Entity_CategoryId")]
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
        var url = "/stellaradmin/products/createsheet?for=Entity_ProductId";
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
            fields.Add(model => model.CategoryId).UseEditor<LookupEditor>(UseCategories)
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
            fields.Add(model => model.CategoryId).UseEditor<LookupEditor>(UseCategories);
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

    private static void UseCategories(LookupEditor lookup)
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

        public override LookupMediaType? MediaType => null;

        public override Task<LookupItem?> FindAsync(
            IServiceProvider services,
            FieldEditorContext context,
            CancellationToken cancellationToken
        ) => Task.FromResult<LookupItem?>(null);

        public override Task<LookupResults> SearchAsync(
            IServiceProvider services,
            LookupQuery query,
            CancellationToken cancellationToken
        ) => Task.FromResult(new LookupResults([], false));
    }
}
