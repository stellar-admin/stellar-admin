using System.Net;
using System.Runtime.CompilerServices;
using AngleSharp.Dom;
using AngleSharp.Html.Parser;
using Microsoft.AspNetCore.TestHost;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using StellarAdmin.EntityFrameworkCore.IntegrationTests.Fixtures;
using StellarAdmin.EntityFrameworkCore.IntegrationTests.Infrastructure;
using StellarAdmin.Resources.Options;

namespace StellarAdmin.EntityFrameworkCore.IntegrationTests.Resources;

public class ResourceCrudTests
{
    [Test]
    public async Task EnabledActions_RenderFormsAndIndexControls()
    {
        // Arrange
        await using var sut = await EfCoreTestHost.CreateAsync(ConfigureCrud);
        using var client = sut.GetTestClient();

        // Act
        using var indexResponse = await client.GetAsync("/stellaradmin/products");
        var index = await ReadDocument(indexResponse);
        using var createResponse = await client.GetAsync("/stellaradmin/products/create");
        var create = await ReadDocument(createResponse);
        using var editResponse = await client.GetAsync("/stellaradmin/products/edit/2");
        var edit = await ReadDocument(editResponse);

        // Assert
        await Assert
            .That(index.QuerySelector("a[href='/stellaradmin/products/create']"))
            .IsNotNull();
        await Assert
            .That(index.QuerySelector("a[href='/stellaradmin/products/edit/2']"))
            .IsNotNull();
        await Assert
            .That(index.QuerySelector("form[action='/stellaradmin/products/delete/2']"))
            .IsNotNull();
        await Assert.That(create.QuerySelector("input[name='Entity.Name']")).IsNotNull();
        await Assert.That(create.QuerySelector("input[name='Entity.Number']")).IsNull();
        await Assert
            .That(edit.QuerySelector("input[name='Entity.Name']")!.GetAttribute("value"))
            .IsEqualTo("Apple");
        await Assert.That(edit.QuerySelector("input[name='Entity.Number']")).IsNull();
    }

    [Test]
    public async Task Create_PersistsConfiguredFieldsAndIgnoresPostedKeyAndHiddenField()
    {
        // Arrange
        await using var sut = await EfCoreTestHost.CreateAsync(ConfigureCrud);
        using var client = sut.GetTestClient();
        var values = await PrepareForm(client, "/stellaradmin/products/create");
        values["Entity.Name"] = "New product";
        values["Entity.Price"] = "25";
        values["Entity.Number"] = "99";
        values["Entity.Hidden"] = "true";

        // Act
        using var response = await client.PostAsync(
            "/stellaradmin/products/create",
            new FormUrlEncodedContent(values)
        );

        // Assert
        await Assert.That(response.StatusCode).IsEqualTo(HttpStatusCode.Redirect);
        await Assert
            .That(response.Headers.Location?.OriginalString)
            .IsEqualTo("/stellaradmin/products");
        using var scope = sut.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<CatalogDbContext>();
        var created = await db.Set<Product>().SingleAsync(product => product.Name == "New product");
        await Assert.That(created.Number).IsNotEqualTo(99);
        await Assert.That(created.Price).IsEqualTo(25);
        await Assert.That(created.Hidden).IsFalse();
    }

    [Test]
    public async Task CreateFactory_SeedsUnconfiguredProperty()
    {
        // Arrange
        await using var sut = await EfCoreTestHost.CreateAsync(resource =>
            resource.AllowCreate(create =>
            {
                create.UseFactory(() => new Product { Price = 75 });
                create.Fields(fields => fields.Add(product => product.Name));
            })
        );
        using var client = sut.GetTestClient();
        var values = await PrepareForm(client, "/stellaradmin/products/create");
        values["Entity.Name"] = "Factory product";

        // Act
        using var response = await client.PostAsync(
            "/stellaradmin/products/create",
            new FormUrlEncodedContent(values)
        );

        // Assert
        await Assert.That(response.StatusCode).IsEqualTo(HttpStatusCode.Redirect);
        using var scope = sut.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<CatalogDbContext>();
        var created = await db.Set<Product>()
            .SingleAsync(product => product.Name == "Factory product");
        await Assert.That(created.Price).IsEqualTo(75);
    }

    [Test]
    public async Task InvalidCreate_RedisplaysValuesWithoutPersisting()
    {
        // Arrange
        await using var sut = await EfCoreTestHost.CreateAsync(ConfigureCrud);
        using var client = sut.GetTestClient();
        var values = await PrepareForm(client, "/stellaradmin/products/create");
        values["Entity.Name"] = "";
        values["Entity.Price"] = "-1";

        // Act
        using var response = await client.PostAsync(
            "/stellaradmin/products/create",
            new FormUrlEncodedContent(values)
        );
        var document = await ReadDocument(response);

        // Assert
        await Assert.That(response.StatusCode).IsEqualTo(HttpStatusCode.OK);
        await Assert
            .That(document.QuerySelector("[data-valmsg-for='Entity.Name']")!.TextContent)
            .IsNotNullOrEmpty();
        using var scope = sut.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<CatalogDbContext>();
        await Assert.That(await db.Set<Product>().CountAsync()).IsEqualTo(4);
    }

