using System.Net;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using StellarAdmin.IntegrationTests.Fixtures;
using StellarAdmin.IntegrationTests.Infrastructure;
using StellarAdmin.Resources.Editors;
using StellarAdmin.Resources.Options;
using StellarAdmin.UI.Icons;

namespace StellarAdmin.IntegrationTests.Diagnostics;

public class ConfigurationProblemsTests
{
    [Test]
    public async Task BrokenResource_TurnsOffOnlyThatResource()
    {
        // Arrange
        await using var sut = await StellarAdminTestHost.CreateAsync(
            new([new(1, "Notebook", 12.50m)]),
            configureAdmin: admin =>
            {
                admin.AddResource<CustomProduct>(
                    "catalog",
                    resource => resource.UseDataSource<CustomProductDataSource>()
                );
                admin.Services.Configure<ResourceOptions<CustomProduct>>(_ =>
                    throw new InvalidOperationException("Broken catalog")
                );
            }
        );
        using var client = sut.GetTestClient();

        // Act
        using var broken = await client.GetAsync("/stellaradmin/catalog");
        var brokenBody = await broken.Content.ReadAsStringAsync();
        var document = await client.GetDocumentAsync("/stellaradmin/products");

        // Assert
        await Assert.That(broken.StatusCode).IsEqualTo(HttpStatusCode.NotFound);
        await Assert.That(brokenBody).Contains("This resource is turned off");
        await Assert.That(brokenBody).Contains("Broken catalog");
        await Assert
            .That(
                document.TextContents(
                    "[data-slot='sidebar-group'] [data-slot='sidebar-menu-button']"
                )
            )
            .IsEquivalentTo(["Products"]);
        var logs = sut.Services.GetRequiredService<RecordingLoggerProvider>();
        await Assert
            .That(logs.ConfigurationProblems.Single().Message)
            .Contains("InvalidOperationException: Broken catalog");
    }

    [Test]
    public async Task BrokenResourceOutsideDevelopment_ReturnsPlainNotFound()
    {
        // Arrange
        await using var sut = await StellarAdminTestHost.CreateAsync(
            new([]),
            configureAdmin: admin =>
            {
                admin.AddResource<CustomProduct>(
                    "catalog",
                    resource => resource.UseDataSource<CustomProductDataSource>()
                );
                admin.Services.Configure<ResourceOptions<CustomProduct>>(_ =>
                    throw new InvalidOperationException("Broken catalog")
                );
            },
            environmentName: "Production"
        );
        using var client = sut.GetTestClient();

        // Act
        using var response = await client.GetAsync("/stellaradmin/catalog");

        // Assert
        await Assert.That(response.StatusCode).IsEqualTo(HttpStatusCode.NotFound);
        await Assert
            .That(await response.Content.ReadAsStringAsync())
            .DoesNotContain("Broken catalog");
    }

    [Test]
    public async Task EditorWithAnotherEditorsTemplate_LeavesFieldOutAndReportsIt()
    {
        // Arrange
        await using var sut = await StellarAdminTestHost.CreateAsync(
            new([]),
            resource =>
                resource.AllowCreate(create =>
                    create.Fields(fields =>
                    {
                        fields.Add(product => product.Name).UseEditor<MismatchedEditor>();
                        fields.Add(product => product.Price);
                    })
                )
        );
        using var client = sut.GetTestClient();

        // Act
        var first = await client.GetDocumentAsync("/stellaradmin/products/create");
        var second = await client.GetDocumentAsync("/stellaradmin/products/create");

        // Assert
        var logs = sut.Services.GetRequiredService<RecordingLoggerProvider>();
        await Assert.That(first.QuerySelector("[name='Entity.Name']")).IsNull();
        await Assert.That(first.QuerySelector("[name='Entity.Price']")).IsNotNull();
        await Assert
            .That(first.TextContents(".sa-configuration-problem-message").Single())
            .Contains("MismatchedEditor is not supported by a template for TextInputEditor.");
        await Assert.That(second.TextContents(".sa-configuration-problem-message")).HasCount(1);
        await Assert.That(logs.ConfigurationProblems).HasCount(1);
    }

    [Test]
    public async Task IconProblem_IsLoggedOnce()
    {
        // Arrange

        // Act
        await using var sut = await StellarAdminTestHost.CreateAsync(
            new([]),
            configureAdmin: admin =>
                admin.ConfigureUI(ui =>
                    ui.Services.Configure<IconOptions>(icons =>
                        icons.MapSemanticIcon(SemanticIconRole.Close, "missing")
                    )
                )
        );

        // Assert
        var logs = sut.Services.GetRequiredService<RecordingLoggerProvider>();
        var problem = logs.ConfigurationProblems.Single();
        await Assert.That(problem.EventId.Id).IsEqualTo(1090);
        await Assert.That(problem.Message).Contains("'missing', which isn't registered");
    }

