using System.Net;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using StellarAdmin.IntegrationTests.Fixtures;
using StellarAdmin.IntegrationTests.Infrastructure;
using StellarAdmin.Widgets;
using TUnit.Assertions.Enums;

namespace StellarAdmin.IntegrationTests.Home;

public class HomePageWidgetTests
{
    [Test]
    public async Task Widgets_TakeThePlaceOfResourceLinks()
    {
        // Arrange
        await using var sut = await CreateHostAsync(widgets =>
            widgets.Add("Notice", "_NoticeWidget")
        );
        using var client = sut.GetTestClient();

        // Act
        var document = await client.GetDocumentAsync("/stellaradmin");

        // Assert
        await Assert
            .That(document.TextContents("[data-test='notice']"))
            .IsEquivalentTo(["Signed in as nobody"]);
        await Assert.That(document.QuerySelector("a[data-slot='item']")).IsNull();
    }

    [Test]
    public async Task NoVisibleWidgets_LinksToResources()
    {
        // Arrange
        await using var sut = await CreateHostAsync(widgets =>
            widgets.Add("Notice", "_NoticeWidget", widget => widget.ShowWhen(_ => false))
        );
        using var client = sut.GetTestClient();

        // Act
        var document = await client.GetDocumentAsync("/stellaradmin");

        // Assert
        await Assert.That(document.QuerySelector("[data-slot='widget']")).IsNull();
        await Assert
            .That(
                document
                    .QuerySelectorAll("a[data-slot='item']")
                    .Select(link => link.GetAttribute("href"))
            )
            .IsEquivalentTo(["/stellaradmin/products"]);
    }

    [Test]
    public async Task Development_ShowsChecklistAboveWidgets()
    {
        // Arrange
        await using var sut = await CreateHostAsync(
            widgets => widgets.Add("Notice", "_NoticeWidget"),
            "Development"
        );
        using var client = sut.GetTestClient();

        // Act
        var document = await client.GetDocumentAsync("/stellaradmin");

        // Assert
        var checklist = document.RequiredElement("[data-slot='checklist']");
        var widget = document.RequiredElement("[data-test='notice']");
        await Assert
            .That(
                checklist.CompareDocumentPosition(widget)
                    & AngleSharp.Dom.DocumentPositions.Following
            )
            .IsNotEqualTo((AngleSharp.Dom.DocumentPositions)0);
    }

    [Test]
    public async Task HandlerWidget_LoadsAfterThePage()
    {
        // Arrange
        await using var sut = await CreateHostAsync(widgets =>
            widgets.Add<GreetingHandler>("Greeting", "_GreetingWidget", widget => widget.Span = 4)
        );
        using var client = sut.GetTestClient();

        // Act
        var page = await client.GetDocumentAsync("/stellaradmin");
        var placeholder = page.RequiredElement("[data-slot='widget']");
        var loaded = await client.GetDocumentAsync(placeholder.GetAttribute("hx-get")!);

        // Assert
        await Assert.That(placeholder.GetAttribute("data-state")).IsEqualTo("loading");
        await Assert.That(placeholder.GetAttribute("data-span")).IsEqualTo("4");
        await Assert.That(placeholder.GetAttribute("hx-trigger")).IsEqualTo("load");
        await Assert.That(page.QuerySelector("[data-test='greeting']")).IsNull();
        await Assert
            .That(loaded.TextContents("[data-test='greeting']"))
            .IsEquivalentTo(["Hello, guest from Greeting"]);
        await Assert
            .That(loaded.RequiredElement("[data-slot='widget']").GetAttribute("data-state"))
            .IsEqualTo("loaded");
    }

    [Test]
    public async Task InlineWidget_LoadsWithThePage()
    {
        // Arrange
        await using var sut = await CreateHostAsync(widgets =>
            widgets.Add<GreetingHandler>(
                "Greeting",
                "_GreetingWidget",
                widget => widget.Inline = true
            )
        );
        using var client = sut.GetTestClient();

        // Act
        var document = await client.GetDocumentAsync("/stellaradmin");

        // Assert
        await Assert
            .That(document.TextContents("[data-test='greeting']"))
            .IsEquivalentTo(["Hello, guest from Greeting"]);
        await Assert
            .That(document.RequiredElement("[data-slot='widget']").HasAttribute("hx-get"))
            .IsFalse();
    }

