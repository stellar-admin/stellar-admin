using System.Net;
using AngleSharp.Dom;
using AngleSharp.Html.Parser;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.TestHost;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using StellarAdmin.EntityFrameworkCore.IntegrationTests.Fixtures;
using StellarAdmin.EntityFrameworkCore.IntegrationTests.Infrastructure;
using StellarAdmin.Resources.Builders;
using StellarAdmin.Resources.Displays;
using StellarAdmin.Resources.Editors;
using StellarAdmin.Resources.Options;

namespace StellarAdmin.EntityFrameworkCore.IntegrationTests.Resources;

public class ResourceManyToManyTests
{
    [Test]
    [Arguments("checkboxGroup")]
    [Arguments("toggleGroup")]
    public async Task ChoiceEditor_Edit_ChecksTheRelatedEntities(string editor)
    {
        // Arrange
        await using var sut = await EfCoreTestHost.CreateAsync(resource =>
            resource.AllowEdit(edit =>
                edit.Fields(editor == "checkboxGroup" ? UseCheckboxGroup : UseToggleGroup)
            )
        );
        using var client = sut.GetTestClient();

        // Act
        using var response = await client.GetAsync("/stellaradmin/products/edit/2");
        var document = await ReadDocument(response);

        // Assert
        var checkboxes = document.QuerySelectorAll("input[type='checkbox'][name='Entity.Tags']");
        await Assert
            .That(checkboxes.Select(checkbox => checkbox.GetAttribute("value")))
            .IsEquivalentTo(["1", "2", "3"]);
        await Assert
            .That(
                checkboxes
                    .Where(checkbox => checkbox.HasAttribute("checked"))
                    .Select(checkbox => checkbox.GetAttribute("value"))
            )
            .IsEquivalentTo(["1", "2"]);
        await Assert.That(checkboxes.Any(checkbox => checkbox.HasAttribute("disabled"))).IsFalse();
    }

    [Test]
    public async Task CheckboxGroup_EditPost_ReplacesTheRelatedEntities()
    {
        // Arrange
        await using var sut = await EfCoreTestHost.CreateAsync(resource =>
            resource.AllowEdit(edit => edit.Fields(UseCheckboxGroup))
        );
        using var client = sut.GetTestClient();
        var values = await PrepareForm(client, "/stellaradmin/products/edit/2");
        values.Add(new("Entity.Tags", "2"));
        values.Add(new("Entity.Tags", "3"));

        // Act
        using var response = await client.PostAsync(
            "/stellaradmin/products/edit/2",
            new FormUrlEncodedContent(values)
        );

        // Assert
        await Assert.That(response.StatusCode).IsEqualTo(HttpStatusCode.Redirect);
        await Assert.That(await ReadTagIds(sut, 2)).IsEquivalentTo([2, 3]);
        await Assert.That(await CountTags(sut)).IsEqualTo(3);
    }

    [Test]
    public async Task CheckboxGroup_EditPostWithoutValues_RemovesTheRelatedEntities()
    {
        // Arrange
        await using var sut = await EfCoreTestHost.CreateAsync(resource =>
            resource.AllowEdit(edit => edit.Fields(UseCheckboxGroup))
        );
        using var client = sut.GetTestClient();
        var values = await PrepareForm(client, "/stellaradmin/products/edit/2");

        // Act
        using var response = await client.PostAsync(
            "/stellaradmin/products/edit/2",
            new FormUrlEncodedContent(values)
        );

        // Assert
        await Assert.That(response.StatusCode).IsEqualTo(HttpStatusCode.Redirect);
        await Assert.That(await ReadTagIds(sut, 2)).IsEmpty();
        await Assert.That(await CountTags(sut)).IsEqualTo(3);
    }

    [Test]
    public async Task EditPost_WithUnknownKey_ShowsTheErrorWithoutSaving()
    {
        // Arrange
        await using var sut = await EfCoreTestHost.CreateAsync(resource =>
            resource.AllowEdit(edit => edit.Fields(UseCheckboxGroup))
        );
        using var client = sut.GetTestClient();
        var values = await PrepareForm(client, "/stellaradmin/products/edit/2");
        values.Add(new("Entity.Tags", "3"));
        values.Add(new("Entity.Tags", "99"));

        // Act
        using var response = await client.PostAsync(
            "/stellaradmin/products/edit/2",
            new FormUrlEncodedContent(values)
        );
        var document = await ReadDocument(response);

        // Assert
        await Assert.That(response.StatusCode).IsEqualTo(HttpStatusCode.OK);
        await Assert
            .That(document.QuerySelector("[data-valmsg-for='Entity.Tags']")!.TextContent)
            .Contains("'99' is not a valid choice.");
        await Assert
            .That(
                document
                    .QuerySelectorAll("input[type='checkbox'][name='Entity.Tags'][checked]")
                    .Select(checkbox => checkbox.GetAttribute("value"))
            )
            .IsEquivalentTo(["3"]);
        await Assert.That(await ReadTagIds(sut, 2)).IsEquivalentTo([1, 2]);
    }

