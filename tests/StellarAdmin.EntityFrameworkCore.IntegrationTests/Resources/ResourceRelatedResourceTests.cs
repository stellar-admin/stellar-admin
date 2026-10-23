using System.Collections.Concurrent;
using System.Net;
using AngleSharp.Dom;
using AngleSharp.Html.Parser;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using StellarAdmin.EntityFrameworkCore.IntegrationTests.Fixtures;
using StellarAdmin.EntityFrameworkCore.IntegrationTests.Infrastructure;
using StellarAdmin.Resources;
using StellarAdmin.Resources.Displays;
using StellarAdmin.Resources.Editors;
using TUnit.Assertions.Enums;

namespace StellarAdmin.EntityFrameworkCore.IntegrationTests.Resources;

// Fields that take their items from the related entity's resource
public class ResourceRelatedResourceTests
{
    [Test]
    public async Task ForeignKeySelect_WithoutItems_TakesCategoriesOrderedByTitle()
    {
        // Arrange
        await using var sut = await EfCoreTestHost.CreateAsync(
            resource =>
                resource.AllowCreate(create =>
                    create.Fields(fields =>
                        fields.Add(product => product.CategoryId).UseEditor<SelectEditor>()
                    )
                ),
            configureAdmin: admin => admin.AddEfCoreResource<CatalogDbContext, Category>()
        );
        using var client = sut.GetTestClient();

        // Act
        var document = await GetDocument(client, "/stellaradmin/products/create");

        // Assert
        await Assert
            .That(OptionValues(document, "Entity.CategoryId"))
            .IsEquivalentTo(["2", "1", "3"], CollectionOrdering.Matching);
    }

    [Test]
    public async Task ForeignKeyLookup_OnEdit_DisplaysSelectionFromReferenceLoadedWithEntity()
    {
        // Arrange
        var commands = new ConcurrentQueue<string>();
        await using var sut = await EfCoreTestHost.CreateAsync(
            resource =>
                resource.AllowEdit(edit =>
                    edit.Fields(fields =>
                        fields.Add(product => product.CategoryId).UseEditor<LookupSheetEditor>()
                    )
                ),
            commands,
            admin =>
                admin.AddEfCoreResource<CatalogDbContext, Category>(category =>
                    category.Reference(reference =>
                        reference.UseDescription(item => "Category #" + item.Id)
                    )
                )
        );
        using var client = sut.GetTestClient();

        // Act
        var document = await GetDocument(client, "/stellaradmin/products/edit/1");

        // Assert
        var selected = document.QuerySelector("#Entity_CategoryId-selected")!;
        await Assert
            .That(selected.QuerySelector("[data-slot='item-title']")!.TextContent.Trim())
            .IsEqualTo("Office");
        await Assert
            .That(selected.QuerySelector("[data-slot='item-description']")!.TextContent)
            .IsEqualTo("Category #1");
        await Assert.That(commands.Count).IsEqualTo(1);
        await Assert.That(commands.Single()).Contains("JOIN \"Category\"");
    }

    [Test]
    public async Task ForeignKeyLookup_EnableCreate_OpensCategoryCreateForm()
    {
        // Arrange
        await using var sut = await EfCoreTestHost.CreateAsync(
            resource =>
                resource.AllowCreate(create =>
                    create.Fields(fields =>
                        fields
                            .Add(product => product.CategoryId)
                            .UseEditor<LookupSheetEditor>(lookup => lookup.EnableCreate())
                    )
                ),
            configureAdmin: admin =>
                admin.AddEfCoreResource<CatalogDbContext, Category>(category =>
                    category.AllowCreate()
                )
        );
        using var client = sut.GetTestClient();

        // Act
        var document = await GetDocument(client, "/stellaradmin/products/create");

        // Assert
        await Assert
            .That(
                document.QuerySelector("#Entity_CategoryId-create")?.GetAttribute("data-sheet-open")
            )
            .IsEqualTo("/stellaradmin/categories/createsheet?for=Entity_CategoryId&level=1");
    }