    [Test]
    public async Task InvalidEdit_RedisplaysValuesWithoutPersisting()
    {
        // Arrange
        await using var sut = await EfCoreTestHost.CreateAsync(ConfigureCrud);
        using var client = sut.GetTestClient();
        var values = await PrepareForm(client, "/stellaradmin/products/edit/2");
        values["Entity.Name"] = "";
        values["Entity.Price"] = "-1";

        // Act
        using var response = await client.PostAsync(
            "/stellaradmin/products/edit/2",
            new FormUrlEncodedContent(values)
        );
        var document = await ReadDocument(response);

        // Assert
        await Assert.That(response.StatusCode).IsEqualTo(HttpStatusCode.OK);
        await Assert
            .That(document.QuerySelector("[data-valmsg-for='Entity.Name']")!.TextContent)
            .IsNotNullOrEmpty();
        using var scope = sut.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<CatalogDbContext>();
        var unchanged = await db.Set<Product>().SingleAsync(product => product.Number == 2);
        await Assert.That(unchanged.Name).IsEqualTo("Apple");
        await Assert.That(unchanged.Price).IsEqualTo(20);
    }

    [Test]
    public async Task Edit_UpdatesOnlyConfiguredFields()
    {
        // Arrange
        await using var sut = await EfCoreTestHost.CreateAsync(ConfigureCrud);
        using var client = sut.GetTestClient();
        var values = await PrepareForm(client, "/stellaradmin/products/edit/2");
        values["Entity.Name"] = "Renamed";
        values["Entity.Price"] = "26";
        values["Entity.Number"] = "99";
        values["Entity.Hidden"] = "true";

        // Act
        using var response = await client.PostAsync(
            "/stellaradmin/products/edit/2",
            new FormUrlEncodedContent(values)
        );

        // Assert
        await Assert.That(response.StatusCode).IsEqualTo(HttpStatusCode.Redirect);
        using var scope = sut.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<CatalogDbContext>();
        var updated = await db.Set<Product>().SingleAsync(product => product.Number == 2);
        await Assert.That(updated.Name).IsEqualTo("Renamed");
        await Assert.That(updated.Price).IsEqualTo(26);
        await Assert.That(updated.Hidden).IsFalse();
        await Assert.That(await db.Set<Product>().CountAsync()).IsEqualTo(4);
    }

    [Test]
    [Arguments("/stellaradmin/products/edit/5")]
    [Arguments("/stellaradmin/products/edit/not-a-number")]
    public async Task HiddenOrInvalidKey_EditReturnsNotFound(string url)
    {
        // Arrange
        await using var sut = await EfCoreTestHost.CreateAsync(ConfigureCrud);
        using var client = sut.GetTestClient();

        // Act
        using var response = await client.GetAsync(url);

        // Assert
        await Assert.That(response.StatusCode).IsEqualTo(HttpStatusCode.NotFound);
    }

    [Test]
    public async Task Delete_RemovesVisibleEntityButCannotDeleteFilteredEntity()
    {
        // Arrange
        await using var sut = await EfCoreTestHost.CreateAsync(ConfigureCrud);
        using var client = sut.GetTestClient();
        var values = await PrepareForm(client, "/stellaradmin/products/create");

        // Act
        using var deleted = await client.PostAsync(
            "/stellaradmin/products/delete/4",
            new FormUrlEncodedContent(values)
        );
        using var filtered = await client.PostAsync(
            "/stellaradmin/products/delete/5",
            new FormUrlEncodedContent(values)
        );

        // Assert
        await Assert.That(deleted.StatusCode).IsEqualTo(HttpStatusCode.Redirect);
        await Assert.That(filtered.StatusCode).IsEqualTo(HttpStatusCode.NotFound);
        using var scope = sut.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<CatalogDbContext>();
        await Assert
            .That(
                await db.Set<Product>()
                    .IgnoreQueryFilters()
                    .AnyAsync(product => product.Number == 4)
            )
            .IsFalse();
        await Assert
            .That(
                await db.Set<Product>()
                    .IgnoreQueryFilters()
                    .AnyAsync(product => product.Number == 5)
            )
            .IsTrue();
    }

    [Test]
    public async Task PrimaryKeyAsCreateField_LogsProblemWithTheFieldsLine()
    {
        // Arrange
        var line = 0;

        // Act
        await using var sut = await EfCoreTestHost.CreateAsync(resource =>
            resource.AllowCreate(create =>
                create.Fields(fields =>
                {
                    line = CurrentLine() + 1;
                    fields.Add(product => product.Number);
                })
            )
        );

        // Assert
        var logs = sut.Services.GetRequiredService<RecordingLoggerProvider>();
        var problem = logs.ConfigurationProblems.Single(log =>
            log.Message.Contains("Product.Number is the primary key")
        );
        await Assert.That(problem.Message).EndsWith($"Configured at {CurrentFile()}:{line}.");
        var options = sut.Services.GetRequiredService<IOptions<ResourceOptions<Product>>>().Value;
        await Assert.That(options.Create!.Fields).IsEmpty();
    }

