using System.ComponentModel.DataAnnotations;
using System.Net;
using AngleSharp.Dom;
using AngleSharp.Html.Parser;
using Microsoft.AspNetCore.TestHost;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using StellarAdmin.EntityFrameworkCore.IntegrationTests.Fixtures;
using StellarAdmin.EntityFrameworkCore.IntegrationTests.Infrastructure;
using StellarAdmin.Resources;
using StellarAdmin.Resources.Editors;

namespace StellarAdmin.EntityFrameworkCore.IntegrationTests.Resources;

public class ResourceWrappedModelTests
{
    [Test]
    public async Task Edit_ShowsStoredEntityAndInferredLookup()
    {
        // Arrange
        await using var sut = await EfCoreTestHost.CreateAsync(
            ConfigureEdit,
            configureAdmin: AddRelatedResources
        );
        using var client = sut.GetTestClient();

        // Act
        using var response = await client.GetAsync("/stellaradmin/products/edit/2");
        var document = await ReadDocument(response);

        // Assert
        await Assert
            .That(
                document.QuerySelector("input[name='Entity.Product.Name']")!.GetAttribute("value")
            )
            .IsEqualTo("Apple");
        await Assert
            .That(document.QuerySelector("[id$='-selected'] [data-lookup='title']")?.TextContent)
            .IsEqualTo("Technology");
        await Assert
            .That(
                document
                    .QuerySelectorAll("input[name='Entity.Product.Tags'][checked]")
                    .Select(checkbox => checkbox.GetAttribute("value"))
            )
            .IsEquivalentTo(["1", "2"]);
        await Assert.That(document.QuerySelector("input[name='Entity.Reason']")).IsNotNull();
        await Assert.That(document.Title).Contains("Apple");
    }

    [Test]
    public async Task Edit_SavesEntityFieldsAndPassesFormFieldToEvent()
    {
        // Arrange
        await using var sut = await EfCoreTestHost.CreateAsync(
            ConfigureEdit,
            configureAdmin: AddRelatedResources
        );
        using var client = sut.GetTestClient();
        var values = await PrepareForm(client, "/stellaradmin/products/edit/2");
        values.Add(new("Entity.Product.Name", "Green apple"));
        values.Add(new("Entity.Product.Price", "22"));
        values.Add(new("Entity.Product.Details.Sku", "G-2"));
        values.Add(new("Entity.Product.CategoryId", "2"));
        values.Add(new("Entity.Product.Tags", "3"));
        values.Add(new("Entity.Reason", "Rebranded"));

        // Act
        using var response = await client.PostAsync(
            "/stellaradmin/products/edit/2",
            new FormUrlEncodedContent(values)
        );

        // Assert
        await Assert.That(response.StatusCode).IsEqualTo(HttpStatusCode.Redirect);
        using var scope = sut.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<CatalogDbContext>();
        var updated = await db.Set<Product>()
            .Include(product => product.Tags)
            .SingleAsync(product => product.Number == 2);
        await Assert.That(updated.Name).IsEqualTo("Green apple");
        await Assert.That(updated.Price).IsEqualTo(22);
        await Assert.That(updated.Details.Sku).IsEqualTo("G-2");
        await Assert.That(updated.Details.InternalNote).IsEqualTo("Keep apple note");
        await Assert.That(updated.CategoryId).IsEqualTo(2);
        await Assert.That(updated.Tags.Select(tag => tag.Id)).IsEquivalentTo([3]);
        await Assert
            .That((await db.Set<Category>().SingleAsync(category => category.Id == 10)).Name)
            .IsEqualTo("Rebranded");
    }