    [Test]
    public async Task ManyToManyCheckboxGroup_WithoutItems_TakesTags()
    {
        // Arrange
        await using var sut = await EfCoreTestHost.CreateAsync(
            resource =>
                resource.AllowEdit(edit =>
                    edit.Fields(fields =>
                        fields.Add(product => product.Tags).UseEditor<CheckboxGroupEditor>()
                    )
                ),
            configureAdmin: admin => admin.AddEfCoreResource<CatalogDbContext, Tag>()
        );
        using var client = sut.GetTestClient();

        // Act
        var document = await GetDocument(client, "/stellaradmin/products/edit/2");

        // Assert
        var checkboxes = document.QuerySelectorAll("input[name='Entity.Tags']");
        await Assert
            .That(checkboxes.Select(checkbox => checkbox.GetAttribute("value")))
            .IsEquivalentTo(["3", "1", "2"], CollectionOrdering.Matching);
        await Assert
            .That(
                document
                    .QuerySelectorAll("[data-slot='field-label']")
                    .Select(label => label.TextContent.Trim())
            )
            .Contains("Tags");
        await Assert
            .That(
                checkboxes
                    .Where(checkbox => checkbox.HasAttribute("checked"))
                    .Select(checkbox => checkbox.GetAttribute("value"))
            )
            .IsEquivalentTo(["1", "2"]);
    }

    [Test]
    public async Task ForeignKeyLookup_WithoutResource_HasNoItemsAndLogsProblem()
    {
        // Arrange
        await using var sut = await EfCoreTestHost.CreateAsync(resource =>
            resource.AllowEdit(edit =>
                edit.Fields(fields =>
                    fields.Add(product => product.CategoryId).UseEditor<LookupSheetEditor>()
                )
            )
        );
        using var client = sut.GetTestClient();

        // Act
        using var response = await client.GetAsync("/stellaradmin/products/edit/1");

        // Assert
        var logs = sut.Services.GetRequiredService<RecordingLoggerProvider>();
        await Assert.That(response.StatusCode).IsEqualTo(HttpStatusCode.OK);
        await Assert
            .That(
                logs.ConfigurationProblems.Any(log =>
                    log.Message.Contains("no resource is registered for Category")
                )
            )
            .IsTrue();
    }

    [Test]
    public async Task ForeignKey_WithoutEditor_UsesLookupSheetOfCategoryResource()
    {
        // Arrange
        await using var sut = await EfCoreTestHost.CreateAsync(
            resource =>
                resource.AllowEdit(edit =>
                    edit.Fields(fields => fields.Add(product => product.CategoryId))
                ),
            configureAdmin: admin => admin.AddEfCoreResource<CatalogDbContext, Category>()
        );
        using var client = sut.GetTestClient();

        // Act
        var document = await GetDocument(client, "/stellaradmin/products/edit/1");

        // Assert
        await Assert
            .That(
                document
                    .QuerySelector("#Entity_CategoryId-selected [data-lookup='title']")
                    ?.TextContent.Trim()
            )
            .IsEqualTo("Office");
        await Assert
            .That(
                document
                    .QuerySelectorAll("[data-slot='field-label']")
                    .Select(label => label.TextContent.Trim())
            )
            .Contains("Category");
    }

    [Test]
    public async Task ForeignKey_WithoutEditorOrResource_KeepsNumberInputAndLogsInfo()
    {
        // Arrange
        await using var sut = await EfCoreTestHost.CreateAsync(resource =>
            resource.AllowEdit(edit =>
                edit.Fields(fields => fields.Add(product => product.CategoryId))
            )
        );
        using var client = sut.GetTestClient();

        // Act
        var document = await GetDocument(client, "/stellaradmin/products/edit/1");

        // Assert
        var logs = sut.Services.GetRequiredService<RecordingLoggerProvider>();
        await Assert
            .That(document.QuerySelector("input[name='Entity.CategoryId']")?.GetAttribute("type"))
            .IsEqualTo("number");
        await Assert
            .That(
                logs.ConfigurationProblems.Any(log =>
                    log.Level == LogLevel.Information
                    && log.Message.Contains(
                        "uses its property's default editor because no resource is registered for Category."
                    )
                )
            )
            .IsTrue();
    }

