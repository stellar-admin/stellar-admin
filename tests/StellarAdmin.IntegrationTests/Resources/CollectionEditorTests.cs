using System.Net;
using System.Text.Json;
using AngleSharp.Dom;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using StellarAdmin.IntegrationTests.Fixtures;
using StellarAdmin.IntegrationTests.Infrastructure;
using StellarAdmin.Resources;
using StellarAdmin.Resources.Builders;
using StellarAdmin.Resources.Displays;
using StellarAdmin.Resources.Editors;
using StellarAdmin.UI;
using static StellarAdmin.IntegrationTests.Infrastructure.FormTestHelpers;

namespace StellarAdmin.IntegrationTests.Resources;

public class CollectionEditorTests
{
    private const string CreateUrl = "/stellaradmin/tours/create";
    private const string EditUrl = "/stellaradmin/tours/edit/7";

    [Test]
    public async Task ListAndDictionaryFields_WithoutEditor_RenderListAndDictionaryEditors()
    {
        // Arrange
        await using var sut = await CreateToursHost(
            new(),
            create =>
                create.Fields(fields =>
                {
                    fields.Add(tour => tour.Highlights);
                    fields.Add(tour => tour.LocalNames);
                })
        );
        using var client = sut.GetTestClient();

        // Act
        var document = await client.GetDocumentAsync(CreateUrl);

        // Assert
        var list = Editor(document, "Entity.Highlights");
        var dictionary = Editor(document, "Entity.LocalNames");
        await Assert.That(list.GetAttribute("data-kind")).IsEqualTo("list");
        await Assert.That(dictionary.GetAttribute("data-kind")).IsEqualTo("dictionary");
        await Assert
            .That(dictionary.TextContents(".sa-collection-editor-head > span:not(:empty)"))
            .IsEquivalentTo(["Key", "Value"]);
    }

    [Test]
    public async Task UseListEditor_WithSettings_RendersThem()
    {
        // Arrange
        await using var sut = await CreateToursHost(
            new(),
            create =>
                create.Fields(fields =>
                    fields
                        .Add(tour => tour.Highlights)
                        .UseListEditor(list =>
                        {
                            list.MaxItems = 8;
                            list.Numbered = true;
                            list.Sortable = true;
                            list.Unique = true;
                            list.Placeholder = "e.g. Tasting at a family-run tasca";
                            list.AddText = "Add highlight";
                            list.EmptyText = "No highlights yet";
                        })
                )
        );
        using var client = sut.GetTestClient();

        // Act
        var document = await client.GetDocumentAsync(CreateUrl);

        // Assert
        var editor = Editor(document, "Entity.Highlights");
        await Assert.That(editor.HasAttribute("data-numbered")).IsTrue();
        await Assert.That(editor.HasAttribute("data-sortable")).IsTrue();
        await Assert.That(editor.HasAttribute("data-unique")).IsTrue();
        await Assert.That(editor.GetAttribute("data-max-items")).IsEqualTo("8");
        await Assert
            .That(editor.RequiredElement(".sa-collection-editor-add").TextContent)
            .Contains("Add highlight");
        await Assert
            .That(editor.RequiredElement(".sa-collection-editor-empty").TextContent.Trim())
            .IsEqualTo("No highlights yet");
        await Assert
            .That(Template(editor).RequiredElement("input").GetAttribute("placeholder"))
            .IsEqualTo("e.g. Tasting at a family-run tasca");
    }