    [Test]
    public async Task Edit_EmptyRequiredFormFieldIsRejected()
    {
        // Arrange
        await using var sut = await EfCoreTestHost.CreateAsync(
            ConfigureEdit,
            configureAdmin: AddRelatedResources
        );
        using var client = sut.GetTestClient();
        var values = await PrepareForm(client, "/stellaradmin/products/edit/2");
        values.Add(new("Entity.Product.Name", "Green apple"));
        values.Add(new("Entity.Product.Price", "22"));
        values.Add(new("Entity.Reason", ""));

        // Act
        using var response = await client.PostAsync(
            "/stellaradmin/products/edit/2",
            new FormUrlEncodedContent(values)
        );
        var document = await ReadDocument(response);

        // Assert
        await Assert.That(response.StatusCode).IsEqualTo(HttpStatusCode.OK);
        await Assert
            .That(document.QuerySelector("[data-valmsg-for='Entity.Reason']")!.TextContent)
            .IsNotEmpty();
        using var scope = sut.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<CatalogDbContext>();
        await Assert
            .That((await db.Set<Product>().SingleAsync(product => product.Number == 2)).Name)
            .IsEqualTo("Apple");
    }

    [Test]
    public async Task Edit_FailOnEntityField_ShowsErrorAndSubmittedValues()
    {
        // Arrange
        var sameEntity = false;
        await using var sut = await EfCoreTestHost.CreateAsync(resource =>
            resource.AllowEdit<EditProductModel>(
                model => model.Product,
                edit =>
                {
                    edit.Fields(fields =>
                    {
                        fields.Add(model => model.Product.Name);
                        fields.Add(model => model.Product.Price);
                        fields.Add(model => model.Reason);
                    });
                    edit.Events(events =>
                        events.OnUpdating = (context, _) =>
                        {
                            sameEntity = ReferenceEquals(context.Model.Product, context.Entity);
                            context.Fail(model => model.Product.Price, "Not this price.");

                            return Task.CompletedTask;
                        }
                    );
                }
            )
        );
        using var client = sut.GetTestClient();
        var values = await PrepareForm(client, "/stellaradmin/products/edit/2");
        values.Add(new("Entity.Product.Name", "Green apple"));
        values.Add(new("Entity.Product.Price", "22"));
        values.Add(new("Entity.Reason", "Because"));

        // Act
        using var response = await client.PostAsync(
            "/stellaradmin/products/edit/2",
            new FormUrlEncodedContent(values)
        );
        var document = await ReadDocument(response);

        // Assert
        await Assert.That(response.StatusCode).IsEqualTo(HttpStatusCode.OK);
        await Assert.That(sameEntity).IsTrue();
        await Assert
            .That(document.QuerySelector("[data-valmsg-for='Entity.Product.Price']")!.TextContent)
            .Contains("Not this price.");
        await Assert
            .That(
                document.QuerySelector("input[name='Entity.Product.Name']")!.GetAttribute("value")
            )
            .IsEqualTo("Green apple");
    }

    [Test]
    public async Task Edit_ValidatableModelErrorOnEntityField_ShowsUnderField()
    {
        // Arrange
        await using var sut = await EfCoreTestHost.CreateAsync(resource =>
            resource.AllowEdit<ValidatedProductModel>(
                model => model.Product,
                edit =>
                    edit.Fields(fields =>
                    {
                        fields.Add(model => model.Product.Name);
                        fields.Add(model => model.Product.Price);
                    })
            )
        );
        using var client = sut.GetTestClient();
        var values = await PrepareForm(client, "/stellaradmin/products/edit/2");
        values.Add(new("Entity.Product.Name", "Free"));
        values.Add(new("Entity.Product.Price", "22"));

        // Act
        using var response = await client.PostAsync(
            "/stellaradmin/products/edit/2",
            new FormUrlEncodedContent(values)
        );
        var document = await ReadDocument(response);

        // Assert
        await Assert.That(response.StatusCode).IsEqualTo(HttpStatusCode.OK);
        await Assert
            .That(document.QuerySelector("[data-valmsg-for='Entity.Product.Name']")!.TextContent)
            .Contains("Free isn't a name.");
    }

