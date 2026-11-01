using System.Collections.Concurrent;
using System.Net;
using AngleSharp.Dom;
using AngleSharp.Html.Parser;
using Microsoft.AspNetCore.TestHost;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using StellarAdmin.EntityFrameworkCore.IntegrationTests.Fixtures;
using StellarAdmin.EntityFrameworkCore.IntegrationTests.Infrastructure;

namespace StellarAdmin.EntityFrameworkCore.IntegrationTests.Resources;

public class ResourceFormEventsTests
{
    [Test]
    public async Task OnInitialized_ShowsDefaultOnCreatePage()
    {
        // Arrange
        await using var sut = await EfCoreTestHost.CreateAsync(resource =>
            resource.AllowCreate(create =>
            {
                create.Fields(fields =>
                {
                    fields.Add(product => product.Name);
                    fields.Add(product => product.Price);
                });
                create.Events(events =>
                    events.OnInitialized = async (context, cancellationToken) =>
                        context.Entity.Price = await context
                            .Db.Set<Product>()
                            .MaxAsync(product => product.Price, cancellationToken)
                );
            })
        );
        using var client = sut.GetTestClient();

        // Act
        using var response = await client.GetAsync("/stellaradmin/products/create");
        var document = await ReadDocument(response);

        // Assert
        await Assert
            .That(document.QuerySelector("input[name='Entity.Price']")!.GetAttribute("value"))
            .IsEqualTo("40");
    }

    [Test]
    public async Task OnCreating_SavesUnshownValueAndAddedRecordsWithEntity()
    {
        // Arrange
        await using var sut = await EfCoreTestHost.CreateAsync(resource =>
            resource.AllowCreate(create =>
            {
                create.Fields(fields => fields.Add(product => product.Name));
                create.Events(events =>
                    events.OnCreating = (context, _) =>
                    {
                        context.Entity.Price = 99;
                        context.Db.Add(new Category { Id = 10, Name = context.Entity.Name });

                        return Task.CompletedTask;
                    }
                );
            })
        );
        using var client = sut.GetTestClient();
        var values = await PrepareForm(client, "/stellaradmin/products/create");
        values["Entity.Name"] = "Created";

        // Act
        using var response = await client.PostAsync(
            "/stellaradmin/products/create",
            new FormUrlEncodedContent(values)
        );

        // Assert
        await Assert.That(response.StatusCode).IsEqualTo(HttpStatusCode.Redirect);
        using var scope = sut.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<CatalogDbContext>();
        var created = await db.Set<Product>().SingleAsync(product => product.Name == "Created");
        await Assert.That(created.Price).IsEqualTo(99);
        await Assert
            .That((await db.Set<Category>().SingleAsync(category => category.Id == 10)).Name)
            .IsEqualTo("Created");
    }

    [Test]
    public async Task OnCreating_FailOnField_ShowsErrorAndSavesNothing()
    {
        // Arrange
        await using var sut = await EfCoreTestHost.CreateAsync(resource =>
            resource.AllowCreate(create =>
            {
                create.Fields(fields =>
                {
                    fields.Add(product => product.Name);
                    fields.Add(product => product.Price);
                });
                create.Events(events =>
                    events.OnCreating = (context, _) =>
                    {
                        context.Db.Add(new Category { Id = 10, Name = "Audit" });
                        context.Fail(product => product.Price, "Too expensive.");

                        return Task.CompletedTask;
                    }
                );
            })
        );
        using var client = sut.GetTestClient();
        var values = await PrepareForm(client, "/stellaradmin/products/create");
        values["Entity.Name"] = "Rejected";
        values["Entity.Price"] = "500";

        // Act
        using var response = await client.PostAsync(
            "/stellaradmin/products/create",
            new FormUrlEncodedContent(values)
        );
        var document = await ReadDocument(response);

        // Assert
        await Assert.That(response.StatusCode).IsEqualTo(HttpStatusCode.OK);
        await Assert
            .That(document.QuerySelector("[data-valmsg-for='Entity.Price']")!.TextContent)
            .Contains("Too expensive.");
        using var scope = sut.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<CatalogDbContext>();
        await Assert
            .That(await db.Set<Product>().AnyAsync(product => product.Name == "Rejected"))
            .IsFalse();
        await Assert
            .That(await db.Set<Category>().AnyAsync(category => category.Id == 10))
            .IsFalse();
    }