    [Test]
    public async Task RefreshInterval_LoadsTheWidgetAgain()
    {
        // Arrange
        await using var sut = await CreateHostAsync(widgets =>
            widgets.Add<GreetingHandler>(
                "Greeting",
                "_GreetingWidget",
                widget =>
                {
                    widget.Inline = true;
                    widget.RefreshInterval = TimeSpan.FromMinutes(5);
                }
            )
        );
        using var client = sut.GetTestClient();

        // Act
        var document = await client.GetDocumentAsync("/stellaradmin");

        // Assert
        var widget = document.RequiredElement("[data-slot='widget']");
        await Assert.That(widget.GetAttribute("hx-trigger")).IsEqualTo("every 300s");
        await Assert.That(widget.GetAttribute("hx-get")).StartsWith("/stellaradmin/home/widget/");
        await Assert.That(widget.GetAttribute("hx-status:5xx")).IsEqualTo("swap:none");
    }

    [Test]
    public async Task Parameters_ReachTheHandler()
    {
        // Arrange
        await using var sut = await CreateHostAsync(widgets =>
            widgets.Add<SalesHandler>(
                "Sales",
                "_SalesWidget",
                widget => widget.Parameters(_ => new SalesParameters("Europe", 30))
            )
        );
        using var client = sut.GetTestClient();

        // Act
        var loaded = await LoadWidgetAsync(client);

        // Assert
        await Assert
            .That(loaded.TextContents("[data-test='sales']"))
            .IsEquivalentTo(["Europe: 30 days, 3000"]);
    }

    [Test]
    public async Task WidgetInput_ChangesOnlyMarkedProperties()
    {
        // Arrange
        await using var sut = await CreateHostAsync(widgets =>
            widgets.Add<SalesHandler>(
                "Sales",
                "_SalesWidget",
                widget => widget.Parameters(_ => new SalesParameters("Europe", 30))
            )
        );
        using var client = sut.GetTestClient();
        var form = (await LoadWidgetAsync(client)).RequiredElement("form[data-slot='widget-form']");

        // Act
        var changed = await client.GetDocumentAsync(
            $"{form.GetAttribute("hx-get")}&Days=90&Region=Asia"
        );
        var refreshed = await client.GetDocumentAsync(
            changed.RequiredElement("form[data-slot='widget-form']").GetAttribute("hx-get")!
        );

        // Assert
        await Assert.That(form.GetAttribute("hx-target")).IsEqualTo("closest [data-slot='widget']");
        await Assert
            .That(changed.TextContents("[data-test='sales']"))
            .IsEquivalentTo(["Europe: 90 days, 9000"]);
        await Assert
            .That(refreshed.TextContents("[data-test='sales']"))
            .IsEquivalentTo(["Europe: 90 days, 9000"]);
    }

    [Test]
    public async Task InvalidWidgetInput_ShowsTheError()
    {
        // Arrange
        await using var sut = await CreateHostAsync(widgets =>
            widgets.Add<SalesHandler>(
                "Sales",
                "_SalesWidget",
                widget => widget.Parameters(_ => new SalesParameters("Europe", 30))
            )
        );
        using var client = sut.GetTestClient();
        var page = await client.GetDocumentAsync("/stellaradmin");
        var url = page.RequiredElement("[data-slot='widget']").GetAttribute("hx-get")!;

        // Act
        using var response = await client.GetAsync($"{url}&Days=many");
        var document = await response.ReadDocumentAsync();

        // Assert
        await Assert.That(response.StatusCode).IsEqualTo(HttpStatusCode.BadRequest);
        await Assert
            .That(document.RequiredElement("[data-slot='widget']").GetAttribute("data-state"))
            .IsEqualTo("failed");
    }

    [Test]
    [Arguments("")]
    [Arguments("tampered")]
    public async Task ChangedParameters_AreRejected(string payload)
    {
        // Arrange
        await using var sut = await CreateHostAsync(widgets =>
            widgets.Add<SalesHandler>(
                "Sales",
                "_SalesWidget",
                widget => widget.Parameters(_ => new SalesParameters("Europe", 30))
            )
        );
        using var client = sut.GetTestClient();

        // Act
        using var response = await client.GetAsync($"/stellaradmin/home/widget/Sales?p={payload}");

        // Assert
        await Assert.That(response.StatusCode).IsEqualTo(HttpStatusCode.BadRequest);
        await Assert.That(await response.Content.ReadAsStringAsync()).DoesNotContain("Europe");
    }

