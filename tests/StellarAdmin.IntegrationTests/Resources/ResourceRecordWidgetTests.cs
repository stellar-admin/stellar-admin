using System.Net;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using StellarAdmin.IntegrationTests.Fixtures;
using StellarAdmin.IntegrationTests.Infrastructure;
using StellarAdmin.Resources.Builders;
using StellarAdmin.Widgets;
using static StellarAdmin.IntegrationTests.Infrastructure.FormTestHelpers;

namespace StellarAdmin.IntegrationTests.Resources;

public class ResourceRecordWidgetTests
{
    [Test]
    public async Task ViewPage_ShowsWidgetsAboveBesideAndBelowTheFields()
    {
        // Arrange
        await using var sut = await CreateHostAsync(resource =>
            resource.AllowView(view =>
                view.Fields(fields => fields.Add(product => product.Name))
                    .Widgets(widgets =>
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
                        widgets.Add("Above", "_RecordContextWidget");
                    })
            )
        );
        using var client = sut.GetTestClient();

        // Act
        var document = await client.GetDocumentAsync("/stellaradmin/products/view/7");

        // Assert
        await Assert
            .That(
                document
                    .QuerySelectorAll("[data-slot='widget']")
                    .Select(widget =>
                        $"{widget.Closest("[id^='view-page-widgets-']")!.Id}: {widget.GetAttribute("data-widget")}"
                    )
            )
            .IsEquivalentTo([
                "view-page-widgets-top: Above",
                "view-page-widgets-aside: Beside",
                "view-page-widgets-bottom: Below",
            ]);
        await Assert
            .That(document.TextContents("[data-test='record-context']"))
            .IsEquivalentTo(["7: Notebook"]);
        var aside = document.RequiredElement("#view-page-widgets-aside");
        await Assert.That(aside.HasAttribute("data-sticky")).IsFalse();
        await Assert.That(aside.PreviousElementSibling!.TextContent).Contains("Notebook");
    }

    [Test]
    public async Task ViewPage_WithoutWidgets_LeavesOutTheirRegions()
    {
        // Arrange
        await using var sut = await CreateHostAsync(resource =>
            resource.AllowView(view => view.Fields(fields => fields.Add(product => product.Name)))
        );
        using var client = sut.GetTestClient();

        // Act
        var document = await client.GetDocumentAsync("/stellaradmin/products/view/7");

        // Assert
        await Assert.That(document.QuerySelector("[id^='view-page-widgets-']")).IsNull();
        await Assert.That(document.QuerySelector(".sa-widgets-aside")).IsNull();
    }

    [Test]
    public async Task EditPage_ShowsAsideWidgetsBesideTheFormAndOutsideIt()
    {
        // Arrange
        await using var sut = await CreateHostAsync(resource =>
            resource.AllowEdit(edit =>
                edit.Fields(fields => fields.Add(product => product.Name))
                    .Widgets(widgets =>
                        widgets.Add(
                            "Context",
                            "_RecordContextWidget",
                            widget => widget.Placement = WidgetPlacement.Aside
                        )
                    )
            )
        );
        using var client = sut.GetTestClient();

        // Act
        var document = await client.GetDocumentAsync("/stellaradmin/products/edit/7");

        // Assert
        var aside = document.RequiredElement("#form-page-widgets-aside");
        await Assert.That(aside.HasAttribute("data-sticky")).IsTrue();
        await Assert.That(aside.Closest("form")).IsNull();
        await Assert
            .That(aside.ParentElement!.QuerySelector("form[data-resource-form]"))
            .IsNotNull();
        await Assert
            .That(document.TextContents("[data-test='record-context']"))
            .IsEquivalentTo(["7: Notebook"]);
    }

    [Test]
    public async Task EditPage_ShownAgainForInvalidValues_GivesWidgetsTheSubmittedValues()
    {
        // Arrange
        await using var sut = await CreateHostAsync(resource =>
            resource.AllowEdit(edit =>
                edit.Fields(fields =>
                    {
                        fields.Add(product => product.Name);
                        fields.Add(product => product.Price);
                    })
                    .Widgets(widgets => widgets.Add("Context", "_RecordContextWidget"))
            )
        );
        using var client = sut.GetTestClient();
        var values = await PrepareForm(client, "/stellaradmin/products/edit/7");
        values["Entity.Name"] = "Fountain pen";
        values["Entity.Price"] = "0";
        using var content = new FormUrlEncodedContent(values);

        // Act
        using var response = await client.PostAsync("/stellaradmin/products/edit/7", content);
        var document = await response.ReadDocumentAsync();

        // Assert
        await Assert.That(response.StatusCode).IsEqualTo(HttpStatusCode.OK);
        await Assert
            .That(document.TextContents("#form-page-widgets-top [data-test='record-context']"))
            .IsEquivalentTo(["7: Fountain pen"]);
    }

    [Test]
    public async Task CreatePage_ShowsItsOwnWidgets()
    {
        // Arrange
        await using var sut = await CreateHostAsync(resource =>
        {
            resource.AllowCreate(create =>
                create
                    .Fields(fields => fields.Add(product => product.Name))
                    .Widgets(widgets =>
                        widgets.Add(
                            "Context",
                            "_CreateContextWidget",
                            widget => widget.Placement = WidgetPlacement.Bottom
                        )
                    )
            );
            resource.AllowEdit(edit =>
                edit.Widgets(widgets => widgets.Add("Context", "_RecordContextWidget"))
            );
        });
        using var client = sut.GetTestClient();

        // Act
        var document = await client.GetDocumentAsync("/stellaradmin/products/create");

        // Assert
        await Assert
            .That(document.TextContents("#form-page-widgets-bottom [data-test='create-context']"))
            .IsEquivalentTo(["Creating"]);
        await Assert.That(document.QuerySelector("[data-test='record-context']")).IsNull();
    }

    [Test]
    public async Task HandlerWidget_LoadsFromTheViewPageWithTheRecordsParameters()
    {
        // Arrange
        await using var sut = await CreateHostAsync(resource =>
            resource.AllowView(view =>
                view.Fields(fields => fields.Add(product => product.Name))
                    .Widgets(widgets =>
                        widgets.Add<SalesHandler>(
                            "Sales",
                            "_SalesWidget",
                            widget =>
                                widget.Parameters(context => new SalesParameters(
                                    context.Record.Name,
                                    30
                                ))
                        )
                    )
            )
        );
        using var client = sut.GetTestClient();

        // Act
        var page = await client.GetDocumentAsync("/stellaradmin/products/view/7");
        var url = page.RequiredElement("[data-slot='widget']").GetAttribute("hx-get")!;
        var loaded = await client.GetDocumentAsync(url);

        // Assert
        await Assert.That(url).StartsWith("/stellaradmin/products/widget/Sales?page=view&p=");
        await Assert
            .That(loaded.TextContents("[data-test='sales']"))
            .IsEquivalentTo(["Notebook: 30 days, 3000"]);
    }

    [Test]
    public async Task ParametersOfAnotherPage_AreRejected()
    {
        // Arrange
        await using var sut = await CreateHostAsync(resource =>
        {
            resource.AllowView(view =>
                view.Fields(fields => fields.Add(product => product.Name))
                    .Widgets(widgets => AddSales(widgets, "View"))
            );
            resource.AllowEdit(edit =>
                edit.Fields(fields => fields.Add(product => product.Name))
                    .Widgets(widgets => AddSales(widgets, "Edit"))
            );
        });
        using var client = sut.GetTestClient();
        var view = await client.GetDocumentAsync("/stellaradmin/products/view/7");
        var url = view.RequiredElement("[data-slot='widget']").GetAttribute("hx-get")!;

        // Act
        var response = await client.GetAsync(url.Replace("page=view", "page=edit"));

        // Assert
        await Assert.That(response.StatusCode).IsEqualTo(HttpStatusCode.BadRequest);
    }

    [Test]
    [Arguments("edit")]
    [Arguments("other")]
    public async Task WidgetOfAPageTheResourceDoesntHave_IsNotFound(string page)
    {
        // Arrange
        await using var sut = await CreateHostAsync(resource =>
            resource.AllowView(view =>
                view.Fields(fields => fields.Add(product => product.Name))
                    .Widgets(widgets => widgets.Add<GreetingHandler>("Greeting", "_GreetingWidget"))
            )
        );
        using var client = sut.GetTestClient();

        // Act
        var found = await client.GetAsync("/stellaradmin/products/widget/Greeting?page=view");
        var missing = await client.GetAsync($"/stellaradmin/products/widget/Greeting?page={page}");

        // Assert
        await Assert.That(found.StatusCode).IsEqualTo(HttpStatusCode.OK);
        await Assert.That(missing.StatusCode).IsEqualTo(HttpStatusCode.NotFound);
    }

    [Test]
    public async Task InlineRowWidgets_AreReportedAndLeftOut()
    {
        // Arrange
        await using var sut = await CreateHostAsync(resource =>
            resource.AllowEdit(edit =>
            {
                edit.Fields(fields => fields.Add(product => product.Name));
                edit.UseInlineRow();
                edit.Widgets(widgets =>
                    widgets.Add<GreetingHandler>("Greeting", "_GreetingWidget")
                );
            })
        );
        using var client = sut.GetTestClient();

        // Act
        var response = await client.GetAsync("/stellaradmin/products/widget/Greeting?page=edit");

        // Assert
        await Assert.That(response.StatusCode).IsEqualTo(HttpStatusCode.NotFound);
        var logs = sut.Services.GetRequiredService<RecordingLoggerProvider>();
        await Assert
            .That(logs.ConfigurationProblems.Select(problem => problem.EventId.Name))
            .IsEquivalentTo(["IgnoredWidgetSetting"]);
    }

    private static void AddSales(
        WidgetsBuilder<RecordWidgetContext<Product>> widgets,
        string region
    ) =>
        widgets.Add<SalesHandler>(
            "Sales",
            "_SalesWidget",
            widget => widget.Parameters(_ => new SalesParameters(region, 30))
        );

    private static Task<Microsoft.AspNetCore.Builder.WebApplication> CreateHostAsync(
        Action<ResourceBuilder<Product>> configure
    ) =>
        StellarAdminTestHost.CreateAsync(
            new([new(7, "Notebook", 8.50m)]),
            resource =>
            {
                resource.UseKey(product => product.Id);
                configure(resource);
            },
            environmentName: "Production"
        );
}