    [Test]
    public async Task OnCreating_FailAfterSavingItself_RollsBackTheSave()
    {
        // Arrange
        await using var sut = await EfCoreTestHost.CreateAsync(resource =>
            resource.AllowCreate(create =>
            {
                create.Fields(fields => fields.Add(product => product.Name));
                create.Events(events =>
                    events.OnCreating = async (context, cancellationToken) =>
                    {
                        context.Entity.Price = 5;
                        await context.Db.SaveChangesAsync(cancellationToken);
                        context.Fail("The record couldn't be finished.");
                    }
                );
            })
        );
        using var client = sut.GetTestClient();
        var values = await PrepareForm(client, "/stellaradmin/products/create");
        values["Entity.Name"] = "Rolled back";

        // Act
        using var response = await client.PostAsync(
            "/stellaradmin/products/create",
            new FormUrlEncodedContent(values)
        );
        var document = await ReadDocument(response);

        // Assert
        await Assert.That(response.StatusCode).IsEqualTo(HttpStatusCode.OK);
        await Assert
            .That(document.QuerySelector(".validation-summary-errors")?.TextContent)
            .Contains("The record couldn't be finished.");
        using var scope = sut.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<CatalogDbContext>();
        await Assert
            .That(await db.Set<Product>().AnyAsync(product => product.Name == "Rolled back"))
            .IsFalse();
    }

    [Test]
    public async Task OnCreating_Handled_SavesOnlyWhatTheEventSaved()
    {
        // Arrange
        await using var sut = await EfCoreTestHost.CreateAsync(resource =>
            resource.AllowCreate(create =>
            {
                create.Fields(fields => fields.Add(product => product.Name));
                create.Events(events =>
                    events.OnCreating = async (context, cancellationToken) =>
                    {
                        context.Entity.Price = 5;
                        await context.Db.SaveChangesAsync(cancellationToken);
                        context.Db.Add(new Category { Id = 10, Name = "Not saved" });
                        context.Handled();
                    }
                );
            })
        );
        using var client = sut.GetTestClient();
        var values = await PrepareForm(client, "/stellaradmin/products/create");
        values["Entity.Name"] = "Handled";

        // Act
        using var response = await client.PostAsync(
            "/stellaradmin/products/create",
            new FormUrlEncodedContent(values)
        );

        // Assert
        await Assert.That(response.StatusCode).IsEqualTo(HttpStatusCode.Redirect);
        using var scope = sut.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<CatalogDbContext>();
        await Assert
            .That(await db.Set<Product>().CountAsync(product => product.Name == "Handled"))
            .IsEqualTo(1);
        await Assert
            .That(await db.Set<Category>().AnyAsync(category => category.Id == 10))
            .IsFalse();
    }

    [Test]
    public async Task OnCreated_ExceptionIsLoggedAndCreateSucceeds()
    {
        // Arrange
        await using var sut = await EfCoreTestHost.CreateAsync(resource =>
            resource.AllowCreate(create =>
            {
                create.Fields(fields =>
                {
                    fields.Add(product => product.Name);
                    fields.Add(product => product.Price);
                });
                create.Events(events =>
                    events.OnCreated = (_, _) => throw new InvalidOperationException("Email down.")
                );
            })
        );
        using var client = sut.GetTestClient();
        var values = await PrepareForm(client, "/stellaradmin/products/create");
        values["Entity.Name"] = "Saved anyway";
        values["Entity.Price"] = "12";

        // Act
        using var response = await client.PostAsync(
            "/stellaradmin/products/create",
            new FormUrlEncodedContent(values)
        );

        // Assert
        await Assert.That(response.StatusCode).IsEqualTo(HttpStatusCode.Redirect);
        using var scope = sut.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<CatalogDbContext>();
        await Assert
            .That(await db.Set<Product>().AnyAsync(product => product.Name == "Saved anyway"))
            .IsTrue();
        var logs = sut.Services.GetRequiredService<RecordingLoggerProvider>().Logs;
        await Assert
            .That(
                logs.Any(log =>
                    log.Level == LogLevel.Error
                    && log.Message.Contains("failed after the record was saved")
                )
            )
            .IsTrue();
    }

    [Test]
    public async Task OnLoaded_ChangesModelTheEditPageShows()
    {
        // Arrange
        await using var sut = await EfCoreTestHost.CreateAsync(resource =>
            resource.AllowEdit(edit =>
            {
                edit.Fields(fields => fields.Add(product => product.Name));
                edit.Events(events =>
                    events.OnLoaded = (context, _) =>
                    {
                        context.Model.Name = context.Entity.Name.ToUpperInvariant();

                        return Task.CompletedTask;
                    }
                );
            })
        );
        using var client = sut.GetTestClient();

        // Act
        using var response = await client.GetAsync("/stellaradmin/products/edit/2");
        var document = await ReadDocument(response);

        // Assert
        await Assert
            .That(document.QuerySelector("input[name='Entity.Name']")!.GetAttribute("value"))
            .IsEqualTo("APPLE");
    }

