using Microsoft.Extensions.DependencyInjection;
using StellarAdmin.IntegrationTests.Fixtures;
using StellarAdmin.IntegrationTests.Infrastructure;
using StellarAdmin.Resources;
using StellarAdmin.Resources.Builders;
using StellarAdmin.Testing;
using TUnit.Assertions.Enums;

namespace StellarAdmin.IntegrationTests.Resources;

public class ResourceIndexDataLayoutsTests
{
    [Test]
    public async Task OneLayout_HasNoSwitcher()
    {
        // Arrange
        await using var sut = await StellarAdminTestHost.CreateAsync(CreateState());
        using var client = sut.CreateClient();

        // Act
        var document = await client.GetDocumentAsync("/stellaradmin/products");

        // Assert
        await Assert.That(document.QuerySelector(".sa-resource-index-layouts")).IsNull();
    }

    [Test]
    public async Task Layouts_SwitcherListsThemInOrderWithTheFirstSelected()
    {
        // Arrange
        await using var sut = await StellarAdminTestHost.CreateAsync(
            CreateState(),
            ConfigureLayouts,
            defaultTable: false
        );
        using var client = sut.CreateClient();

        // Act
        var document = await client.GetDocumentAsync("/stellaradmin/products?search=lamp");

        // Assert
        var switcher = document.RequiredElement(
            ".sa-resource-index-layouts-row > .sa-resource-index-layouts"
        );
        await Assert
            .That(
                switcher
                    .QuerySelectorAll("label")
                    .Select(label =>
                        $"{label.QuerySelector("input")!.GetAttribute("value")}: {label.GetAttribute("title")}"
                    )
            )
            .IsEquivalentTo(
                ["products: All products", "prices: Prices", "tiles: Tiles", "totals: Totals"],
                CollectionOrdering.Matching
            );
        await Assert
            .That(switcher.QuerySelector("input:checked")!.GetAttribute("value"))
            .IsEqualTo("products");
        await Assert.That(switcher.QuerySelector("label[title='All products'] svg")).IsNotNull();
        await Assert
            .That(switcher.GetAttribute("hx-get"))
            .IsEqualTo("/stellaradmin/products?search=lamp");
    }

    [Test]
    public async Task LayoutsWithoutTitles_AreTitledWithTheirKeysInSentenceCase()
    {
        // Arrange
        await using var sut = await StellarAdminTestHost.CreateAsync(
            CreateState(),
            resource =>
                resource.Index(index =>
                {
                    index.AddTable(
                        "all-products",
                        table =>
                            table.Columns(columns => columns.Clear().Add(product => product.Name))
                    );
                    index.AddPartial("tiles", "_ProductTiles");
                }),
            defaultTable: false
        );
        using var client = sut.CreateClient();

        // Act
        var document = await client.GetDocumentAsync("/stellaradmin/products");

        // Assert
        await Assert
            .That(
                document
                    .QuerySelectorAll(".sa-resource-index-layouts label")
                    .Select(label => label.GetAttribute("title")!)
            )
            .IsEquivalentTo(["All products", "Tiles"], CollectionOrdering.Matching);
    }

    [Test]
    public async Task LayoutQuery_ShowsThatTableWithItsColumnsSortAndPaging()
    {
        // Arrange
        var state = CreateState();
        await using var sut = await StellarAdminTestHost.CreateAsync(
            state,
            ConfigureLayouts,
            defaultTable: false
        );
        using var client = sut.CreateClient();

        // Act
        var document = await client.GetDocumentAsync("/stellaradmin/products?layout=PRICES");

        // Assert
        var request = state.ListRequests.Single();
        await Assert.That(request.Layout).IsEqualTo("prices");
        await Assert
            .That(request.Sort! with { Selector = null })
            .IsEqualTo(new ResourceSort(nameof(Product.Price), ResourceSortDirection.Descending));
        await Assert.That(request.Sort!.Selector?.ToString()).IsEqualTo("product => product.Price");
        await Assert
            .That(request.Fields.Select(field => field.ToString()))
            .IsEquivalentTo(["product => product.Price"]);
        await Assert.That(request.Paging).IsEqualTo(new ResourcePaging(1, 2));
        await Assert
            .That(
                document
                    .QuerySelectorAll("#index-page-grid thead th")
                    .Select(header => header.TextContent.Trim())
                    .Where(text => text.Length > 0)
            )
            .IsEquivalentTo(["Price"]);
        await Assert
            .That(
                document
                    .RequiredElement(".sa-resource-index-layouts input:checked")
                    .GetAttribute("value")
            )
            .IsEqualTo("prices");
    }

