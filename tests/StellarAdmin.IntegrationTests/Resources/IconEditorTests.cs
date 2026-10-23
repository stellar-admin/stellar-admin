using System.Net;
using AngleSharp.Dom;
using AngleSharp.Html.Dom;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using StellarAdmin.IntegrationTests.Fixtures;
using StellarAdmin.IntegrationTests.Infrastructure;
using StellarAdmin.Resources.Builders;
using StellarAdmin.Resources.Editors;
using static StellarAdmin.IntegrationTests.Infrastructure.FormTestHelpers;

namespace StellarAdmin.IntegrationTests.Resources;

public class IconEditorTests
{
    private const string EditUrl = "/stellaradmin/products/edit/7";

    private static readonly string[] Amenities = ["wifi", "coffee", "bus"];

    [Test]
    public async Task Edit_WithoutIcons_RendersPickerLoadingEveryIconFromEndpoint()
    {
        // Arrange
        await using var sut = await CreateAsync(fields =>
            fields.Add(model => model.Icon).UseIconEditor()
        );
        using var client = sut.GetTestClient();

        // Act
        var document = await client.GetDocumentAsync(EditUrl);

        // Assert
        var picker = Picker(document, "Icon");
        await Assert.That(picker.GetAttribute("data-layout")).IsEqualTo("picker");
        await Assert
            .That(picker.GetAttribute("data-icons-url"))
            .Matches("^/stellaradmin/home/icons\\?v=[0-9a-f]{16}$");
        await Assert
            .That(picker.RequiredElement("[data-slot='icon-picker-text']").TextContent)
            .IsEqualTo("coffee");
        await Assert.That(picker.QuerySelector("template")).IsNull();
    }

    [Test]
    [Arguments(IconEditorLayout.Auto, 24, "inline")]
    [Arguments(IconEditorLayout.Auto, 25, "picker")]
    [Arguments(IconEditorLayout.Picker, 3, "picker")]
    [Arguments(IconEditorLayout.Inline, 30, "inline")]
    public async Task Edit_WithIcons_RendersLayoutWithoutEndpoint(
        IconEditorLayout layout,
        int count,
        string expected
    )
    {
        // Arrange
        var icons = Icons(count);
        await using var sut = await CreateAsync(fields =>
            fields
                .Add(model => model.Icon)
                .UseIconEditor(editor =>
                {
                    editor.Icons = icons;
                    editor.Layout = layout;
                })
        );
        using var client = sut.GetTestClient();

        // Act
        var document = await client.GetDocumentAsync(EditUrl);

        // Assert
        var picker = Picker(document, "Icon");
        await Assert.That(picker.GetAttribute("data-layout")).IsEqualTo(expected);
        await Assert.That(picker.HasAttribute("data-icons-url")).IsFalse();
        var names =
            expected == "inline"
                ? picker
                    .QuerySelectorAll("input[type='radio']")
                    .Select(input => input.GetAttribute("value"))
                : (
                    (IHtmlTemplateElement)picker.RequiredElement("template")
                ).Content.Children.Select(svg => svg.GetAttribute("data-icon-name"));
        await Assert.That(names).IsEquivalentTo(icons);
    }

    [Test]
    public async Task Edit_WithIconsAndUnlistedStoredValue_KeepsStoredValue()
    {
        // Arrange
        await using var sut = await CreateAsync(fields =>
            fields.Add(model => model.Badge).UseIconEditor(Configure)
        );
        using var client = sut.GetTestClient();

        // Act
        var document = await client.GetDocumentAsync(EditUrl);

        // Assert
        var picker = Picker(document, "Badge");
        await Assert.That(picker.QuerySelector("input[type='radio']:checked")).IsNull();
        var fallback = picker.RequiredElement("input[type='hidden'][name='Entity.Badge']");
        await Assert.That(fallback.GetAttribute("value")).IsEqualTo("spa-flower");
        await Assert.That(fallback.HasAttribute("disabled")).IsFalse();
    }

    [Test]
    public async Task EditRow_WithIconEditor_RendersSmallTriggerOpeningPicker()
    {
        // Arrange
        await using var sut = await StellarAdminTestHost.CreateAsync(
            new([new(7, "coffee", 8.50m)]),
            resource =>
            {
                resource.UseKey(product => product.Id);
                resource.AllowEdit(edit =>
                {
                    edit.Fields(fields =>
                        fields.Add(product => product.Name).UseIconEditor(Configure)
                    );
                    edit.UseInlineRow();
                });
            }
        );
        using var client = sut.GetTestClient();

        // Act
        var document = await client.GetDocumentAsync("/stellaradmin/products/editrow/7");

        // Assert
        var picker = document.RequiredElement("sel-icon-picker");
        await Assert.That(picker.GetAttribute("data-layout")).IsEqualTo("picker");
        await Assert
            .That(
                picker
                    .RequiredElement("[data-slot='icon-picker-trigger']")
                    .ClassList.Contains("sa-icon-picker-cell-trigger")
            )
            .IsTrue();
        await Assert
            .That(
                picker.RequiredElement("input[data-slot='icon-picker-value']").GetAttribute("name")
            )
            .IsEqualTo("rows[7].Name");
    }

