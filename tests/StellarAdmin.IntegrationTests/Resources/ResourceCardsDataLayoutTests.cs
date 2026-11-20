using Microsoft.Extensions.DependencyInjection;
using StellarAdmin.IntegrationTests.Fixtures;
using StellarAdmin.IntegrationTests.Infrastructure;
using StellarAdmin.Resources;
using StellarAdmin.Resources.Builders;
using StellarAdmin.Resources.Displays;
using StellarAdmin.Testing;
using TUnit.Assertions.Enums;

namespace StellarAdmin.IntegrationTests.Resources;

public class ResourceCardsDataLayoutTests
{
    [Test]
    public async Task CardsWithoutParts_ShowOnlyTheResourcesTitle()
    {
        // Arrange
        await using var sut = await StellarAdminTestHost.CreateAsync(
            CreateState(),
            resource =>
            {
                resource
                    .UseKey(product => product.Id)
                    .UseTitle(product => "Product " + product.Name)
                    .Reference(reference =>
                        reference
                            .UseDescription(product => product.Name + " description")
                            .UseImage(product =>
                                product.Id == 2 ? null : $"/images/{product.Id}.jpg"
                            )
                    )
                    .AllowView(view => view.Fields(fields => fields.Add(product => product.Name)));
                resource.Index(index => index.AddCards("cards"));
            },
            defaultTable: false
        );
        using var client = sut.CreateClient();

        // Act
        var document = await client.GetDocumentAsync("/stellaradmin/products");

        // Assert
        var cards = document.QuerySelectorAll(".sa-resource-cards > .sa-resource-card");
        await Assert.That(cards.Length).IsEqualTo(3);
        var link = cards[0].RequiredElement(".sa-card-title > a");
        await Assert.That(link.TextContent.Trim()).IsEqualTo("Product Blue lamp");
        await Assert.That(link.GetAttribute("href")).IsEqualTo("/stellaradmin/products/view/1");
        await Assert.That(cards[0].QuerySelector(".sa-card-description")).IsNull();
        await Assert.That(cards[0].QuerySelector(".sa-resource-card-media")).IsNull();
        await Assert.That(cards[0].QuerySelector(".sa-resource-card-stats")).IsNull();
        await Assert.That(cards[0].QuerySelector(".sa-resource-card-fields")).IsNull();
        await Assert.That(document.QuerySelector("#index-page-grid")).IsNull();
    }