    [Test]
    public async Task UnknownLayout_ShowsTheFirstLayout()
    {
        // Arrange
        var state = CreateState();
        await using var sut = await StellarAdminTestHost.CreateAsync(
            state,
            ConfigureLayouts,
            defaultTable: false
        );
        using var client = sut.CreateClient();

        // Act
        await client.GetDocumentAsync("/stellaradmin/products?layout=missing");

        // Assert
        await Assert.That(state.ListRequests.Single().Layout).IsEqualTo("products");
    }

    [Test]
    public async Task LayoutThatIgnoresScopesAndSearch_HidesAndSkipsThemButKeepsThemInLinks()
    {
        // Arrange
        var state = CreateState();
        await using var sut = await StellarAdminTestHost.CreateAsync(
            state,
            ConfigureLayouts,
            defaultTable: false
        );
        using var client = sut.CreateClient();

        // Act
        var document = await client.GetDocumentAsync(
            "/stellaradmin/products?layout=prices&scope=cheap&search=lamp"
        );

        // Assert
        var request = state.ListRequests.Single();
        await Assert.That(request.Scope).IsNull();
        await Assert.That(request.Search).IsNull();
        await Assert.That(document.QuerySelector("#index-page-scopes")).IsNull();
        await Assert.That(document.QuerySelector("#index-page-search")).IsNull();
        await Assert
            .That(document.RequiredElement(".sa-resource-index-layouts").GetAttribute("hx-get"))
            .IsEqualTo("/stellaradmin/products?scope=cheap&search=lamp");
    }

    [Test]
    public async Task OtherLayout_KeepsItsKeyInScopeAndRowLinks()
    {
        // Arrange
        await using var sut = await StellarAdminTestHost.CreateAsync(
            CreateState(),
            resource =>
            {
                ConfigureLayouts(resource);
                resource.UseKey(product => product.Id).AllowDelete();
            },
            defaultTable: false
        );
        using var client = sut.CreateClient();

        // Act
        var document = await client.GetDocumentAsync("/stellaradmin/products?layout=products");

        // Assert
        await Assert
            .That(
                document
                    .QuerySelectorAll("#index-page-scopes a")
                    .All(link => link.GetAttribute("href")!.Contains("layout=products"))
            )
            .IsTrue();
        await Assert
            .That(document.RequiredElement("form[data-resource-delete]").GetAttribute("action"))
            .Contains("layout=products");
    }

    [Test]
    public async Task RowActions_TurnOffDeleteForThatTableOnly()
    {
        // Arrange
        await using var sut = await StellarAdminTestHost.CreateAsync(
            CreateState(),
            resource =>
            {
                ConfigureLayouts(resource);
                resource.UseKey(product => product.Id).AllowDelete();
            },
            defaultTable: false
        );
        using var client = sut.CreateClient();

        // Act
        var products = await client.GetDocumentAsync("/stellaradmin/products");
        var prices = await client.GetDocumentAsync("/stellaradmin/products?layout=prices");

        // Assert
        await Assert
            .That(products.QuerySelectorAll("form[data-resource-delete]").Length)
            .IsGreaterThan(0);
        await Assert.That(prices.QuerySelector("form[data-resource-delete]")).IsNull();
    }