    [Test]
    public async Task EditPost_OverMaxLength_ShowsTheErrorWithoutSaving()
    {
        // Arrange
        await using var sut = await EfCoreTestHost.CreateAsync(resource =>
            resource.AllowEdit(edit => edit.Fields(UseCheckboxGroup))
        );
        using var client = sut.GetTestClient();
        var values = await PrepareForm(client, "/stellaradmin/products/edit/2");
        values.Add(new("Entity.Tags", "1"));
        values.Add(new("Entity.Tags", "2"));
        values.Add(new("Entity.Tags", "3"));

        // Act
        using var response = await client.PostAsync(
            "/stellaradmin/products/edit/2",
            new FormUrlEncodedContent(values)
        );
        var document = await ReadDocument(response);

        // Assert
        await Assert.That(response.StatusCode).IsEqualTo(HttpStatusCode.OK);
        await Assert
            .That(document.QuerySelector("[data-valmsg-for='Entity.Tags']")!.TextContent)
            .Contains("Tags");
        await Assert.That(await ReadTagIds(sut, 2)).IsEquivalentTo([1, 2]);
    }

    [Test]
    public async Task CreatePost_LinksTheExistingRelatedEntities()
    {
        // Arrange
        await using var sut = await EfCoreTestHost.CreateAsync(resource =>
            resource.AllowCreate(create =>
                create.Fields(fields =>
                {
                    fields.Add(product => product.Name);
                    fields.Add(product => product.Price);
                    UseCheckboxGroup(fields);
                })
            )
        );
        using var client = sut.GetTestClient();
        var values = await PrepareForm(client, "/stellaradmin/products/create");
        values.Add(new("Entity.Name", "Tagged"));
        values.Add(new("Entity.Price", "15"));
        values.Add(new("Entity.Tags", "1"));
        values.Add(new("Entity.Tags", "3"));

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
            .Include(product => product.Tags)
            .SingleAsync(product => product.Name == "Tagged");
        await Assert.That(created.Tags.Select(tag => tag.Id)).IsEquivalentTo([1, 3]);
        await Assert.That(await CountTags(sut)).IsEqualTo(3);
    }

    [Test]
    public async Task MultiLookupSheet_Edit_PostsTheRelatedKeysAndShowsTheirTitles()
    {
        // Arrange
        await using var sut = await EfCoreTestHost.CreateAsync(resource =>
            resource.AllowEdit(edit => edit.Fields(UseMultiLookupSheet))
        );
        using var client = sut.GetTestClient();

        // Act
        using var response = await client.GetAsync("/stellaradmin/products/edit/2");
        var document = await ReadDocument(response);

        // Assert
        var editor = document.QuerySelector("admin-multi-lookup-sheet-editor")!;
        await Assert
            .That(
                editor
                    .QuerySelectorAll("[data-lookup='values'] input[type='hidden']")
                    .Select(input => input.GetAttribute("value"))
            )
            .IsEquivalentTo(["1", "2"]);
        await Assert.That(editor.TextContent).Contains("Sale");
        await Assert.That(editor.TextContent).Contains("Seasonal");
    }

    [Test]
    public async Task MultiLookupSelection_ConvertsTheKeysToTheRelatedKeyType()
    {
        // Arrange
        await using var sut = await EfCoreTestHost.CreateAsync(resource =>
            resource.AllowEdit(edit => edit.Fields(UseMultiLookupSheet))
        );
        using var client = sut.GetTestClient();

        // Act
        using var response = await client.GetAsync(
            "/stellaradmin/products/multilookupselection?form=edit&field=Tags&selected=3&selected=1"
        );

        // Assert
        await Assert.That(response.StatusCode).IsEqualTo(HttpStatusCode.OK);
        var content = await response.Content.ReadAsStringAsync();
        await Assert.That(content).Contains("Organic");
        await Assert.That(content).Contains("Sale");
    }

    [Test]
    public async Task MultiLookupSheet_EditPost_ReplacesTheRelatedEntities()
    {
        // Arrange
        await using var sut = await EfCoreTestHost.CreateAsync(resource =>
            resource.AllowEdit(edit => edit.Fields(UseMultiLookupSheet))
        );
        using var client = sut.GetTestClient();
        var values = await PrepareForm(client, "/stellaradmin/products/edit/2");
        values.Add(new("Entity.Tags", ""));
        values.Add(new("Entity.Tags", "3"));

        // Act
        using var response = await client.PostAsync(
            "/stellaradmin/products/edit/2",
            new FormUrlEncodedContent(values)
        );

        // Assert
        await Assert.That(response.StatusCode).IsEqualTo(HttpStatusCode.Redirect);
        await Assert.That(await ReadTagIds(sut, 2)).IsEquivalentTo([3]);
    }