    [Test]
    public async Task ParametersOfAnotherWidget_AreRejected()
    {
        // Arrange
        await using var sut = await CreateHostAsync(widgets =>
        {
            widgets.Add<SalesHandler>(
                "Sales",
                "_SalesWidget",
                widget => widget.Parameters(_ => new SalesParameters("Europe", 30))
            );
            widgets.Add<SalesHandler>(
                "Other sales",
                "_SalesWidget",
                widget => widget.Parameters(_ => new SalesParameters("Asia", 30))
            );
        });
        using var client = sut.GetTestClient();
        var page = await client.GetDocumentAsync("/stellaradmin");
        var europe = new Uri(
            new Uri("http://localhost"),
            page.RequiredElement("[data-widget='Sales']").GetAttribute("hx-get")!
        );

        // Act
        using var response = await client.GetAsync(
            $"/stellaradmin/home/widget/Other%20sales{europe.Query}"
        );

        // Assert
        await Assert.That(response.StatusCode).IsEqualTo(HttpStatusCode.BadRequest);
    }

    [Test]
    public async Task HandlerReturningNull_HidesTheWidget()
    {
        // Arrange
        await using var sut = await CreateHostAsync(widgets =>
        {
            widgets.Add<NothingToShowHandler>(
                "Inline",
                "_GreetingWidget",
                widget => widget.Inline = true
            );
            widgets.Add<NothingToShowHandler>("Lazy", "_GreetingWidget");
        });
        using var client = sut.GetTestClient();

        // Act
        var page = await client.GetDocumentAsync("/stellaradmin");
        using var response = await client.GetAsync("/stellaradmin/home/widget/Lazy");

        // Assert
        await Assert
            .That(
                page.QuerySelectorAll("[data-slot='widget']")
                    .Select(widget => widget.GetAttribute("data-widget"))
            )
            .IsEquivalentTo(["Lazy"]);
        await Assert.That(response.StatusCode).IsEqualTo(HttpStatusCode.OK);
        await Assert.That(response.Headers.GetValues("HX-Reswap")).IsEquivalentTo(["delete"]);
    }

    [Test]
    public async Task FailingHandler_ShowsAnErrorWithRetry()
    {
        // Arrange
        await using var sut = await CreateHostAsync(widgets =>
        {
            widgets.Add<FailingWidgetHandler>(
                "Inline",
                "_GreetingWidget",
                widget => widget.Inline = true
            );
            widgets.Add<FailingWidgetHandler>("Lazy", "_GreetingWidget");
        });
        using var client = sut.GetTestClient();

        // Act
        var page = await client.GetDocumentAsync("/stellaradmin");
        using var response = await client.GetAsync("/stellaradmin/home/widget/Lazy");
        var lazy = await response.ReadDocumentAsync();

        // Assert
        var inline = page.RequiredElement("[data-widget='Inline']");
        await Assert.That(inline.GetAttribute("data-state")).IsEqualTo("failed");
        await Assert
            .That(inline.RequiredElement("button[hx-get]").GetAttribute("hx-get"))
            .IsEqualTo("/stellaradmin/home/widget/Inline");
        await Assert.That(response.StatusCode).IsEqualTo(HttpStatusCode.InternalServerError);
        await Assert
            .That(lazy.RequiredElement("[data-slot='widget']").GetAttribute("data-state"))
            .IsEqualTo("failed");
        var logs = sut.Services.GetRequiredService<RecordingLoggerProvider>();
        await Assert
            .That(
                logs.Logs.Count(log =>
                    log.Level == LogLevel.Error && log.Message.Contains("FailingWidgetHandler")
                )
            )
            .IsEqualTo(2);
    }

    [Test]
    public async Task RequiredAuthorization_HidesTheWidgetAndRefusesItsRequest()
    {
        // Arrange
        await using var sut = await CreateHostAsync(
            widgets =>
            {
                widgets.Add<GreetingHandler>(
                    "Revenue",
                    "_GreetingWidget",
                    widget => widget.RequireAuthorization(policy => policy.RequireRole("Finance"))
                );
                widgets.Add<GreetingHandler>("Greeting", "_GreetingWidget");
            },
            configureServices: TestAuthenticationHandler.Register
        );
        using var client = sut.GetTestClient();
        TestAuthenticationHandler.SignIn(client, "ada", "Staff");

        // Act
        var page = await client.GetDocumentAsync("/stellaradmin");
        using var response = await client.GetAsync("/stellaradmin/home/widget/Revenue");

        // Assert
        await Assert
            .That(
                page.QuerySelectorAll("[data-slot='widget']")
                    .Select(widget => widget.GetAttribute("data-widget"))
            )
            .IsEquivalentTo(["Greeting"]);
        await Assert.That(response.StatusCode).IsEqualTo(HttpStatusCode.NotFound);
    }