    [Test]
    public async Task PartialLayout_RendersItsPartialWithEveryItemTheSearchSelects()
    {
        // Arrange
        var state = CreateState();
        await using var sut = await StellarAdminTestHost.CreateAsync(
            state,
            ConfigureLayouts,
            defaultTable: false
        );
        using var client = sut.CreateClient();

        // Act
        var document = await client.GetDocumentAsync(
            "/stellaradmin/products?layout=tiles&search=lamp"
        );

        // Assert
        var request = state.ListRequests.Single();
        await Assert.That(request.Layout).IsEqualTo("tiles");
        await Assert.That(request.Fields).IsEmpty();
        await Assert.That(request.Search).IsEqualTo("lamp");
        await Assert.That(request.Sort).IsNull();
        await Assert.That(request.Paging).IsNull();
        await Assert.That(document.QuerySelector("#index-page-grid")).IsNull();
        await Assert
            .That(
                document
                    .QuerySelectorAll("#index-page-data-grid [data-test='tile']")
                    .Select(tile => tile.FirstChild!.TextContent.Trim())
            )
            .IsEquivalentTo(["Blue lamp", "Red lamp"], CollectionOrdering.Matching);
        await Assert.That(document.QuerySelector("#index-page-search")).IsNotNull();
        await Assert
            .That(
                document
                    .RequiredElement(".sa-resource-index-layouts input:checked")
                    .GetAttribute("value")
            )
            .IsEqualTo("tiles");
    }

    [Test]
    public async Task PartialLayout_LinksToRecordPagesAndToTheIndexWithItsState()
    {
        // Arrange
        await using var sut = await StellarAdminTestHost.CreateAsync(
            CreateState(),
            resource =>
            {
                ConfigureLayouts(resource);
                resource
                    .UseKey(product => product.Id)
                    .AllowEdit(edit => edit.Fields(fields => fields.Add(product => product.Name)))
                    .AllowView(view => view.Fields(fields => fields.Add(product => product.Name)));
            },
            defaultTable: false
        );
        using var client = sut.CreateClient();

        // Act
        var document = await client.GetDocumentAsync(
            "/stellaradmin/products?layout=tiles&scope=cheap&search=desk"
        );

        // Assert
        var tile = document.RequiredElement("[data-test='tile']");
        await Assert
            .That(tile.RequiredElement("[data-test='view']").GetAttribute("href"))
            .IsEqualTo("/stellaradmin/products/view/2");
        await Assert
            .That(tile.RequiredElement("[data-test='edit']").GetAttribute("href"))
            .IsEqualTo("/stellaradmin/products/edit/2");
        await Assert
            .That(document.RequiredElement("[data-test='index-url']").GetAttribute("href"))
            .IsEqualTo("/stellaradmin/products?layout=products&scope=cheap&search=desk&page=2");
    }

    [Test]
    public async Task PartialLayout_HasNoRowDetails()
    {
        // Arrange
        await using var sut = await StellarAdminTestHost.CreateAsync(
            CreateState(),
            resource =>
            {
                ConfigureLayouts(resource);
                resource.UseKey(product => product.Id);
            },
            defaultTable: false
        );
        using var client = sut.CreateClient();

        // Act
        using var response = await client.GetAsync(
            "/stellaradmin/products/rowdetail/1?layout=tiles"
        );

        // Assert
        await Assert.That(response.StatusCode).IsEqualTo(System.Net.HttpStatusCode.NotFound);
    }

    [Test]
    public async Task HandlerLayout_RendersItsPartialWithTheHandlersItemsInsteadOfTheResources()
    {
        // Arrange
        var state = CreateState();
        await using var sut = await StellarAdminTestHost.CreateAsync(
            state,
            resource =>
            {
                ConfigureLayouts(resource);
                resource
                    .UseKey(product => product.Id)
                    .AllowView(view => view.Fields(fields => fields.Add(product => product.Name)));
            },
            defaultTable: false
        );
        using var client = sut.CreateClient();

        // Act
        var document = await client.GetDocumentAsync(
            "/stellaradmin/products?layout=totals&scope=cheap&search=lamp"
        );

        // Assert
        await Assert.That(state.ListRequests).IsEmpty();
        var total = document.RequiredElement("#index-page-data-grid [data-test='total']");
        await Assert.That(total.TextContent.Trim()).IsEqualTo("totals/cheap/none/3");
        await Assert.That(total.GetAttribute("data-view")).IsEqualTo("none");
        await Assert.That(document.QuerySelector("#index-page-scopes")).IsNotNull();
        await Assert.That(document.QuerySelector("#index-page-search")).IsNull();
        await Assert
            .That(document.RequiredElement("[data-test='index-url']").GetAttribute("href"))
            .IsEqualTo("/stellaradmin/products?layout=products&scope=cheap&search=lamp");
    }