    [Test]
    public async Task UseDictionaryEditor_WithKeyItems_RendersSelectOfItems()
    {
        // Arrange
        await using var sut = await CreateToursHost(
            new(),
            create =>
                create.Fields(fields =>
                    fields
                        .Add(tour => tour.LocalNames)
                        .UseDictionaryEditor(dictionary =>
                        {
                            dictionary.Key(key =>
                            {
                                key.Label = "Language";
                                key.UseItems([
                                    new ChoiceItem("pt", "Portuguese"),
                                    new ChoiceItem("es", "Spanish"),
                                ]);
                            });
                            dictionary.Value(value => value.Label = "Local name");
                        })
                )
        );
        using var client = sut.GetTestClient();

        // Act
        var document = await client.GetDocumentAsync(CreateUrl);

        // Assert
        var editor = Editor(document, "Entity.LocalNames");
        var options = Template(editor).QuerySelectorAll("select option");
        await Assert
            .That(editor.TextContents(".sa-collection-editor-head > span:not(:empty)"))
            .IsEquivalentTo(["Language", "Local name"]);
        await Assert
            .That(options.Select(option => option.TextContent))
            .IsEquivalentTo(["", "Portuguese", "Spanish"]);
        await Assert.That(editor.GetAttribute("data-max-items")).IsEqualTo("2");
    }

    [Test]
    public async Task Create_WithPostedRows_BindsListsAndDictionaries()
    {
        // Arrange
        var store = new TourStore();
        await using var sut = await CreateToursHost(
            store,
            create =>
                create.Fields(fields =>
                {
                    fields.Add(tour => tour.Highlights);
                    fields.Add(tour => tour.DepartureDays);
                    fields.Add(tour => tour.SeasonalPrices);
                })
        );
        using var client = sut.GetTestClient();
        var values = await PrepareForm(client, CreateUrl);
        values["Entity.Highlights[0]"] = "Tram 28 to Alfama";
        values["Entity.Highlights[1]"] = "Pastéis de Belém";
        values["Entity.DepartureDays[0]"] = "4";
        values["Entity.SeasonalPrices[0].Key"] = "Summer";
        values["Entity.SeasonalPrices[0].Value"] = "1290.50";
        using var content = new FormUrlEncodedContent(values);

        // Act
        using var response = await client.PostAsync(CreateUrl, content);

        // Assert
        await Assert.That(response.StatusCode).IsEqualTo(HttpStatusCode.Redirect);
        await Assert
            .That(store.Saved!.Highlights)
            .IsEquivalentTo(["Tram 28 to Alfama", "Pastéis de Belém"]);
        await Assert.That(store.Saved.DepartureDays).IsEquivalentTo([4]);
        await Assert.That(store.Saved.SeasonalPrices[Season.Summer]).IsEqualTo(1290.50m);
    }

    [Test]
    public async Task Edit_WithOnlyTheMarkerPosted_SavesEmptyCollections()
    {
        // Arrange
        var store = new TourStore();
        store.Tours.Add(
            new()
            {
                Id = 7,
                Highlights = ["Tram 28 to Alfama"],
                LocalNames = new() { ["pt"] = "Passeio por Lisboa" },
            }
        );
        await using var sut = await CreateToursHost(
            store,
            edit: edit =>
                edit.Fields(fields =>
                {
                    fields.Add(tour => tour.Highlights);
                    fields.Add(tour => tour.LocalNames);
                })
        );
        using var client = sut.GetTestClient();
        var values = await PrepareForm(client, EditUrl);
        values["__sa_checkbox_group.Entity.Highlights"] = "true";
        values["__sa_checkbox_group.Entity.LocalNames"] = "true";
        using var content = new FormUrlEncodedContent(values);

        // Act
        using var response = await client.PostAsync(EditUrl, content);

        // Assert
        await Assert.That(response.StatusCode).IsEqualTo(HttpStatusCode.Redirect);
        await Assert.That(store.Saved!.Highlights).IsEmpty();
        await Assert.That(store.Saved.LocalNames).IsEmpty();
    }