    [Test]
    public async Task CardParts_RenderTheirValuesWithDisplaysLabelsAndFullTextInTitles()
    {
        // Arrange
        var state = CreateState();
        await using var sut = await StellarAdminTestHost.CreateAsync(
            state,
            resource =>
                resource.Index(index =>
                    index.AddCards(
                        "cards",
                        cards =>
                            cards.Card(card =>
                            {
                                card.Title(product => product.Name);
                                card.Description(product => "#" + product.Id);
                                card.Badge(product => product.Price > 50 ? "Premium" : null);
                                card.Stats(stats =>
                                    stats.Add(
                                        product => product.Price,
                                        stat =>
                                        {
                                            stat.Title = "From";
                                            stat.UseDisplay<NumberDisplay>(number =>
                                                number.Format = "0.00"
                                            );
                                        }
                                    )
                                );
                                card.Fields(fields =>
                                {
                                    fields.Layout = ResourceCardFieldLayout.Grid;
                                    fields.Add(product => product.Name);
                                    fields.Add(
                                        product => product.Id > 2 ? null : "Yes",
                                        field =>
                                        {
                                            field.Title = "Featured";
                                            field.EmptyText = "No";
                                        }
                                    );
                                });
                            })
                    )
                ),
            defaultTable: false
        );
        using var client = sut.CreateClient();

        // Act
        var document = await client.GetDocumentAsync("/stellaradmin/products");

        // Assert
        await Assert
            .That(state.ListRequests.Single().Fields.Select(field => field.ToString()))
            .IsEquivalentTo(
                [
                    "product => product.Name",
                    "product => (\"#\" + Convert(product.Id, Object))",
                    "product => IIF((product.Price > 50), \"Premium\", null)",
                    "product => product.Price",
                    "product => product.Name",
                    "product => IIF((product.Id > 2), null, \"Yes\")",
                ],
                CollectionOrdering.Matching
            );
        var cards = document.QuerySelectorAll(".sa-resource-card");
        var desk = cards[1];
        await Assert.That(desk.QuerySelector(".sa-resource-card-media")).IsNull();
        await Assert
            .That(desk.RequiredElement(".sa-card-title > span[title]").TextContent.Trim())
            .IsEqualTo("Desk");
        await Assert
            .That(desk.RequiredElement(".sa-card-title .sa-resource-card-badge").TextContent.Trim())
            .IsEqualTo("Premium");
        await Assert.That(cards[0].QuerySelector(".sa-resource-card-badge")).IsNull();
        var stat = desk.RequiredElement(".sa-resource-card-stats > div");
        await Assert.That(stat.RequiredElement("dt").TextContent.Trim()).IsEqualTo("From");
        await Assert.That(stat.RequiredElement("dd").TextContent.Trim()).IsEqualTo("100.00");
        await Assert.That(stat.RequiredElement("dd").GetAttribute("title")).IsEqualTo("100.00");
        var fields = desk.RequiredElement(".sa-resource-card-fields");
        await Assert.That(fields.GetAttribute("data-layout")).IsEqualTo("grid");
        await Assert
            .That(
                fields
                    .QuerySelectorAll("div")
                    .Select(field =>
                        $"{field.RequiredElement("dt").TextContent.Trim()}: {field.RequiredElement("dd").GetAttribute("title")}"
                    )
            )
            .IsEquivalentTo(["Product name: Desk", "Featured: Yes"], CollectionOrdering.Matching);
        await Assert
            .That(
                cards[2]
                    .RequiredElement(".sa-resource-card-fields div:last-child dd")
                    .TextContent.Trim()
            )
            .IsEqualTo("No");
    }

    [Test]
    public async Task Cards_SortByTheirDefaultSortAndPageAsATableDoes()
    {
        // Arrange
        var state = CreateState();
        await using var sut = await StellarAdminTestHost.CreateAsync(
            state,
            ConfigureSortedCards,
            defaultTable: false
        );
        using var client = sut.CreateClient();

        // Act
        var document = await client.GetDocumentAsync("/stellaradmin/products?page=2&pageSize=2");

        // Assert
        var request = state.ListRequests.Single();
        await Assert.That(request.Layout).IsEqualTo("cards");
        await Assert
            .That(request.Sort! with { Selector = null })
            .IsEqualTo(new ResourceSort(nameof(Product.Price), ResourceSortDirection.Descending));
        await Assert.That(request.Sort!.Selector?.ToString()).IsEqualTo("product => product.Price");
        await Assert.That(request.Paging).IsEqualTo(new ResourcePaging(2, 2));
        var pager = document.RequiredElement(".sa-resource-cards-pager");
        await Assert
            .That(pager.RequiredElement(".sa-resource-cards-pager-status").TextContent.Trim())
            .IsEqualTo("Page 2 of 2");
        await Assert
            .That(pager.RequiredElement("a.sa-pagination-previous").GetAttribute("href"))
            .IsEqualTo("/stellaradmin/products?page=1&pageSize=2");
        await Assert.That(pager.QuerySelector(".sa-pagination-next")).IsNull();
    }

    [Test]
    public async Task SortQuery_ChangesTheDefaultSortsDirectionAndIgnoresOtherFields()
    {
        // Arrange
        var state = CreateState();
        await using var sut = await StellarAdminTestHost.CreateAsync(
            state,
            ConfigureSortedCards,
            defaultTable: false
        );
        using var client = sut.CreateClient();

        // Act
        await client.GetDocumentAsync("/stellaradmin/products?sortBy=price&sortDirection=asc");
        await client.GetDocumentAsync("/stellaradmin/products?sortBy=name&sortDirection=asc");

        // Assert
        await Assert
            .That(state.ListRequests.Select(request => request.Sort! with { Selector = null }))
            .IsEquivalentTo(
                [
                    new ResourceSort(nameof(Product.Price), ResourceSortDirection.Ascending),
                    new ResourceSort(nameof(Product.Price), ResourceSortDirection.Descending),
                ],
                CollectionOrdering.Matching
            );
    }