    [Test]
    public async Task Create_SavesEntityWithInitializedFormField()
    {
        // Arrange
        await using var sut = await EfCoreTestHost.CreateAsync(
            resource =>
                resource.AllowCreate<CreateProductModel>(
                    model => model.Product,
                    create =>
                    {
                        create.Fields(fields =>
                        {
                            fields.Add(model => model.Product.Name);
                            fields.Add(model => model.Product.Price);
                            fields.Add(model => model.Product.CategoryId);
                            fields
                                .Add(model => model.Product.Tags)
                                .UseCheckboxGroupEditor(_ => { });
                            fields.Add(model => model.Note);
                        });
                        create.Events(events =>
                        {
                            events.OnInitialized = (context, _) =>
                            {
                                context.Model.Note = "Imported";

                                return Task.CompletedTask;
                            };
                            events.OnCreating = (context, _) =>
                            {
                                context.Db.Add(new Category { Id = 10, Name = context.Model.Note });

                                return Task.CompletedTask;
                            };
                        });
                    }
                ),
            configureAdmin: AddRelatedResources
        );
        using var client = sut.GetTestClient();
        var page = await ReadDocument(await client.GetAsync("/stellaradmin/products/create"));
        var values = await PrepareForm(client, "/stellaradmin/products/create");
        values.Add(new("Entity.Product.Name", "Pear"));
        values.Add(new("Entity.Product.Price", "15"));
        values.Add(new("Entity.Product.CategoryId", "2"));
        values.Add(new("Entity.Product.Tags", "1"));
        values.Add(new("Entity.Note", "Seasonal"));

        // Act
        using var response = await client.PostAsync(
            "/stellaradmin/products/create",
            new FormUrlEncodedContent(values)
        );

        // Assert
        await Assert
            .That(page.QuerySelector("input[name='Entity.Note']")!.GetAttribute("value"))
            .IsEqualTo("Imported");
        await Assert.That(response.StatusCode).IsEqualTo(HttpStatusCode.Redirect);
        using var scope = sut.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<CatalogDbContext>();
        var created = await db.Set<Product>()
            .Include(product => product.Tags)
            .SingleAsync(product => product.Name == "Pear");
        await Assert.That(created.Price).IsEqualTo(15);
        await Assert.That(created.CategoryId).IsEqualTo(2);
        await Assert.That(created.Tags.Select(tag => tag.Id)).IsEquivalentTo([1]);
        await Assert
            .That((await db.Set<Category>().SingleAsync(category => category.Id == 10)).Name)
            .IsEqualTo("Seasonal");
    }

    [Test]
    public async Task InlineEditRow_MatchesColumnsWithoutEntityPrefix()
    {
        // Arrange
        await using var sut = await EfCoreTestHost.CreateAsync(resource =>
        {
            resource.AllowDelete();
            resource.AllowEdit<EditProductModel>(
                model => model.Product,
                edit =>
                {
                    edit.Fields(fields =>
                    {
                        fields.Add(model => model.Product.Name);
                        fields.Add(model => model.Product.Price);
                    });
                    edit.UseInlineRow(inline => inline.AfterSave = InlineRowAfterSave.KeepInPlace);
                }
            );
        });
        using var client = sut.GetTestClient();
        var values = await PrepareForm(client, "/stellaradmin/products");
        values.Add(new("rows[2].Product.Name", "Apple pie"));
        values.Add(new("rows[2].Product.Price", "25"));

        // Act
        using var response = await client.PostAsync(
            "/stellaradmin/products/editrow/2",
            new FormUrlEncodedContent(values)
        );

        // Assert
        var logs = sut.Services.GetRequiredService<RecordingLoggerProvider>();
        await Assert.That(logs.ConfigurationProblems).IsEmpty();
        await Assert.That(response.StatusCode).IsEqualTo(HttpStatusCode.OK);
        using var scope = sut.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<CatalogDbContext>();
        var updated = await db.Set<Product>().SingleAsync(product => product.Number == 2);
        await Assert.That(updated.Name).IsEqualTo("Apple pie");
        await Assert.That(updated.Price).IsEqualTo(25);
    }