    [Test]
    public async Task Create_WithDuplicateInUniqueList_ShowsErrorUnderRow()
    {
        // Arrange
        var store = new TourStore();
        await using var sut = await CreateToursHost(
            store,
            create =>
                create.Fields(fields =>
                    fields.Add(tour => tour.Highlights).UseListEditor(list => list.Unique = true)
                )
        );
        using var client = sut.GetTestClient();
        var values = await PrepareForm(client, CreateUrl);
        values["Entity.Highlights[0]"] = "Fado night";
        values["Entity.Highlights[1]"] = "fado night ";
        using var content = new FormUrlEncodedContent(values);

        // Act
        using var response = await client.PostAsync(CreateUrl, content);
        var document = await response.ReadDocumentAsync();

        // Assert
        await Assert.That(response.StatusCode).IsEqualTo(HttpStatusCode.OK);
        await Assert.That(store.Saved).IsNull();
        await Assert
            .That(RowErrors(document, "Entity.Highlights"))
            .IsEquivalentTo([null, "fado night is already in the list."]);
    }

    [Test]
    public async Task Create_WithTooFewItems_ShowsErrorUnderField()
    {
        // Arrange
        await using var sut = await CreateToursHost(
            new(),
            create =>
                create.Fields(fields =>
                    fields.Add(tour => tour.Highlights).UseListEditor(list => list.MinItems = 2)
                )
        );
        using var client = sut.GetTestClient();
        var values = await PrepareForm(client, CreateUrl);
        values["__sa_checkbox_group.Entity.Highlights"] = "true";
        values["Entity.Highlights[0]"] = "Fado night";
        using var content = new FormUrlEncodedContent(values);

        // Act
        using var response = await client.PostAsync(CreateUrl, content);
        var document = await response.ReadDocumentAsync();

        // Assert
        await Assert
            .That(document.RequiredElement("[data-slot='field-error']").TextContent.Trim())
            .IsEqualTo("Add at least 2 items.");
    }

    [Test]
    public async Task Create_WithPropertyMinLength_ShowsOnlyTheAttributeError()
    {
        // Arrange
        await using var sut = await CreateToursHost(
            new(),
            create => create.Fields(fields => fields.Add(tour => tour.Themes))
        );
        using var client = sut.GetTestClient();
        var values = await PrepareForm(client, CreateUrl);
        values["__sa_checkbox_group.Entity.Themes"] = "true";
        using var content = new FormUrlEncodedContent(values);

        // Act
        using var response = await client.PostAsync(CreateUrl, content);
        var document = await response.ReadDocumentAsync();

        // Assert
        var errors = document.QuerySelectorAll("[data-slot='field-error']");
        await Assert.That(errors.Length).IsEqualTo(1);
        await Assert.That(errors[0].TextContent).Contains("minimum length of '1'");
    }

    [Test]
    public async Task Create_WithInvalidItems_ShowsTheEditorsMessagesUnderRows()
    {
        // Arrange
        await using var sut = await CreateToursHost(
            new(),
            create =>
                create.Fields(fields =>
                {
                    fields
                        .Add(tour => tour.Highlights)
                        .UseListEditor(list =>
                        {
                            list.MaxLength = 12;
                            list.Pattern = "[A-Z].*";
                            list.PatternMessage = "Start with a capital letter.";
                        });
                    fields
                        .Add(tour => tour.DepartureDays)
                        .UseListEditor(list =>
                        {
                            list.Min = 1;
                            list.Max = 31;
                        });
                })
        );
        using var client = sut.GetTestClient();
        var values = await PrepareForm(client, CreateUrl);
        values["Entity.Highlights[0]"] = "fado night";
        values["Entity.Highlights[1]"] = "Tram 28 to Alfama";
        values["Entity.Highlights[2]"] = "Belém";
        values["Entity.DepartureDays[0]"] = "32";
        values["Entity.DepartureDays[1]"] = "12";
        using var content = new FormUrlEncodedContent(values);

        // Act
        using var response = await client.PostAsync(CreateUrl, content);
        var document = await response.ReadDocumentAsync();

        // Assert
        await Assert
            .That(RowErrors(document, "Entity.Highlights"))
            .IsEquivalentTo(["Start with a capital letter.", "Use 12 characters or fewer.", null]);
        await Assert
            .That(RowErrors(document, "Entity.DepartureDays"))
            .IsEquivalentTo(["Enter a number from 1 to 31.", null]);
    }