    [Test]
    public async Task MenuActions_OfferTheResourcesActionsThatTheCardsAllow()
    {
        // Arrange
        await using var sut = await StellarAdminTestHost.CreateAsync(
            CreateState(),
            resource =>
            {
                ConfigureRecordPages(resource);
                resource.Index(index =>
                {
                    index.AddCards("cards");
                    index.AddCards(
                        "browse",
                        cards => cards.RowActions(actions => actions.Delete = false)
                    );
                });
            },
            defaultTable: false
        );
        using var client = sut.CreateClient();

        // Act
        var cards = await client.GetDocumentAsync("/stellaradmin/products");
        var browse = await client.GetDocumentAsync("/stellaradmin/products?layout=browse");

        // Assert
        var card = cards.RequiredElement(".sa-resource-card");
        var menu = card.RequiredElement(
            ".sa-card-header .sa-card-action [data-slot='dropdown-menu-content']"
        );
        await Assert
            .That(menu.QuerySelectorAll("a").Select(link => link.GetAttribute("href")))
            .IsEquivalentTo(
                ["/stellaradmin/products/view/1", "/stellaradmin/products/edit/1"],
                CollectionOrdering.Matching
            );
        await Assert
            .That(menu.RequiredElement("form[data-resource-delete]").GetAttribute("action"))
            .IsEqualTo("/stellaradmin/products/delete/1");
        await Assert.That(browse.QuerySelector("form[data-resource-delete]")).IsNull();
        await Assert
            .That(browse.QuerySelectorAll(".sa-resource-card a[href$='/edit/1']").Length)
            .IsEqualTo(1);
    }

    [Test]
    public async Task FooterActions_ShowEditAsAButtonAndDeleteInTheFootersMenu()
    {
        // Arrange
        await using var sut = await StellarAdminTestHost.CreateAsync(
            CreateState(),
            resource =>
            {
                ConfigureRecordPages(resource);
                resource.Index(index =>
                    index.AddCards(
                        "cards",
                        cards =>
                            cards.Card(card =>
                                card.ActionsPlacement = ResourceCardActionsPlacement.Footer
                            )
                    )
                );
            },
            defaultTable: false
        );
        using var client = sut.CreateClient();

        // Act
        var document = await client.GetDocumentAsync("/stellaradmin/products");

        // Assert
        var card = document.RequiredElement(".sa-resource-card");
        await Assert.That(card.QuerySelector(".sa-card-header .sa-card-action")).IsNull();
        var footer = card.RequiredElement(".sa-card-footer.sa-resource-card-actions");
        await Assert
            .That(footer.RequiredElement(":scope > a.sa-button").GetAttribute("href"))
            .IsEqualTo("/stellaradmin/products/edit/1");
        var menu = footer.RequiredElement(
            ".sa-resource-card-actions-menu [data-slot='dropdown-menu-content']"
        );
        await Assert.That(menu.QuerySelector("a")).IsNull();
        await Assert.That(menu.QuerySelector("form[data-resource-delete]")).IsNotNull();
    }

