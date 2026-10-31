using System.Net;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using StellarAdmin.IntegrationTests.Fixtures;
using StellarAdmin.IntegrationTests.Infrastructure;
using StellarAdmin.Resources.Builders;
using StellarAdmin.Widgets;
using TUnit.Assertions.Enums;

namespace StellarAdmin.IntegrationTests.Resources;

public class ResourceIndexWidgetTests
{
    [Test]
    public async Task Placements_ShowWidgetsAboveBesideAndBelowTheGrid()
    {
        // Arrange
        await using var sut = await CreateHostAsync(widgets =>
        {
            widgets.Add(
                "Below",
                "_NoticeWidget",
                widget => widget.Placement = WidgetPlacement.Bottom
            );
            widgets.Add(
                "Beside",
                "_NoticeWidget",
                widget => widget.Placement = WidgetPlacement.Aside
            );
            widgets.Add("Above", "_NoticeWidget");
        });
        using var client = sut.GetTestClient();

        // Act
        var document = await client.GetDocumentAsync("/stellaradmin/products");

        // Assert
        await Assert
            .That(
                document
                    .QuerySelectorAll("[data-slot='widget']")
                    .Select(widget =>
                        $"{widget.Closest("[id^='index-page-widgets-']")!.Id}: {widget.GetAttribute("data-widget")}"
                    )
            )
            .IsEquivalentTo([
                "index-page-widgets-top: Above",
                "index-page-widgets-aside: Beside",
                "index-page-widgets-bottom: Below",
            ]);
        var grid = document.RequiredElement("#index-page-data-grid");
        await Assert
            .That(grid.Closest(".sa-resource-index-list")!.ParentElement!.ClassName)
            .IsEqualTo("sa-widgets-aside-grid");
        await Assert
            .That(
                document.RequiredElement("#index-page-widgets-bottom").CompareDocumentPosition(grid)
                    & AngleSharp.Dom.DocumentPositions.Preceding
            )
            .IsNotEqualTo((AngleSharp.Dom.DocumentPositions)0);
    }

    [Test]
    public async Task NoWidgets_LeavesOutTheirRegions()
    {
        // Arrange
        await using var sut = await StellarAdminTestHost.CreateAsync(new([]));
        using var client = sut.GetTestClient();

        // Act
        var document = await client.GetDocumentAsync("/stellaradmin/products");

        // Assert
        await Assert.That(document.QuerySelector("[id^='index-page-widgets-']")).IsNull();
        await Assert.That(document.QuerySelector(".sa-widgets-aside")).IsNull();
    }

    [Test]
    public async Task HiddenWidgets_KeepTheirRegionHidden()
    {
        // Arrange
        await using var sut = await CreateHostAsync(widgets =>
            widgets.Add(
                "Beside",
                "_NoticeWidget",
                widget =>
                {
                    widget.Placement = WidgetPlacement.Aside;
                    widget.ShowWhen(context => context.Search is not null);
                }
            )
        );
        using var client = sut.GetTestClient();

        // Act
        var hidden = await client.GetDocumentAsync("/stellaradmin/products");
        var shown = await client.GetDocumentAsync("/stellaradmin/products?search=lamp");

        // Assert
        await Assert
            .That(hidden.RequiredElement("#index-page-widgets-aside").HasAttribute("hidden"))
            .IsTrue();
        await Assert.That(hidden.QuerySelector("[data-slot='widget']")).IsNull();
        await Assert
            .That(shown.RequiredElement("#index-page-widgets-aside").HasAttribute("hidden"))
            .IsFalse();
    }

    [Test]
    public async Task Context_HasTheSearchAndScope()
    {
        // Arrange
        await using var sut = await CreateHostAsync(
            widgets => widgets.Add("Context", "_IndexContextWidget"),
            index =>
                index.EnableScopes(scopes =>
                {
                    scopes.Add("all", "All products");
                    scopes.Add("under-50", "Under 50");
                    scopes.DefaultScope = "all";
                })
        );
        using var client = sut.GetTestClient();

        // Act
        var unfiltered = await client.GetDocumentAsync("/stellaradmin/products");
        var filtered = await client.GetDocumentAsync(
            "/stellaradmin/products?scope=under-50&search=%20lamp%20"
        );

        // Assert
        await Assert
            .That(unfiltered.TextContents("[data-test='index-context']"))
            .IsEquivalentTo(["all, no search"]);
        await Assert
            .That(filtered.TextContents("[data-test='index-context']"))
            .IsEquivalentTo(["under-50, lamp"]);
    }

    [Test]
    public async Task Search_RendersTheWidgetsAgain()
    {
        // Arrange
        await using var sut = await CreateHostAsync(widgets =>
        {
            widgets.Add("Above", "_IndexContextWidget");
            widgets.Add(
                "Below",
                "_NoticeWidget",
                widget => widget.Placement = WidgetPlacement.Bottom
            );
        });
        using var client = sut.GetTestClient();

        // Act
        var page = await client.GetDocumentAsync("/stellaradmin/products");
        var search = await GetGridDocumentAsync(
            client,
            "/stellaradmin/products?search=lamp",
            "input#index-page-search-input"
        );
        var nextPage = await GetGridDocumentAsync(
            client,
            "/stellaradmin/products?search=lamp",
            "a"
        );

        // Assert
        await Assert
            .That(page.RequiredElement("input[name='search']").GetAttribute("hx-select-oob"))
            .IsEqualTo("#index-page-widgets-top, #index-page-widgets-bottom");
        await Assert
            .That(search.TextContents("#index-page-widgets-top [data-test='index-context']"))
            .IsEquivalentTo(["no scope, lamp"]);
        await Assert.That(nextPage.QuerySelector("[data-slot='widget']")).IsNull();
        await Assert
            .That(nextPage.RequiredElement("input[name='search']").GetAttribute("hx-select-oob"))
            .IsEqualTo("#index-page-widgets-top, #index-page-widgets-bottom");
        await Assert.That(nextPage.QuerySelector("#index-page-data-grid")).IsNotNull();
    }