    [Test]
    public async Task HandlerTable_ListsTheHandlersRowsWithTheRequestsSortAndPaging()
    {
        // Arrange
        var state = CreateState();
        await using var sut = await StellarAdminTestHost.CreateAsync(
            state,
            resource =>
                ConfigureRowsTable(
                    resource,
                    rows =>
                        rows.EnablePaging(paging =>
                        {
                            paging.PageSize = 2;
                            paging.PageSizes = [2, 3];
                        })
                ),
            defaultTable: false
        );
        using var client = sut.CreateClient();

        // Act
        var document = await client.GetDocumentAsync(
            "/stellaradmin/products?layout=rows&scope=cheap&search=lamp&sortBy=price&sortDirection=desc&page=2"
        );

        // Assert
        await Assert.That(state.ListRequests).IsEmpty();
        var request = state.HandlerRequests.Single();
        await Assert.That(request.Layout).IsEqualTo("rows");
        await Assert.That(request.Scope).IsEqualTo("cheap");
        await Assert.That(request.Search).IsEqualTo("lamp");
        await Assert
            .That(request.Sort! with { Selector = null })
            .IsEqualTo(
                new ResourceSort(nameof(ProductRow.Price), ResourceSortDirection.Descending)
            );
        await Assert.That(request.Paging).IsEqualTo(new ResourcePaging(2, 2));
        await Assert
            .That(RowLabels(document))
            .IsEquivalentTo(["Red lamp", "Blue lamp"], CollectionOrdering.Matching);
        await Assert.That(document.QuerySelector("#index-page-grid tbody a")).IsNull();
    }

    [Test]
    public async Task HandlerTableWithRecordKey_GivesRecordRowsTheResourcesRowActions()
    {
        // Arrange
        await using var sut = await StellarAdminTestHost.CreateAsync(
            CreateState(),
            resource =>
            {
                ConfigureRowsTable(
                    resource,
                    rows =>
                        rows.HasRecordKey(row => row.ProductId)
                            .RowActions(actions => actions.Edit = false)
                );
                resource
                    .UseKey(product => product.Id)
                    .AllowEdit(edit => edit.Fields(fields => fields.Add(product => product.Name)))
                    .AllowView(view => view.Fields(fields => fields.Add(product => product.Name)));
                resource.AllowDelete();
            },
            defaultTable: false
        );
        using var client = sut.CreateClient();

        // Act
        var document = await client.GetDocumentAsync("/stellaradmin/products?layout=rows");

        // Assert
        var rows = document.QuerySelectorAll("#index-page-grid tbody tr").ToArray();
        await Assert
            .That(rows[0].RequiredElement("a[href*='/view/']").GetAttribute("href"))
            .IsEqualTo("/stellaradmin/products/view/1");
        await Assert.That(rows[0].QuerySelector("a[href*='/edit/']")).IsNull();
        await Assert
            .That(rows[0].RequiredElement("form[data-resource-delete]").GetAttribute("action"))
            .StartsWith("/stellaradmin/products/delete/1?layout=rows");
        await Assert.That(rows[^1].TextContent).Contains("Total");
        await Assert.That(rows[^1].QuerySelector("a, form")).IsNull();
    }