    [Test]
    public async Task ConcurrencyTokenAsEditField_LogsProblem()
    {
        // Arrange

        // Act
        await using var sut = await EfCoreTestHost.CreateAsync(resource =>
            resource.AllowEdit(edit =>
                edit.Fields(fields => fields.Add(product => product.Version))
            )
        );

        // Assert
        var logs = sut.Services.GetRequiredService<RecordingLoggerProvider>();
        await Assert
            .That(
                logs.ConfigurationProblems.Any(log =>
                    log.Message.Contains("Product.Version is a concurrency token")
                )
            )
            .IsTrue();
        var options = sut.Services.GetRequiredService<IOptions<ResourceOptions<Product>>>().Value;
        await Assert.That(options.Edit!.Fields).IsEmpty();
    }

    [Test]
    public async Task GeneratedValueAsEditField_LogsProblem()
    {
        // Arrange

        // Act
        await using var sut = await EfCoreTestHost.CreateAsync(resource =>
            resource.AllowEdit(edit =>
                edit.Fields(fields => fields.Add(product => product.GeneratedCode))
            )
        );

        // Assert
        var logs = sut.Services.GetRequiredService<RecordingLoggerProvider>();
        await Assert
            .That(
                logs.ConfigurationProblems.Any(log =>
                    log.Message.Contains("the database generates Product.GeneratedCode")
                )
            )
            .IsTrue();
        var options = sut.Services.GetRequiredService<IOptions<ResourceOptions<Product>>>().Value;
        await Assert.That(options.Edit!.Fields).IsEmpty();
    }

    [Test]
    public async Task Delete_ReferencedEntity_ShowsErrorAndKeepsEntity()
    {
        // Arrange
        await using var sut = await EfCoreTestHost.CreateAsync(configureAdmin: admin =>
            admin.AddEfCoreResource<Category>(resource =>
            {
                resource.Index(index =>
                    index.Columns(columns => columns.Add(category => category.Name))
                );
                resource.AllowDelete();
            })
        );
        using var client = sut.GetTestClient();
        var values = await PrepareForm(client, "/stellaradmin/categories");

        // Act
        using var response = await client.PostAsync(
            "/stellaradmin/categories/delete/1",
            new FormUrlEncodedContent(values)
        );

        // Assert
        await Assert.That(response.StatusCode).IsEqualTo(HttpStatusCode.OK);
        var document = await ReadDocument(response);
        await Assert
            .That(document.QuerySelector(".validation-summary-errors")?.TextContent)
            .Contains("This record couldn't be deleted. Other records may still refer to it.");
        using var scope = sut.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<CatalogDbContext>();
        await Assert.That(await db.Set<Category>().AnyAsync(category => category.Id == 1)).IsTrue();
    }

    [Test]
    public async Task MissingAntiforgeryToken_RejectsWrites()
    {
        // Arrange
        await using var sut = await EfCoreTestHost.CreateAsync(ConfigureCrud);
        using var client = sut.GetTestClient();

        // Act
        using var response = await client.PostAsync(
            "/stellaradmin/products/delete/4",
            new FormUrlEncodedContent([])
        );

        // Assert
        await Assert.That(response.StatusCode).IsEqualTo(HttpStatusCode.BadRequest);
        using var scope = sut.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<CatalogDbContext>();
        await Assert
            .That(await db.Set<Product>().AnyAsync(product => product.Number == 4))
            .IsTrue();
    }

    private static void ConfigureCrud(EfCoreResourceBuilder<Product> resource)
    {
        resource.AllowCreate(create =>
            create.Fields(fields =>
            {
                fields.Add(product => product.Name);
                fields.Add(product => product.Price);
            })
        );
        resource.AllowEdit(edit =>
            edit.Fields(fields =>
            {
                fields.Add(product => product.Name);
                fields.Add(product => product.Price);
            })
        );
        resource.AllowDelete();
    }

    private static string CurrentFile([CallerFilePath] string path = "") => path;

    private static int CurrentLine([CallerLineNumber] int line = 0) => line;

    private static async Task<Dictionary<string, string>> PrepareForm(HttpClient client, string url)
    {
        using var response = await client.GetAsync(url);
        response.EnsureSuccessStatusCode();
        var document = await ReadDocument(response);
        var token = document
            .QuerySelector("input[name='__RequestVerificationToken']")!
            .GetAttribute("value")!;
        client.DefaultRequestHeaders.Add(
            "Cookie",
            response.Headers.GetValues("Set-Cookie").Select(cookie => cookie.Split(';')[0])
        );

        return new() { ["__RequestVerificationToken"] = token };
    }

    private static async Task<IDocument> ReadDocument(HttpResponseMessage response) =>
        await new HtmlParser().ParseDocumentAsync(await response.Content.ReadAsStringAsync());
}