    [Test]
    [Arguments("Development", true, true)]
    [Arguments("Development", false, false)]
    [Arguments("Production", true, false)]
    public async Task Indicator_RendersOnlyInDevelopmentWhenShown(
        string environment,
        bool showIndicator,
        bool rendered
    )
    {
        // Arrange
        await using var sut = await StellarAdminTestHost.CreateAsync(
            new([new(1, "Notebook", 12.50m)]),
            resource => resource.Index(index => index.DefaultSortBy(product => product.Name)),
            admin =>
                admin.ConfigureDiagnostics(diagnostics =>
                    diagnostics.ShowIndicator = showIndicator
                ),
            environmentName: environment
        );
        using var client = sut.GetTestClient();

        // Act
        var document = await client.GetDocumentAsync("/stellaradmin/products");

        // Assert
        await Assert
            .That(document.QuerySelector(".sa-configuration-problems-trigger") is not null)
            .IsEqualTo(rendered);
        await Assert
            .That(document.QuerySelector("#configuration-problems") is not null)
            .IsEqualTo(rendered);
    }

    [Test]
    public async Task Indicator_ListsProblemsByResource()
    {
        // Arrange
        await using var sut = await StellarAdminTestHost.CreateAsync(
            new([new(1, "Notebook", 12.50m)]),
            resource => resource.Index(index => index.DefaultSortBy(product => product.Name)),
            admin => admin.AddStylesheet(" ")
        );
        using var client = sut.GetTestClient();

        // Act
        var document = await client.GetDocumentAsync("/stellaradmin/products");

        // Assert
        await Assert
            .That(document.QuerySelector(".sa-configuration-problems-count")!.TextContent.Trim())
            .IsEqualTo("2");
        await Assert
            .That(document.TextContents(".sa-configuration-problems-group-title > span"))
            .IsEquivalentTo(["Admin panel", "Products"]);
        await Assert
            .That(
                document
                    .RequiredElement(".sa-configuration-problems-group-title > a")
                    .GetAttribute("href")
            )
            .IsEqualTo("/stellaradmin/products");
    }

    [Test]
    public async Task ValidConfiguration_RendersNoIndicator()
    {
        // Arrange
        await using var sut = await StellarAdminTestHost.CreateAsync(new([]));
        using var client = sut.GetTestClient();

        // Act
        var document = await client.GetDocumentAsync("/stellaradmin/products");

        // Assert
        await Assert.That(document.QuerySelector(".sa-configuration-problems")).IsNull();
    }

    [Test]
    public async Task Problem_IsLoggedOnceWithItsEventId()
    {
        // Arrange
        await using var sut = await StellarAdminTestHost.CreateAsync(
            new([new(1, "Notebook", 12.50m)]),
            resource => resource.Index(index => index.DefaultSortBy(product => product.Name))
        );
        using var client = sut.GetTestClient();

        // Act
        using var first = await client.GetAsync("/stellaradmin/products");
        using var second = await client.GetAsync("/stellaradmin/products");

        // Assert
        var logs = sut.Services.GetRequiredService<RecordingLoggerProvider>();
        var problem = logs.ConfigurationProblems.Single();
        await Assert.That(first.StatusCode).IsEqualTo(HttpStatusCode.OK);
        await Assert.That(second.StatusCode).IsEqualTo(HttpStatusCode.OK);
        await Assert.That(problem.EventId.Id).IsEqualTo(1015);
        await Assert.That(problem.EventId.Name).IsEqualTo("UnsortableDefaultSort");
        await Assert.That(problem.Message).Contains("The list uses the data source's order.");
    }

    [Test]
    public async Task ThrowOnProblems_FailsStartupWithEveryProblem()
    {
        // Arrange
        static async Task Create()
        {
            await using var app = await StellarAdminTestHost.CreateAsync(
                new([]),
                resource => resource.Index(index => index.DefaultSortBy(product => product.Name)),
                admin =>
                {
                    admin.AddStylesheet(" ");
                    admin.ConfigureDiagnostics(diagnostics => diagnostics.ThrowOnProblems = true);
                }
            );
        }

        // Act
        var exception = await Assert.That(Create).Throws<InvalidOperationException>();

        // Assert
        await Assert
            .That(exception!.Message)
            .Contains("The admin panel's configuration has 2 problems:");
        await Assert.That(exception.Message).Contains("AddStylesheet is called with a blank path.");
        await Assert.That(exception.Message).Contains("The default sort 'Name'");
    }

    [Test]
    public async Task ValidConfiguration_LogsNoProblems()
    {
        // Arrange

        // Act
        await using var sut = await StellarAdminTestHost.CreateAsync(new([]));

        // Assert
        var logs = sut.Services.GetRequiredService<RecordingLoggerProvider>();
        await Assert.That(logs.ConfigurationProblems).IsEmpty();
    }

    private sealed class MismatchedEditor : FieldEditor
    {
        public override string TemplateName => "Editors/TextInput";
    }
}