    [Test]
    public async Task HandlerTableWithLink_LinksRowsToTheOtherResourcesViewPage()
    {
        // Arrange
        await using var sut = await StellarAdminTestHost.CreateAsync(
            CreateState(),
            resource =>
            {
                ConfigureRowsTable(resource, rows => rows.LinkTo<Category>(row => row.CategoryId));
                resource
                    .UseKey(product => product.Id)
                    .AllowView(view => view.Fields(fields => fields.Add(product => product.Name)));
                resource.AllowDelete();
            },
            admin =>
                admin.AddCategoryResource(categories =>
                    categories.AllowView(view =>
                        view.Fields(fields => fields.Add(category => category.Name))
                    )
                ),
            services => services.AddSingleton<CategoryStore>(),
            defaultTable: false
        );
        using var client = sut.CreateClient();

        // Act
        var document = await client.GetDocumentAsync("/stellaradmin/products?layout=rows");

        // Assert
        var rows = document.QuerySelectorAll("#index-page-grid tbody tr").ToArray();
        await Assert
            .That(rows[0].QuerySelectorAll("a").Select(link => link.GetAttribute("href")))
            .IsEquivalentTo(["/stellaradmin/categories/view/11"]);
        await Assert.That(rows[0].QuerySelector("form")).IsNull();
        await Assert.That(rows[^1].QuerySelector("a")).IsNull();
    }

    [Test]
    public async Task HandlerTableWithLinkToAResourceWithoutAViewPage_HasNoLinksAndReportsIt()
    {
        // Arrange
        await using var sut = await StellarAdminTestHost.CreateAsync(
            CreateState(),
            resource =>
                ConfigureRowsTable(resource, rows => rows.LinkTo<Category>(row => row.CategoryId)),
            admin => admin.AddCategoryResource(),
            services => services.AddSingleton<CategoryStore>(),
            defaultTable: false
        );
        using var client = sut.CreateClient();

        // Act
        var document = await client.GetDocumentAsync("/stellaradmin/products?layout=rows");

        // Assert
        var logs = sut.Services.GetRequiredService<RecordingLoggerProvider>();
        await Assert.That(document.QuerySelector("#index-page-grid tbody a")).IsNull();
        await Assert
            .That(
                logs.ConfigurationProblems.Any(log =>
                    log.Message.Contains("don't link to Category")
                )
            )
            .IsTrue();
    }

    [Test]
    public async Task HandlerPartialWithRecordKey_LinksItemsToTheirRecordsPages()
    {
        // Arrange
        await using var sut = await StellarAdminTestHost.CreateAsync(
            CreateState(),
            resource =>
            {
                ConfigureRowsPartial(resource, rows => rows.HasRecordKey(row => row.ProductId));
                resource
                    .UseKey(product => product.Id)
                    .AllowEdit(edit => edit.Fields(fields => fields.Add(product => product.Name)))
                    .AllowView(view => view.Fields(fields => fields.Add(product => product.Name)));
            },
            defaultTable: false
        );
        using var client = sut.CreateClient();

        // Act
        var document = await client.GetDocumentAsync("/stellaradmin/products?layout=rows");

        // Assert
        var rows = document.QuerySelectorAll("[data-test='row']").ToArray();
        await Assert
            .That(rows[0].GetAttribute("data-view"))
            .IsEqualTo("/stellaradmin/products/view/1");
        await Assert
            .That(rows[0].GetAttribute("data-edit"))
            .IsEqualTo("/stellaradmin/products/edit/1");
        await Assert.That(rows[^1].TextContent).IsEqualTo("Total");
        await Assert.That(rows[^1].GetAttribute("data-view")).IsEqualTo("none");
        await Assert.That(rows[^1].GetAttribute("data-edit")).IsEqualTo("none");
    }