    [Test]
    public async Task Create_WithIncompleteAndRepeatedEntries_ShowsErrorsUnderRows()
    {
        // Arrange
        await using var sut = await CreateToursHost(
            new(),
            create =>
                create.Fields(fields =>
                    fields
                        .Add(tour => tour.LocalNames)
                        .UseDictionaryEditor(dictionary =>
                        {
                            dictionary.Key(key => key.Label = "Language");
                            dictionary.Value(value => value.Label = "Local name");
                        })
                )
        );
        using var client = sut.GetTestClient();
        var values = await PrepareForm(client, CreateUrl);
        values["Entity.LocalNames[0].Key"] = "pt";
        values["Entity.LocalNames[0].Value"] = "Passeio por Lisboa";
        values["Entity.LocalNames[1].Key"] = "PT";
        values["Entity.LocalNames[1].Value"] = "Lisboa a pé";
        values["Entity.LocalNames[2].Key"] = "es";
        values["Entity.LocalNames[2].Value"] = "";
        using var content = new FormUrlEncodedContent(values);

        // Act
        using var response = await client.PostAsync(CreateUrl, content);
        var document = await response.ReadDocumentAsync();

        // Assert
        await Assert
            .That(RowErrors(document, "Entity.LocalNames"))
            .IsEquivalentTo([null, "PT is already used.", "Local name is required."]);
    }

    [Test]
    public async Task Create_WhenHandlerRejectsAnItem_ShowsTheErrorUnderItsRow()
    {
        // Arrange
        var store = new TourStore
        {
            SaveResult = ResourceOperationResult.ValidationFailed([
                new ResourceValidationError("Highlights[1]", "This stop is closed in winter."),
                new ResourceValidationError("LocalNames[0].Key", "Use a language we publish in."),
            ]),
        };
        await using var sut = await CreateToursHost(
            store,
            create =>
                create.Fields(fields =>
                {
                    fields.Add(tour => tour.Highlights);
                    fields.Add(tour => tour.LocalNames);
                })
        );
        using var client = sut.GetTestClient();
        var values = await PrepareForm(client, CreateUrl);
        values["Entity.Highlights[0]"] = "Fado night";
        values["Entity.Highlights[1]"] = "Sintra palaces";
        values["Entity.LocalNames[0].Key"] = "xx";
        values["Entity.LocalNames[0].Value"] = "Passeio";
        using var content = new FormUrlEncodedContent(values);

        // Act
        using var response = await client.PostAsync(CreateUrl, content);
        var document = await response.ReadDocumentAsync();

        // Assert
        await Assert
            .That(RowErrors(document, "Entity.Highlights"))
            .IsEquivalentTo([null, "This stop is closed in winter."]);
        await Assert
            .That(RowErrors(document, "Entity.LocalNames"))
            .IsEquivalentTo(["Use a language we publish in."]);
        await Assert.That(document.QuerySelector(".validation-summary-errors")).IsNull();
    }

    [Test]
    public async Task ConfigureResourceLabels_WithListEditorTexts_UsesThem()
    {
        // Arrange
        await using var sut = await CreateToursHost(
            new(),
            create =>
                create.Fields(fields =>
                    fields.Add(tour => tour.Highlights).UseListEditor(list => list.MinItems = 1)
                ),
            admin =>
                admin.ConfigureResourceLabels(labels =>
                    labels.ListEditor(list =>
                    {
                        list.AddText = context => $"Adicionar a {context.FieldLabel}";
                        list.TooFewText = context => $"Adicione pelo menos {context.MinItems}.";
                        list.RemoveLabel = context => "Remover item {n}";
                    })
                )
        );
        using var client = sut.GetTestClient();
        var values = await PrepareForm(client, CreateUrl);
        values["__sa_checkbox_group.Entity.Highlights"] = "true";
        using var content = new FormUrlEncodedContent(values);

        // Act
        using var response = await client.PostAsync(CreateUrl, content);
        var document = await response.ReadDocumentAsync();

        // Assert
        var editor = Editor(document, "Entity.Highlights");
        var texts = JsonSerializer.Deserialize<Dictionary<string, string>>(
            editor.GetAttribute("data-texts")!
        )!;
        await Assert
            .That(editor.RequiredElement(".sa-collection-editor-add").TextContent)
            .Contains("Adicionar a Highlights");
        await Assert.That(texts["remove"]).IsEqualTo("Remover item {n}");
        await Assert.That(texts["minItems"]).IsEqualTo("Adicione pelo menos 1.");
        await Assert
            .That(document.RequiredElement("[data-slot='field-error']").TextContent.Trim())
            .IsEqualTo("Adicione pelo menos 1.");
    }