    [Test]
    public async Task Search_ReferenceSettings_SearchOrderAndFilterItems()
    {
        // Arrange
        await using var sut = await CreateLookupHost(category =>
            category.Reference(reference =>
                reference
                    .SearchOn(item => item.Name)
                    .OrderBy(item => item.Id)
                    .Where(item => item.Id != 2)
            )
        );
        using var client = sut.GetTestClient();

        // Act
        var document = await GetDocument(
            client,
            "/stellaradmin/products/lookup?form=create&field=CategoryId&term=E"
        );

        // Assert
        await Assert
            .That(ItemValues(document))
            .IsEquivalentTo(["1", "3"], CollectionOrdering.Matching);
    }

    [Test]
    public async Task Search_WithoutReferenceSettings_UsesIndexSearchAndDefaultSort()
    {
        // Arrange
        await using var sut = await CreateLookupHost(category =>
            category.Index(index =>
            {
                index.Columns(columns =>
                    columns.Add(item => item.Name, column => column.Sortable())
                );
                index.DefaultSortByDescending(item => item.Name);
                index.EnableSearch(term => item => item.Name.EndsWith(term));
            })
        );
        using var client = sut.GetTestClient();

        // Act
        var search = await GetDocument(
            client,
            "/stellaradmin/products/lookup?form=create&field=CategoryId&term=e"
        );
        var all = await GetDocument(
            client,
            "/stellaradmin/products/lookup?form=create&field=CategoryId"
        );

        // Assert
        await Assert
            .That(ItemValues(search))
            .IsEquivalentTo(["1", "2"], CollectionOrdering.Matching);
        await Assert
            .That(ItemValues(all))
            .IsEquivalentTo(["3", "1", "2"], CollectionOrdering.Matching);
    }

    [Test]
    public async Task Lookup_FieldWhere_FiltersByTheForm()
    {
        // Arrange
        await using var sut = await EfCoreTestHost.CreateAsync(configureAdmin: admin =>
            admin.AddEfCoreResource<CatalogDbContext, Category>(category =>
                category.AllowEdit(edit =>
                    edit.Fields(fields =>
                        fields
                            .Add(item => item.ParentId)
                            .UseEditor<LookupSheetEditor>(lookup =>
                                lookup.UseResource<Category>(items =>
                                    items.Where<Category>((item, form) => item.Id != form.Id)
                                )
                            )
                    )
                )
            )
        );
        using var client = sut.GetTestClient();

        // Act
        var document = await GetDocument(
            client,
            "/stellaradmin/categories/lookup/1?form=edit&field=ParentId"
        );

        // Assert
        await Assert
            .That(ItemValues(document))
            .IsEquivalentTo(["2", "3"], CollectionOrdering.Matching);
    }

    [Test]
    public async Task Select_UseResource_AppliesGlobalQueryFilter()
    {
        // Arrange
        await using var sut = await EfCoreTestHost.CreateAsync(configureAdmin: admin =>
            admin.AddEfCoreResource<CatalogDbContext, Category>(category =>
                category.AllowCreate<FeatureProductModel, FeatureProductHandler>(create =>
                    create.Fields(fields =>
                        fields
                            .Add(model => model.ProductNumber)
                            .UseEditor<SelectEditor>(select => select.UseResource<Product>())
                    )
                )
            )
        );
        using var client = sut.GetTestClient();

        // Act
        var document = await GetDocument(client, "/stellaradmin/categories/create");

        // Assert
        await Assert
            .That(OptionValues(document, "Entity.ProductNumber"))
            .IsEquivalentTo(["2", "3", "4", "1"], CollectionOrdering.Matching);
    }