    [Test]
    public async Task HandlerWidget_LoadsWithTheSearchFromTheResource()
    {
        // Arrange
        await using var sut = await CreateHostAsync(widgets =>
            widgets.Add<SalesHandler>(
                "Sales",
                "_SalesWidget",
                widget =>
                    widget.Parameters(context => new SalesParameters(context.Search ?? "All", 30))
            )
        );
        using var client = sut.GetTestClient();

        // Act
        var page = await client.GetDocumentAsync("/stellaradmin/products?search=lamp");
        var url = page.RequiredElement("[data-slot='widget']").GetAttribute("hx-get")!;
        var loaded = await client.GetDocumentAsync(url);

        // Assert
        await Assert.That(url).StartsWith("/stellaradmin/products/widget/Sales?p=");
        await Assert
            .That(loaded.TextContents("[data-test='sales']"))
            .IsEquivalentTo(["lamp: 30 days, 3000"]);
    }

    [Test]
    public async Task ParametersOfAnotherPage_AreRejected()
    {
        // Arrange
        await using var sut = await StellarAdminTestHost.CreateAsync(
            new([]),
            resource =>
                resource.Index(index =>
                    index.Widgets(widgets =>
                        widgets.Add<SalesHandler>(
                            "Sales",
                            "_SalesWidget",
                            widget => widget.Parameters(_ => new SalesParameters("Index", 30))
                        )
                    )
                ),
            admin =>
                admin.ConfigureHomePage(home =>
                    home.Widgets(widgets =>
                        widgets.Add<SalesHandler>(
                            "Sales",
                            "_SalesWidget",
                            widget => widget.Parameters(_ => new SalesParameters("Home", 30))
                        )
                    )
                ),
            environmentName: "Production"
        );
        using var client = sut.GetTestClient();
        var home = await client.GetDocumentAsync("/stellaradmin");
        var payload = new Uri(
            "http://localhost" + home.RequiredElement("[data-slot='widget']").GetAttribute("hx-get")
        ).Query;

        // Act
        var response = await client.GetAsync("/stellaradmin/products/widget/Sales" + payload);

        // Assert
        await Assert.That(response.StatusCode).IsEqualTo(HttpStatusCode.BadRequest);
    }

    [Test]
    public async Task UnknownWidget_IsNotFound()
    {
        // Arrange
        await using var sut = await CreateHostAsync(widgets =>
            widgets.Add<GreetingHandler>("Greeting", "_GreetingWidget")
        );
        using var client = sut.GetTestClient();

        // Act
        var response = await client.GetAsync("/stellaradmin/products/widget/Missing");

        // Assert
        await Assert.That(response.StatusCode).IsEqualTo(HttpStatusCode.NotFound);
    }

    [Test]
    public async Task UnusableWidgets_AreReportedForTheResource()
    {
        // Arrange
        await using var sut = await CreateHostAsync(widgets =>
        {
            widgets.Add("Notice", "_NoticeWidget");
            widgets.Add("notice", "_NoticeWidget");
            widgets.Add<GreetingHandler>("Greeting", "_GreetingWidget", widget => widget.Span = 0);
        });
        using var client = sut.GetTestClient();

        // Act
        var document = await client.GetDocumentAsync("/stellaradmin/products");

        // Assert
        await Assert
            .That(
                document
                    .QuerySelectorAll("[data-slot='widget']")
                    .Select(widget => widget.GetAttribute("data-widget"))
            )
            .IsEquivalentTo(["Notice", "Greeting"]);
        var logs = sut.Services.GetRequiredService<RecordingLoggerProvider>();
        await Assert
            .That(logs.ConfigurationProblems.Select(problem => problem.EventId.Name))
            .IsEquivalentTo(["RepeatedWidgetName", "InvalidValue"], CollectionOrdering.Any);
    }

    private static Task<Microsoft.AspNetCore.Builder.WebApplication> CreateHostAsync(
        Action<WidgetsBuilder<IndexWidgetContext<Product>>> configure,
        Action<ResourceIndexBuilder<Product>>? configureIndex = null
    ) =>
        StellarAdminTestHost.CreateAsync(
            new([]),
            resource =>
                resource.Index(index =>
                {
                    index.EnableSearch();
                    configureIndex?.Invoke(index);
                    index.Widgets(configure);
                }),
            environmentName: "Production"
        );

    // A request from htmx that replaces only the grid, from the search box or from a link in the grid
    private static async Task<AngleSharp.Html.Dom.IHtmlDocument> GetGridDocumentAsync(
        HttpClient client,
        string url,
        string source
    )
    {
        using var request = new HttpRequestMessage(HttpMethod.Get, url);
        request.Headers.Add("HX-Request", "true");
        request.Headers.Add("HX-Target", "div#index-page-data-grid");
        request.Headers.Add("HX-Source", source);
        using var response = await client.SendAsync(request);
        response.EnsureSuccessStatusCode();

        return await response.ReadDocumentAsync();
    }
}