    [Test]
    public async Task UnusableWidgets_AreLeftOutAndReported()
    {
        // Arrange
        await using var sut = await CreateHostAsync(widgets =>
        {
            widgets.Add<AmbiguousWidgetHandler>("Ambiguous", "_GreetingWidget");
            widgets.Add<SalesHandler>("Unset", "_SalesWidget");
            widgets.Add<GreetingHandler>(
                "Extra",
                "_GreetingWidget",
                widget => widget.Parameters(_ => 1)
            );
            widgets.Add<SalesHandler>(
                "Mismatched",
                "_SalesWidget",
                widget => widget.Parameters(_ => "Europe")
            );
            widgets.Add("Notice", "_NoticeWidget");
            widgets.Add("notice", "_NoticeWidget");
            widgets.Add(" ", "_NoticeWidget");
        });
        using var client = sut.GetTestClient();

        // Act
        var page = await client.GetDocumentAsync("/stellaradmin");

        // Assert
        await Assert
            .That(
                page.QuerySelectorAll("[data-slot='widget']")
                    .Select(widget => widget.GetAttribute("data-widget"))
            )
            .IsEquivalentTo(["Notice"]);
        var logs = sut.Services.GetRequiredService<RecordingLoggerProvider>();
        await Assert
            .That(logs.ConfigurationProblems.Select(problem => problem.EventId.Name))
            .IsEquivalentTo(
                [
                    "UnusableWidgetHandler",
                    "MismatchedWidgetParameters",
                    "MismatchedWidgetParameters",
                    "MismatchedWidgetParameters",
                    "RepeatedWidgetName",
                    "BlankWidgetName",
                ],
                CollectionOrdering.Any
            );
    }

    [Test]
    public async Task UnusableSettings_AreResetAndReported()
    {
        // Arrange
        await using var sut = await CreateHostAsync(widgets =>
        {
            widgets.Add<GreetingHandler>(
                "Greeting",
                "_GreetingWidget",
                widget =>
                {
                    widget.Span = 13;
                    widget.RefreshInterval = TimeSpan.FromMilliseconds(10);
                }
            );
            widgets.Add(
                "Notice",
                "_NoticeWidget",
                widget =>
                {
                    widget.Parameters(_ => 1);
                    widget.RefreshInterval = TimeSpan.FromMinutes(1);
                }
            );
        });
        using var client = sut.GetTestClient();

        // Act
        var page = await client.GetDocumentAsync("/stellaradmin");

        // Assert
        await Assert
            .That(page.RequiredElement("[data-widget='Greeting']").GetAttribute("data-span"))
            .IsEqualTo("12");
        await Assert
            .That(page.RequiredElement("[data-widget='Notice']").HasAttribute("hx-trigger"))
            .IsFalse();
        var logs = sut.Services.GetRequiredService<RecordingLoggerProvider>();
        await Assert
            .That(logs.ConfigurationProblems.Select(problem => problem.EventId.Name))
            .IsEquivalentTo(
                ["InvalidValue", "InvalidValue", "IgnoredWidgetSetting", "IgnoredWidgetSetting"],
                CollectionOrdering.Any
            );
    }

    private static Task<Microsoft.AspNetCore.Builder.WebApplication> CreateHostAsync(
        Action<WidgetsBuilder<WidgetContext>> configure,
        string environmentName = "Production",
        Action<IServiceCollection>? configureServices = null
    ) =>
        StellarAdminTestHost.CreateAsync(
            new([]),
            configureAdmin: admin => admin.ConfigureHomePage(home => home.Widgets(configure)),
            configureServices: configureServices,
            environmentName: environmentName
        );

    private static async Task<AngleSharp.Html.Dom.IHtmlDocument> LoadWidgetAsync(HttpClient client)
    {
        var page = await client.GetDocumentAsync("/stellaradmin");

        return await client.GetDocumentAsync(
            page.RequiredElement("[data-slot='widget']").GetAttribute("hx-get")!
        );
    }
}