    [Test]
    public async Task UntranslatableTitle_ShowsTitleOrderedByKeyAndLogsProblem()
    {
        // Arrange
        await using var sut = await EfCoreTestHost.CreateAsync(
            resource =>
                resource.AllowCreate(create =>
                    create.Fields(fields =>
                        fields.Add(product => product.CategoryId).UseEditor<SelectEditor>()
                    )
                ),
            configureAdmin: admin =>
                admin.AddEfCoreResource<CatalogDbContext, Category>(category =>
                    category.UseTitle(item => Shout(item.Name))
                )
        );
        using var client = sut.GetTestClient();

        // Act
        var document = await GetDocument(client, "/stellaradmin/products/create");

        // Assert
        var logs = sut.Services.GetRequiredService<RecordingLoggerProvider>();
        await Assert
            .That(OptionValues(document, "Entity.CategoryId"))
            .IsEquivalentTo(["1", "2", "3"], CollectionOrdering.Matching);
        await Assert
            .That(
                document
                    .QuerySelectorAll("select[name='Entity.CategoryId'] option[value='1']")
                    .Single()
                    .TextContent
            )
            .IsEqualTo("OFFICE!");
        await Assert
            .That(
                logs.ConfigurationProblems.Any(log =>
                    log.Message.Contains("EF Core can't translate the Category title")
                )
            )
            .IsTrue();
    }

    [Test]
    public async Task ForeignKeySelect_WithoutTitle_IsLabelledByCategoryResource()
    {
        // Arrange
        await using var sut = await EfCoreTestHost.CreateAsync(
            resource =>
                resource.AllowCreate(create =>
                    create.Fields(fields =>
                        fields.Add(product => product.CategoryId).UseEditor<SelectEditor>()
                    )
                ),
            configureAdmin: admin => admin.AddEfCoreResource<CatalogDbContext, Category>()
        );
        using var client = sut.GetTestClient();

        // Act
        var document = await GetDocument(client, "/stellaradmin/products/create");

        // Assert
        await Assert
            .That(document.QuerySelector("label[for='Entity_CategoryId']")?.TextContent.Trim())
            .IsEqualTo("Category");
    }

    [Test]
    public async Task Index_NavigationColumn_ShowsCategoryTitles()
    {
        // Arrange
        await using var sut = await EfCoreTestHost.CreateAsync(
            resource =>
                resource.Index(index =>
                    index.Columns(columns => columns.Add(product => product.Category))
                ),
            configureAdmin: admin => admin.AddEfCoreResource<CatalogDbContext, Category>()
        );
        using var client = sut.GetTestClient();

        // Act
        var document = await GetDocument(client, "/stellaradmin/products");

        // Assert
        await Assert
            .That(
                document
                    .QuerySelectorAll("tbody > tr")
                    .Select(row => row.QuerySelectorAll("td")[3].TextContent.Trim())
            )
            .IsEquivalentTo(
                ["Office", "Technology", "Technology", "Beverage"],
                CollectionOrdering.Matching
            );
    }

    [Test]
    public async Task Index_NavigationColumnWithTitleThroughNavigation_ShowsFullTitles()
    {
        // Arrange
        await using var sut = await EfCoreTestHost.CreateAsync(
            resource =>
                resource.Index(index =>
                    index.Columns(columns => columns.Add(product => product.Category))
                ),
            configureAdmin: admin =>
                admin.AddEfCoreResource<CatalogDbContext, Category>(category =>
                    category.UseTitle(item =>
                        item.Parent != null ? item.Parent.Name + " / " + item.Name : item.Name
                    )
                )
        );
        using var client = sut.GetTestClient();

        // Act
        var document = await GetDocument(client, "/stellaradmin/products");

        // Assert
        await Assert
            .That(
                document
                    .QuerySelectorAll("tbody > tr")
                    .Select(row => row.QuerySelectorAll("td")[3].TextContent.Trim())
            )
            .IsEquivalentTo(
                ["Office", "Office / Technology", "Office / Technology", "Beverage"],
                CollectionOrdering.Matching
            );
    }