    [Test]
    public async Task HandlerPartialWithLink_LinksItemsToTheOtherResourcesViewPage()
    {
        // Arrange
        await using var sut = await StellarAdminTestHost.CreateAsync(
            CreateState(),
            resource =>
            {
                ConfigureRowsPartial(
                    resource,
                    rows => rows.LinkTo<Category>(row => row.CategoryId)
                );
                resource
                    .UseKey(product => product.Id)
                    .AllowEdit(edit => edit.Fields(fields => fields.Add(product => product.Name)));
            },
            admin =>
                admin.AddCategoryResource(categories =>
                    categories.AllowView(view =>
                        view.Fields(fields => fields.Add(category => category.Name))
                    )
                ),
            services => services.AddSingleton<CategoryStore>(),
            defaultTable: false
        );
        using var client = sut.CreateClient();

        // Act
        var document = await client.GetDocumentAsync("/stellaradmin/products?layout=rows");

        // Assert
        var rows = document.QuerySelectorAll("[data-test='row']").ToArray();
        await Assert
            .That(rows[0].GetAttribute("data-view"))
            .IsEqualTo("/stellaradmin/categories/view/11");
        await Assert.That(rows[0].GetAttribute("data-edit")).IsEqualTo("none");
        await Assert.That(rows[^1].GetAttribute("data-view")).IsEqualTo("none");
    }

    // The products table, then a partial of rows its handler loads
    private static void ConfigureRowsPartial(
        ResourceBuilder<Product> resource,
        Action<ResourceHandlerPartialDataLayoutBuilder<Product, ProductRow>> configure
    ) =>
        resource.Index(index =>
        {
            index.AddTable("products");
            index.AddPartial<ProductRow, ProductRowsHandler>("rows", "_ProductRows", configure);
        });

    // The products table, then a table of rows its handler loads
    private static void ConfigureRowsTable(
        ResourceBuilder<Product> resource,
        Action<ResourceHandlerTableDataLayoutBuilder<Product, ProductRow>> configure
    ) =>
        resource.Index(index =>
        {
            index.EnableScopes(scopes =>
            {
                scopes.Add("all", "All");
                scopes.Add("cheap", "Cheap");
            });
            index.EnableSearch();
            index.AddTable("products");
            index.AddTable<ProductRow, ProductRowsHandler>(
                "rows",
                rows =>
                {
                    rows.Columns(columns =>
                    {
                        columns.Add(row => row.Label);
                        columns.Add(row => row.Price, column => column.Sortable());
                    });
                    configure(rows);
                }
            );
        });

    private static IEnumerable<string> RowLabels(AngleSharp.Dom.IDocument document) =>
        document
            .QuerySelectorAll("#index-page-grid tbody tr")
            .Select(row => row.QuerySelector("td")!.TextContent.Trim());

    private static void ConfigureLayouts(ResourceBuilder<Product> resource) =>
        resource.Index(index =>
        {
            index.EnableScopes(scopes =>
            {
                scopes.Add("all", "All");
                scopes.Add("cheap", "Cheap");
            });
            index.EnableSearch();
            index.AddTable(
                "products",
                table =>
                {
                    table.Title = "All products";
                    table.Icon = "table";
                    table.Columns(columns =>
                        columns.Clear().Add(product => product.Name, column => column.Sortable())
                    );
                    table.DefaultSortBy(product => product.Name);
                }
            );
            index.AddTable(
                "prices",
                table =>
                {
                    table.Columns(columns =>
                        columns.Clear().Add(product => product.Price, column => column.Sortable())
                    );
                    table.DefaultSortByDescending(product => product.Price);
                    table.EnablePaging(paging =>
                    {
                        paging.PageSize = 2;
                        paging.PageSizes = [2, 3];
                    });
                    table.RowActions(actions => actions.Delete = false);
                    table.IgnoreScopes();
                    table.IgnoreSearch();
                }
            );
            index.AddPartial("tiles", "_ProductTiles");
            index.AddPartial<ProductTotal, ProductTotalsHandler>(
                "totals",
                "_ProductTotals",
                totals => totals.IgnoreSearch()
            );
        });

    private static ProductState CreateState() =>
        new([
            new Product(1, "Blue lamp", 10),
            new Product(2, "Desk", 100),
            new Product(3, "Red lamp", 70),
        ]);
}