    [Test]
    [Arguments("anchor", "bus")]
    [Arguments("", "")]
    public async Task Save_WithOfferedIconsOrEmptyValues_SavesValues(string icon, string badge)
    {
        // Arrange
        var icons = new AmenityIcons();
        await using var sut = await CreateAsync(
            fields =>
            {
                fields.Add(model => model.Icon).UseIconEditor();
                fields.Add(model => model.Badge).UseIconEditor(Configure);
            },
            icons
        );
        using var client = sut.GetTestClient();
        var values = await PrepareForm(client, EditUrl);
        values["Entity.Icon"] = icon;
        values["Entity.Badge"] = badge;
        using var content = new FormUrlEncodedContent(values);

        // Act
        using var response = await client.PostAsync(EditUrl, content);

        // Assert
        await Assert.That(response.StatusCode).IsEqualTo(HttpStatusCode.Redirect);
        await Assert.That(icons.Saved?.Icon).IsEqualTo(icon == "" ? null : icon);
        await Assert.That(icons.Saved?.Badge).IsEqualTo(badge == "" ? null : badge);
    }

    [Test]
    public async Task Save_WithUnchangedStoredValueThatIsNotOffered_SavesValue()
    {
        // Arrange
        var icons = new AmenityIcons();
        await using var sut = await CreateAsync(
            fields => fields.Add(model => model.Badge).UseIconEditor(Configure),
            icons
        );
        using var client = sut.GetTestClient();
        var values = await PrepareForm(client, EditUrl);
        values["Entity.Badge"] = "spa-flower";
        using var content = new FormUrlEncodedContent(values);

        // Act
        using var response = await client.PostAsync(EditUrl, content);

        // Assert
        await Assert.That(response.StatusCode).IsEqualTo(HttpStatusCode.Redirect);
        await Assert.That(icons.Saved?.Badge).IsEqualTo("spa-flower");
    }

    [Test]
    [Arguments("Icon", "not-an-icon")]
    [Arguments("Badge", "anchor")]
    [Arguments("Badge", "not-an-icon")]
    public async Task Save_WithIconTheFieldDoesNotOffer_ShowsErrorWithoutSaving(
        string field,
        string value
    )
    {
        // Arrange
        var icons = new AmenityIcons();
        await using var sut = await CreateAsync(
            fields =>
            {
                fields.Add(model => model.Icon).UseIconEditor();
                fields.Add(model => model.Badge).UseIconEditor(Configure);
            },
            icons
        );
        using var client = sut.GetTestClient();
        var values = await PrepareForm(client, EditUrl);
        values["Entity.Icon"] = "coffee";
        values["Entity.Badge"] = "spa-flower";
        values[$"Entity.{field}"] = value;
        using var content = new FormUrlEncodedContent(values);

        // Act
        using var response = await client.PostAsync(EditUrl, content);

        // Assert
        await Assert.That(response.StatusCode).IsEqualTo(HttpStatusCode.OK);
        await Assert.That(icons.Saved).IsNull();
        var document = await response.ReadDocumentAsync();
        var picker = Picker(document, field);
        await Assert
            .That(
                picker
                    .Closest(".sa-field")!
                    .RequiredElement("[data-slot='field-error']")
                    .TextContent.Trim()
            )
            .IsEqualTo("Choose an icon from the list.");
    }

    [Test]
    [Arguments(
        new[] { "wifi", "spa-flower" },
        IconEditorLayout.Auto,
        "The Icon field on the edit form doesn't offer spa-flower because they aren't registered."
    )]
    [Arguments(
        new string[0],
        IconEditorLayout.Auto,
        "The Icon field on the edit form offers every registered icon because its Icons list is empty."
    )]
    [Arguments(
        null,
        IconEditorLayout.Inline,
        "The Icon field on the edit form doesn't use the inline layout because it has no list of icons."
    )]
    public async Task Startup_WithInvalidIconEditor_LogsProblem(
        string[]? icons,
        IconEditorLayout layout,
        string expected
    )
    {
        // Arrange

        // Act
        await using var app = await CreateAsync(fields =>
            fields
                .Add(model => model.Icon)
                .UseIconEditor(editor =>
                {
                    editor.Icons = icons;
                    editor.Layout = layout;
                })
        );

        // Assert
        var logs = app.Services.GetRequiredService<RecordingLoggerProvider>();
        await Assert
            .That(logs.ConfigurationProblems.Any(log => log.Message.Contains(expected)))
            .IsTrue();
    }

    private static void Configure(IconEditor editor) => editor.Icons = Amenities;

    // Registered Lucide names, in the order the pack lists them
    private static string[] Icons(int count) =>
        new StellarAdmin.UI.Icons.LucideIconPack().GetIcons().Keys.Take(count).ToArray();

    private static IElement Picker(IDocument document, string name) =>
        document.QuerySelectorAll($"[name='Entity.{name}']").First().Closest("sel-icon-picker")!;

    private static Task<WebApplication> CreateAsync(
        Action<ResourceFieldsBuilder<AmenityIconModel>> fields,
        AmenityIcons? icons = null
    ) =>
        StellarAdminTestHost.CreateAsync(
            new([new(7, "Breakfast", 8.50m)]),
            resource =>
            {
                resource.UseKey(product => product.Id);
                resource.AllowEdit<AmenityIconModel, AmenityIconHandler>(edit =>
                    edit.Fields(fields)
                );
            },
            configureServices: services => services.AddSingleton(icons ?? new AmenityIcons())
        );
}