    [Test]
    public async Task Arrangement_MarksTheCardsForTheirLayout()
    {
        // Arrange
        await using var sut = await StellarAdminTestHost.CreateAsync(
            CreateState(),
            resource =>
            {
                ConfigureRecordPages(resource);
                resource.Index(index =>
                {
                    index.AddCards(
                        "overlay",
                        cards =>
                            cards.Card(card =>
                            {
                                card.Image(
                                    product => $"/images/{product.Id}.jpg",
                                    image => image.Shape = ResourceCardImageShape.Square
                                );
                                card.Badge(product => product.Name);
                                card.Arrangement = ResourceCardArrangement.Overlay;
                            })
                    );
                    index.AddCards(
                        "horizontal",
                        cards =>
                            cards.Card(card =>
                            {
                                card.Image(product => $"/images/{product.Id}.jpg");
                                card.Badge(product => product.Name);
                                card.Arrangement = ResourceCardArrangement.Horizontal;
                            })
                    );
                });
            },
            defaultTable: false
        );
        using var client = sut.CreateClient();

        // Act
        var overlay = await client.GetDocumentAsync("/stellaradmin/products?layout=overlay");
        var horizontal = await client.GetDocumentAsync("/stellaradmin/products?layout=horizontal");

        // Assert
        var overlayCard = overlay.RequiredElement(".sa-resource-card");
        await Assert.That(overlayCard.GetAttribute("data-shape")).IsEqualTo("square");
        await Assert.That(overlayCard.GetAttribute("data-arrangement")).IsEqualTo("overlay");
        await Assert
            .That(
                overlayCard.QuerySelector(
                    ".sa-resource-card-media > .sa-card-header .sa-card-title > a"
                )
            )
            .IsNotNull();
        await Assert
            .That(overlayCard.QuerySelector(".sa-resource-card-on-image .sa-resource-card-badge"))
            .IsNotNull();
        await Assert
            .That(
                overlayCard.QuerySelector(
                    ".sa-resource-card-on-image [data-slot='dropdown-menu-content']"
                )
            )
            .IsNotNull();
        await Assert
            .That(horizontal.RequiredElement(".sa-resource-cards").GetAttribute("data-arrangement"))
            .IsEqualTo("horizontal");
        var horizontalCard = horizontal.RequiredElement(".sa-resource-card");
        await Assert.That(horizontalCard.QuerySelector(".sa-resource-card-on-image")).IsNull();
        await Assert
            .That(
                horizontalCard.QuerySelector(
                    ".sa-resource-card-body .sa-card-title .sa-resource-card-badge"
                )
            )
            .IsNotNull();
    }