    [Test]
    public async Task ViewPage_WithListAndDictionary_ShowsBadges()
    {
        // Arrange
        var store = new TourStore();
        store.Tours.Add(
            new()
            {
                Id = 7,
                Highlights = ["Tram 28 to Alfama", "Pastéis de Belém"],
                SeasonalPrices = new() { [Season.Summer] = 1290m, [Season.Autumn] = 990m },
            }
        );
        await using var sut = await CreateToursHost(
            store,
            view: view =>
                view.Fields(fields =>
                {
                    fields.Add(tour => tour.Highlights);
                    fields.Add(tour => tour.SeasonalPrices);
                })
        );
        using var client = sut.GetTestClient();

        // Act
        var document = await client.GetDocumentAsync("/stellaradmin/tours/view/7");

        // Assert
        await Assert
            .That(document.TextContents("[data-slot='badge']"))
            .IsEquivalentTo([
                "Tram 28 to Alfama",
                "Pastéis de Belém",
                "Summer: 1290",
                "Autumn and winter: 990",
            ]);
    }

    [Test]
    public async Task BadgeDisplay_WithLimit_ShowsTheRestAsOneBadge()
    {
        // Arrange
        var store = new TourStore();
        store.Tours.Add(
            new()
            {
                Id = 7,
                Name = "Lisbon in a day",
                Highlights = ["Alfama", "Belém", "Sintra", "Cascais"],
            }
        );
        await using var sut = await CreateToursHost(
            store,
            configureIndex: columns =>
                columns.Add(
                    tour => tour.Highlights,
                    column => column.UseDisplay<BadgeDisplay>(badge => badge.Limit = 2)
                )
        );
        using var client = sut.GetTestClient();

        // Act
        var document = await client.GetDocumentAsync("/stellaradmin/tours");

        // Assert
        var badges = document.QuerySelectorAll("td [data-slot='badge']");
        await Assert
            .That(badges.Select(badge => badge.TextContent.Trim()))
            .IsEquivalentTo(["Alfama", "Belém", "+2"]);
        await Assert.That(badges[^1].GetAttribute("title")).IsEqualTo("Sintra, Cascais");
    }

    [Test]
    public async Task InlineEditRow_WithListField_ShowsBadgesInPlaceOfTheEditor()
    {
        // Arrange
        var store = new TourStore();
        store.Tours.Add(
            new()
            {
                Id = 7,
                Name = "Lisbon in a day",
                Highlights = ["Alfama"],
            }
        );
        await using var sut = await CreateToursHost(
            store,
            edit: edit =>
            {
                edit.Fields(fields =>
                {
                    fields.Add(tour => tour.Name);
                    fields.Add(tour => tour.Highlights);
                });
                edit.UseInlineRow(_ => { });
            },
            configureIndex: columns => columns.Add(tour => tour.Highlights)
        );
        using var client = sut.GetTestClient();

        // Act
        using var response = await client.GetAsync("/stellaradmin/tours/editrow/7");
        var document = await response.ReadRowsAsync();

        // Assert
        await Assert.That(document.QuerySelector("sel-collection-editor")).IsNull();
        await Assert
            .That(document.TextContents("td [data-slot='badge']"))
            .IsEquivalentTo(["Alfama"]);
    }