    [Test]
    public async Task InlineEditRow_ReplacesTheRelatedEntities()
    {
        // Arrange
        await using var sut = await EfCoreTestHost.CreateAsync(resource =>
        {
            resource.Index(index =>
                index.Columns(columns =>
                    columns.Add(
                        product => product.Tags,
                        column =>
                            column.UseDisplay<BadgeDisplay>(badge =>
                                badge.UseText<Tag>(tag => tag.Name)
                            )
                    )
                )
            );
            resource.AllowDelete();
            resource.AllowEdit(edit =>
            {
                edit.Fields(UseCheckboxGroup);
                edit.UseInlineRow();
            });
        });
        using var client = sut.GetTestClient();
        var values = await PrepareForm(client, "/stellaradmin/products");
        values.Add(new("rows[2].Tags", "3"));

        // Act
        using var response = await client.PostAsync(
            "/stellaradmin/products/editrow/2",
            new FormUrlEncodedContent(values)
        );

        // Assert
        await Assert.That(response.IsSuccessStatusCode).IsTrue();
        await Assert.That(await ReadTagIds(sut, 2)).IsEquivalentTo([3]);
    }

    private static async Task<int> CountTags(WebApplication sut)
    {
        using var scope = sut.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<CatalogDbContext>();

        return await db.Set<Tag>().CountAsync();
    }

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
        client.DefaultRequestHeaders.Add(
            "Cookie",
            response.Headers.GetValues("Set-Cookie").Select(cookie => cookie.Split(';')[0])
        );

        return [new("__RequestVerificationToken", token)];
    }

    private static async Task<IDocument> ReadDocument(HttpResponseMessage response) =>
        await new HtmlParser().ParseDocumentAsync(await response.Content.ReadAsStringAsync());

    private static async Task<int[]> ReadTagIds(WebApplication sut, int number)
    {
        using var scope = sut.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<CatalogDbContext>();

        return await db.Set<Product>()
            .Where(product => product.Number == number)
            .SelectMany(product => product.Tags)
            .Select(tag => tag.Id)
            .ToArrayAsync();
    }

    [Test]
    public async Task Edit_WithCustomEditor_Starts()
    {
        // Arrange

        // Act
        await using var sut = await EfCoreTestHost.CreateAsync(resource =>
            resource.AllowEdit(edit =>
                edit.Fields(fields => fields.Add(product => product.Tags).UseEditor<TagEditor>())
            )
        );

        // Assert
        var options = sut.Services.GetRequiredService<IOptions<ResourceOptions<Product>>>().Value;
        await Assert.That(options.Edit!.Fields.Single().Editor).IsTypeOf<TagEditor>();
    }

    [Test]
    [Arguments("none")]
    [Arguments("select")]
    public async Task Edit_WithSingleValueEditor_FailsToStart(string editor)
    {
        // Arrange

        // Act
        async Task act()
        {
            await using var sut = await EfCoreTestHost.CreateAsync(resource =>
                resource.AllowEdit(edit =>
                    edit.Fields(fields =>
                    {
                        var field = fields.Add(product => product.Tags);
                        if (editor == "select")
                        {
                            field.UseEditor<SelectEditor>(select =>
                                select.UseItems<CatalogDbContext, Tag, int>(
                                    tag => tag.Id,
                                    tag => tag.Name
                                )
                            );
                        }
                    })
                )
            );
        }

        // Assert
        var exception = await Assert.That(act).Throws<InvalidOperationException>();
        await Assert
            .That(exception!.Message)
            .Contains(
                "Product.Tags is a many-to-many field, so it needs an editor that selects several items."
            );
    }

    private static void UseCheckboxGroup(ResourceFieldsBuilder<Product> fields) =>
        fields
            .Add(product => product.Tags)
            .UseEditor<CheckboxGroupEditor>(editor =>
                editor.UseItems<CatalogDbContext, Tag, int>(tag => tag.Id, tag => tag.Name)
            );

    private static void UseToggleGroup(ResourceFieldsBuilder<Product> fields) =>
        fields
            .Add(product => product.Tags)
            .UseEditor<ToggleGroupEditor>(editor =>
                editor.UseItems<CatalogDbContext, Tag, int>(tag => tag.Id, tag => tag.Name)
            );

    private static void UseMultiLookupSheet(ResourceFieldsBuilder<Product> fields) =>
        fields
            .Add(product => product.Tags)
            .UseEditor<MultiLookupSheetEditor>(editor =>
                editor.UseItems<CatalogDbContext, Tag, int>(tag => tag.Id, tag => tag.Name)
            );

    private sealed class TagEditor : FieldEditor
    {
        public override string TemplateName => "Tags";
    }
}