    [Test]
    public async Task View_NavigationFieldWithTitleThroughNavigation_ShowsFullTitle()
    {
        // Arrange
        await using var sut = await EfCoreTestHost.CreateAsync(
            resource =>
                resource.AllowView(view =>
                    view.Fields(fields => fields.Add(product => product.Category))
                ),
            configureAdmin: admin =>
                admin.AddEfCoreResource<CatalogDbContext, Category>(category =>
                    category.UseTitle(item =>
                        item.Parent != null ? item.Parent.Name + " / " + item.Name : item.Name
                    )
                )
        );
        using var client = sut.GetTestClient();

        // Act
        var document = await GetDocument(client, "/stellaradmin/products/view/2");

        // Assert
        await Assert
            .That(document.QuerySelector("[data-slot='display-field-value']")?.TextContent.Trim())
            .IsEqualTo("Office / Technology");
    }

    [Test]
    public async Task Index_SortableNavigationColumn_IsNotSortableAndLogsProblem()
    {
        // Arrange
        await using var sut = await EfCoreTestHost.CreateAsync(
            resource =>
                resource.Index(index =>
                    index.Columns(columns =>
                        columns.Add(product => product.Category, column => column.Sortable())
                    )
                ),
            configureAdmin: admin => admin.AddEfCoreResource<CatalogDbContext, Category>()
        );
        using var client = sut.GetTestClient();

        // Act
        var document = await GetDocument(client, "/stellaradmin/products");

        // Assert
        var logs = sut.Services.GetRequiredService<RecordingLoggerProvider>();
        await Assert.That(document.QuerySelectorAll("tbody > tr").Length).IsEqualTo(4);
        await Assert
            .That(
                logs.ConfigurationProblems.Any(log =>
                    log.Message.Contains(
                        "The Category column isn't sortable because it shows a related record."
                    )
                )
            )
            .IsTrue();
    }

    [Test]
    public async Task View_ManyToManyBadges_ShowTagTitles()
    {
        // Arrange
        await using var sut = await EfCoreTestHost.CreateAsync(
            resource =>
                resource.AllowView(view =>
                    view.Fields(fields =>
                        fields.Add(product => product.Tags).UseDisplay<BadgeDisplay>()
                    )
                ),
            configureAdmin: admin => admin.AddEfCoreResource<CatalogDbContext, Tag>()
        );
        using var client = sut.GetTestClient();

        // Act
        var document = await GetDocument(client, "/stellaradmin/products/view/2");

        // Assert
        await Assert
            .That(
                document
                    .QuerySelectorAll("[data-slot='badge']")
                    .Select(badge => badge.TextContent.Trim())
            )
            .IsEquivalentTo(["Sale", "Seasonal"]);
    }

    private static Task<Microsoft.AspNetCore.Builder.WebApplication> CreateLookupHost(
        Action<EfCoreResourceBuilder<CatalogDbContext, Category>> configureCategories
    ) =>
        EfCoreTestHost.CreateAsync(
            resource =>
                resource.AllowCreate(create =>
                    create.Fields(fields =>
                        fields.Add(product => product.CategoryId).UseEditor<LookupSheetEditor>()
                    )
                ),
            configureAdmin: admin =>
                admin.AddEfCoreResource<CatalogDbContext, Category>(configureCategories)
        );

    private static async Task<IDocument> GetDocument(HttpClient client, string url)
    {
        using var response = await client.GetAsync(url);
        response.EnsureSuccessStatusCode();

        return await new HtmlParser().ParseDocumentAsync(
            await response.Content.ReadAsStringAsync()
        );
    }

    private static string?[] ItemValues(IDocument document) =>
        document
            .QuerySelectorAll("[data-lookup='item']")
            .Select(item => item.GetAttribute("data-value"))
            .ToArray();

    private static string?[] OptionValues(IDocument document, string name) =>
        document
            .QuerySelectorAll($"select[name='{name}'] option")
            .Select(option => option.GetAttribute("value"))
            .Where(value => value != "")
            .ToArray();

    private static string Shout(string name) => name.ToUpperInvariant() + "!";

    public sealed class FeatureProductModel
    {
        public int? ProductNumber { get; set; }
    }

    public sealed class FeatureProductHandler : IResourceCreateHandler<FeatureProductModel>
    {
        public Task<ResourceOperationResult> CreateAsync(
            FeatureProductModel model,
            CancellationToken cancellationToken
        ) => Task.FromResult(ResourceOperationResult.Success());
    }
}