    [Test]
    public async Task InlineCreateRow_RequiredFormFieldWithoutColumnIsAProblem()
    {
        // Arrange
        await using var sut = await EfCoreTestHost.CreateAsync(resource =>
            resource.AllowCreate<CreateProductModel>(
                model => model.Product,
                create =>
                {
                    create.Fields(fields =>
                    {
                        fields.Add(model => model.Product.Name);
                        fields.Add(model => model.Product.Price);
                    });
                    create.UseInlineRow();
                }
            )
        );

        // Act
        using var response = await sut.GetTestClient().GetAsync("/stellaradmin/products");

        // Assert
        var logs = sut.Services.GetRequiredService<RecordingLoggerProvider>();
        await Assert
            .That(logs.ConfigurationProblems.Single().Message)
            .Contains("required properties Reference of CreateProductModel");
    }

    [Test]
    public async Task EntitySelectorThatIsNotAProperty_Throws()
    {
        // Arrange
        Task Act() =>
            EfCoreTestHost.CreateAsync(resource =>
                resource.AllowEdit<EditProductModel>(model => new Product(), _ => { })
            );

        // Act
        var exception = await Assert.ThrowsAsync<ArgumentException>(Act);

        // Assert
        await Assert.That(exception!.ParamName).IsEqualTo("entity");
    }

    private static void AddRelatedResources(StellarAdminBuilder admin)
    {
        admin.AddEfCoreResource<Category>();
        admin.AddEfCoreResource<Tag>();
    }

    private static void ConfigureEdit(EfCoreResourceBuilder<Product> resource) =>
        resource.AllowEdit<EditProductModel>(
            model => model.Product,
            edit =>
            {
                edit.Fields(fields =>
                {
                    fields.Add(model => model.Product.Name);
                    fields.Add(model => model.Product.Price);
                    fields.Add(model => model.Product.Details.Sku);
                    fields.Add(model => model.Product.CategoryId);
                    fields.Add(model => model.Product.Tags).UseCheckboxGroupEditor(_ => { });
                    fields.Add(model => model.Reason);
                });
                edit.Events(events =>
                    events.OnUpdating = (context, _) =>
                    {
                        context.Db.Add(new Category { Id = 10, Name = context.Model.Reason });

                        return Task.CompletedTask;
                    }
                );
            }
        );

    private static async Task<List<KeyValuePair<string, string>>> PrepareForm(
        HttpClient client,
        string url
    )
    {
        using var response = await client.GetAsync(url);
        response.EnsureSuccessStatusCode();
        var document = await ReadDocument(response);
        var token = document
            .QuerySelector("input[name='__RequestVerificationToken']")!
            .GetAttribute("value")!;
        client.DefaultRequestHeaders.Remove("Cookie");
        client.DefaultRequestHeaders.Add(
            "Cookie",
            response.Headers.GetValues("Set-Cookie").Select(cookie => cookie.Split(';')[0])
        );

        return [new("__RequestVerificationToken", token)];
    }

    private static async Task<IDocument> ReadDocument(HttpResponseMessage response) =>
        await new HtmlParser().ParseDocumentAsync(await response.Content.ReadAsStringAsync());

    public sealed class CreateProductModel
    {
        public Product Product { get; set; } = new();

        public string? Note { get; set; }

        [Required]
        public string Reference { get; set; } = "";
    }

    public sealed class EditProductModel
    {
        public Product Product { get; set; } = null!;

        [Required, StringLength(500)]
        public string Reason { get; set; } = "";
    }

    public sealed class ValidatedProductModel : IValidatableObject
    {
        public Product Product { get; set; } = null!;

        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            if (Product.Name == "Free")
            {
                yield return new("Free isn't a name.", ["Product.Name"]);
            }
        }
    }
}