    [Test]
    public async Task HandlerCards_ListTheHandlersItemsWithTheRequestsSortAndPaging()
    {
        // Arrange
        var state = CreateState();
        await using var sut = await StellarAdminTestHost.CreateAsync(
            state,
            resource =>
                ConfigureHandlerCards(
                    resource,
                    rows =>
                        rows.DefaultSortByDescending(row => row.Price)
                            .EnablePaging(paging => paging.PageSize = 2)
                ),
            defaultTable: false
        );
        using var client = sut.CreateClient();

        // Act
        var document = await client.GetDocumentAsync(
            "/stellaradmin/products?layout=rows&scope=cheap&search=lamp&page=2"
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
            .That(CardTitles(document))
            .IsEquivalentTo(["Red lamp", "Blue lamp"], CollectionOrdering.Matching);
        await Assert
            .That(document.QuerySelector(".sa-resource-card a, .sa-resource-card form"))
            .IsNull();
        await Assert
            .That(document.RequiredElement(".sa-resource-cards-pager-status").TextContent.Trim())
            .IsEqualTo("Page 2 of 2");
    }

    [Test]
    public async Task HandlerCardsWithRecordKey_GiveRecordCardsTheResourcesRowActions()
    {
        // Arrange
        await using var sut = await StellarAdminTestHost.CreateAsync(
            CreateState(),
            resource =>
            {
                ConfigureHandlerCards(
                    resource,
                    rows =>
                        rows.HasRecordKey(row => row.ProductId)
                            .RowActions(actions => actions.Edit = false)
                );
                ConfigureRecordPages(resource);
            },
            defaultTable: false
        );
        using var client = sut.CreateClient();

        // Act
        var document = await client.GetDocumentAsync("/stellaradmin/products?layout=rows");

        // Assert
        var cards = document.QuerySelectorAll(".sa-resource-card").ToArray();
        await Assert.That(cards[0].GetAttribute("data-key")).IsEqualTo("1");
        await Assert
            .That(cards[0].RequiredElement(".sa-card-title > a").GetAttribute("href"))
            .IsEqualTo("/stellaradmin/products/view/1");
        await Assert.That(cards[0].QuerySelector("a[href*='/edit/']")).IsNull();
        await Assert
            .That(cards[0].RequiredElement("form[data-resource-delete]").GetAttribute("action"))
            .StartsWith("/stellaradmin/products/delete/1?layout=rows");
        await Assert.That(cards[^1].TextContent).Contains("Total");
        await Assert.That(cards[^1].QuerySelector("a, form")).IsNull();
    }

    [Test]
    public async Task HandlerCardsWithLink_LinkCardsToTheOtherResourcesViewPage()
    {
        // Arrange
        await using var sut = await StellarAdminTestHost.CreateAsync(
            CreateState(),
            resource =>
            {
                ConfigureHandlerCards(
                    resource,
                    rows => rows.LinkTo<Category>(row => row.CategoryId)
                );
                ConfigureRecordPages(resource);
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
        var cards = document.QuerySelectorAll(".sa-resource-card").ToArray();
        await Assert
            .That(cards[0].QuerySelectorAll("a").Select(link => link.GetAttribute("href")))
            .IsEquivalentTo([
                "/stellaradmin/categories/view/11",
                "/stellaradmin/categories/view/11",
            ]);
        await Assert.That(cards[0].QuerySelector("form")).IsNull();
        await Assert.That(cards[^1].QuerySelector("a")).IsNull();
    }

    [Test]
    public async Task NoRecords_ShowsTheEmptyState()
    {
        // Arrange
        await using var sut = await StellarAdminTestHost.CreateAsync(
            new ProductState([]),
            resource => resource.Index(index => index.AddCards("cards")),
            defaultTable: false
        );
        using var client = sut.CreateClient();

        // Act
        var document = await client.GetDocumentAsync("/stellaradmin/products");

        // Assert
        await Assert.That(document.QuerySelector(".sa-resource-cards")).IsNull();
        await Assert
            .That(document.RequiredElement(".sa-resource-cards-empty").TextContent)
            .Contains("No records found");
    }

    private static IEnumerable<string> CardTitles(AngleSharp.Dom.IDocument document) =>
        document
            .QuerySelectorAll(".sa-resource-card .sa-card-title")
            .Select(title => title.TextContent.Trim());

    // The products' cards, then cards of rows the handler loads, titled with their labels
    private static void ConfigureHandlerCards(
        ResourceBuilder<Product> resource,
        Action<ResourceHandlerCardsDataLayoutBuilder<Product, ProductRow>> configure
    ) =>
        resource.Index(index =>
        {
            index.EnableScopes(scopes =>
            {
                scopes.Add("all", "All");
                scopes.Add("cheap", "Cheap");
            });
            index.EnableSearch();
            index.AddCards("cards");
            index.AddCards<ProductRow, ProductRowsHandler>(
                "rows",
                rows =>
                {
                    rows.Card(card => card.Title(row => row.Label));
                    configure(rows);
                }
            );
        });

    private static void ConfigureRecordPages(ResourceBuilder<Product> resource)
    {
        resource
            .UseKey(product => product.Id)
            .AllowEdit(edit => edit.Fields(fields => fields.Add(product => product.Name)))
            .AllowView(view => view.Fields(fields => fields.Add(product => product.Name)));
        resource.AllowDelete();
    }

    private static void ConfigureSortedCards(ResourceBuilder<Product> resource) =>
        resource.Index(index =>
            index.AddCards(
                "cards",
                cards =>
                {
                    cards.DefaultSortByDescending(product => product.Price);
                    cards.EnablePaging(paging =>
                    {
                        paging.PageSize = 2;
                        paging.PageSizes = [2, 3];
                    });
                }
            )
        );

    private static ProductState CreateState() =>
        new([
            new Product(1, "Blue lamp", 10),
            new Product(2, "Desk", 100),
            new Product(3, "Red lamp", 70),
        ]);
}