    [Test]
    public async Task OnUpdating_ComparesStoredValueAndFailsOnField()
    {
        // Arrange
        await using var sut = await EfCoreTestHost.CreateAsync(resource =>
            resource.AllowEdit(edit =>
            {
                edit.Fields(fields =>
                {
                    fields.Add(product => product.Name);
                    fields.Add(product => product.Price);
                });
                edit.Events(events =>
                    events.OnUpdating = (context, _) =>
                    {
                        var original = context
                            .Entry.Property(product => product.Price)
                            .OriginalValue;
                        if (context.Entity.Price < original / 2)
                        {
                            context.Fail(product => product.Price, "Too big a drop.");
                        }

                        return Task.CompletedTask;
                    }
                );
            })
        );
        using var client = sut.GetTestClient();
        var values = await PrepareForm(client, "/stellaradmin/products/edit/2");
        values["Entity.Name"] = "Cheap apple";
        values["Entity.Price"] = "5";

        // Act
        using var response = await client.PostAsync(
            "/stellaradmin/products/edit/2",
            new FormUrlEncodedContent(values)
        );
        var document = await ReadDocument(response);

        // Assert
        await Assert.That(response.StatusCode).IsEqualTo(HttpStatusCode.OK);
        await Assert
            .That(document.QuerySelector("[data-valmsg-for='Entity.Price']")!.TextContent)
            .Contains("Too big a drop.");
        await Assert
            .That(document.QuerySelector("input[name='Entity.Name']")!.GetAttribute("value"))
            .IsEqualTo("Cheap apple");
        using var scope = sut.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<CatalogDbContext>();
        var unchanged = await db.Set<Product>().SingleAsync(product => product.Number == 2);
        await Assert.That(unchanged.Name).IsEqualTo("Apple");
        await Assert.That(unchanged.Price).IsEqualTo(20);
    }

    [Test]
    public async Task OnUpdated_RunsWithSavedEntity()
    {
        // Arrange
        var saved = new ConcurrentQueue<string>();
        await using var sut = await EfCoreTestHost.CreateAsync(resource =>
            resource.AllowEdit(edit =>
            {
                edit.Fields(fields => fields.Add(product => product.Name));
                edit.Events(events =>
                    events.OnUpdated = (context, _) =>
                    {
                        saved.Enqueue(context.Entity.Name);

                        return Task.CompletedTask;
                    }
                );
            })
        );
        using var client = sut.GetTestClient();
        var values = await PrepareForm(client, "/stellaradmin/products/edit/2");
        values["Entity.Name"] = "Renamed";

        // Act
        using var response = await client.PostAsync(
            "/stellaradmin/products/edit/2",
            new FormUrlEncodedContent(values)
        );

        // Assert
        await Assert.That(response.StatusCode).IsEqualTo(HttpStatusCode.Redirect);
        await Assert.That(saved).IsEquivalentTo(["Renamed"]);
    }

    [Test]
    public async Task UseEvents_RunsEventsClassFromServices()
    {
        // Arrange
        await using var sut = await EfCoreTestHost.CreateAsync(resource =>
            resource.AllowCreate(create =>
            {
                create.Fields(fields => fields.Add(product => product.Name));
                create.UseEvents<PricingEvents>();
            })
        );
        using var client = sut.GetTestClient();
        var values = await PrepareForm(client, "/stellaradmin/products/create");
        values["Entity.Name"] = "Priced";

        // Act
        using var response = await client.PostAsync(
            "/stellaradmin/products/create",
            new FormUrlEncodedContent(values)
        );

        // Assert
        await Assert.That(response.StatusCode).IsEqualTo(HttpStatusCode.Redirect);
        using var scope = sut.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<CatalogDbContext>();
        var created = await db.Set<Product>().SingleAsync(product => product.Name == "Priced");
        await Assert.That(created.Price).IsEqualTo(41);
    }

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

    private sealed class PricingEvents(CatalogDbContext db) : EfCoreCreateEvents<Product, Product>
    {
        public override async Task CreatingAsync(
            EfCoreSaveContext<Product, Product> context,
            CancellationToken cancellationToken
        ) =>
            context.Entity.Price =
                await db.Set<Product>().MaxAsync(product => product.Price, cancellationToken) + 1;
    }
}