    [Test]
    public async Task InlineEditRow_WhenSaved_KeepsTheListItShows()
    {
        // Arrange
        var store = new TourStore();
        store.Tours.Add(
            new()
            {
                Id = 7,
                Name = "Lisbon in a day",
                Highlights = ["Alfama"],
            }
        );
        await using var sut = await CreateToursHost(
            store,
            edit: edit =>
            {
                edit.Fields(fields =>
                {
                    fields.Add(tour => tour.Name);
                    fields.Add(tour => tour.Highlights).UseListEditor(list => list.MinItems = 1);
                });
                edit.UseInlineRow(_ => { });
            },
            configureIndex: columns => columns.Add(tour => tour.Highlights)
        );
        using var client = sut.GetTestClient();
        var values = await PrepareRowForm(client, "/stellaradmin/tours/editrow/7");
        values["rows[7].Name"] = "Lisbon by tram";
        using var content = new FormUrlEncodedContent(values);

        // Act
        using var response = await client.PostAsync("/stellaradmin/tours/editrow/7", content);

        // Assert
        await Assert.That(response.StatusCode).IsEqualTo(HttpStatusCode.OK);
        await Assert.That(store.Saved!.Name).IsEqualTo("Lisbon by tram");
        await Assert.That(store.Saved.Highlights).IsEquivalentTo(["Alfama"]);
    }

    private static Task<WebApplication> CreateToursHost(
        TourStore store,
        Action<ResourceCreateBuilder<Tour>>? create = null,
        Action<StellarAdminBuilder>? configureAdmin = null,
        Action<ResourceEditBuilder<Tour>>? edit = null,
        Action<ResourceViewBuilder<Tour>>? view = null,
        Action<ResourceColumnsBuilder<Tour>>? configureIndex = null
    ) =>
        StellarAdminTestHost.CreateAsync(
            new([]),
            configureAdmin: admin =>
            {
                admin.Services.AddSingleton(store);
                admin.AddResource<Tour>(
                    "tours",
                    resource =>
                    {
                        resource.UseDataSource<TourDataSource>();
                        resource.UseKey(tour => tour.Id);
                        resource.Index(index =>
                            index.Columns(columns =>
                            {
                                columns.Add(tour => tour.Name);
                                configureIndex?.Invoke(columns);
                            })
                        );
                        if (create is not null)
                        {
                            resource.AllowCreate(create);
                        }

                        if (edit is not null)
                        {
                            resource.AllowEdit(edit);
                        }

                        if (view is not null)
                        {
                            resource.AllowView(view);
                        }
                    }
                );
                configureAdmin?.Invoke(admin);
            }
        );

    private static IElement Editor(IParentNode document, string name) =>
        document.RequiredElement($"sel-collection-editor[data-name='{name}']");

    // The editing row renders the token the row posts
    private static async Task<Dictionary<string, string>> PrepareRowForm(
        HttpClient client,
        string url
    )
    {
        using var response = await client.GetAsync(url);
        response.EnsureSuccessStatusCode();
        var token = (await response.ReadRowsAsync())
            .RequiredElement("input[name='__RequestVerificationToken']")
            .GetAttribute("value")!;
        client.DefaultRequestHeaders.Add(
            "Cookie",
            response.Headers.GetValues("Set-Cookie").Select(cookie => cookie.Split(';')[0])
        );

        return new() { ["__RequestVerificationToken"] = token };
    }

    private static IParentNode Template(IElement editor) =>
        ((AngleSharp.Html.Dom.IHtmlTemplateElement)editor.RequiredElement("template")).Content;

    // Each row's error, or null for a row without one
    private static string?[] RowErrors(IParentNode document, string name) =>
        Editor(document, name)
            .QuerySelectorAll(".sa-collection-editor-items > li")
            .Select(row =>
                row.QuerySelector("[data-slot='collection-editor-item-error']")?.TextContent.Trim()
            )
            .ToArray();
}
