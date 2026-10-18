using System.ComponentModel.DataAnnotations;
using AngleSharp.Html.Dom;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Razor.TagHelpers;
using StellarAdmin.UI.Tests.Support;

namespace StellarAdmin.UI.Tests.TagHelpers.IconPicker;

public class IconPickerTagHelperTests
{
    [Test]
    public async Task ProcessAsync_WithChildIcons_RendersTriggerHiddenInputAndTemplate()
    {
        // Arrange
        using var context = new RenderingContext();
        var sut = CreateSut(context);
        sut.Name = "AmenityIcon";
        sut.Value = "coffee";

        // Act
        using var html = await TagHelperRenderer.RenderAsync(
            sut,
            Icons(context, "wifi", "coffee", "wifi")
        );

        // Assert
        var picker = html.QuerySelector("sel-icon-picker[data-slot=icon-picker]");
        await Assert.That(picker?.GetAttribute("data-layout")).IsEqualTo("picker");
        await Assert.That(picker?.GetAttribute("data-placeholder")).IsEqualTo("Choose an icon");
        await Assert.That(picker?.HasAttribute("name")).IsFalse();
        var hidden = picker?.QuerySelector("input[type=hidden][data-slot=icon-picker-value]");
        await Assert.That(hidden?.GetAttribute("name")).IsEqualTo("AmenityIcon");
        await Assert.That(hidden?.GetAttribute("value")).IsEqualTo("coffee");
        var trigger = picker?.QuerySelector("button[data-slot=icon-picker-trigger]");
        var popover = picker?.QuerySelector("[data-slot=icon-picker-popover]");
        await Assert.That(trigger?.GetAttribute("popovertarget")).IsEqualTo(popover?.Id);
        await Assert.That(popover?.HasAttribute("popover")).IsTrue();
        await Assert
            .That(trigger?.QuerySelector("[data-slot=icon-picker-text]")?.TextContent)
            .IsEqualTo("coffee");
        await Assert.That(trigger?.QuerySelectorAll("svg").Length).IsEqualTo(2);
        var template =
            html.QuerySelector("template[data-slot=icon-picker-icons]") as IHtmlTemplateElement;
        var names = template
            ?.Content.QuerySelectorAll("svg")
            .Select(svg => svg.GetAttribute("data-icon-name"))
            .ToList();
        await Assert.That(names).IsEquivalentTo(["wifi", "coffee"]);
    }

    [Test]
    public async Task ProcessAsync_WithoutValue_ShowsPlaceholderWithoutIcon()
    {
        // Arrange
        using var context = new RenderingContext();
        var sut = CreateSut(context);
        sut.Placeholder = "Pick an amenity icon";

        // Act
        using var html = await TagHelperRenderer.RenderAsync(sut, Icons(context, "wifi"));

        // Assert
        var trigger = html.QuerySelector("[data-slot=icon-picker-trigger]");
        var text = trigger?.QuerySelector("[data-slot=icon-picker-text]");
        await Assert.That(text?.TextContent).IsEqualTo("Pick an amenity icon");
        await Assert.That(text?.GetAttribute("data-placeholder")).IsEqualTo("true");
        await Assert.That(trigger?.QuerySelectorAll("svg").Length).IsEqualTo(1);
        await Assert
            .That(html.QuerySelector("sel-icon-picker")?.GetAttribute("data-placeholder"))
            .IsEqualTo("Pick an amenity icon");
    }

    [Test]
    public async Task ProcessAsync_ValueIsNotRegistered_ShowsMissingBoxAndKeepsValue()
    {
        // Arrange
        using var context = new RenderingContext();
        var sut = CreateSut(context);
        sut.Value = "hot-tub";

        // Act
        using var html = await TagHelperRenderer.RenderAsync(sut, Icons(context, "wifi"));

        // Assert
        var trigger = html.QuerySelector("[data-slot=icon-picker-trigger]");
        await Assert.That(trigger?.QuerySelector(".sa-icon-missing")).IsNotNull();
        await Assert
            .That(trigger?.QuerySelector("[data-slot=icon-picker-text]")?.TextContent)
            .IsEqualTo("hot-tub");
        await Assert
            .That(html.QuerySelector("[data-slot=icon-picker-value]")?.GetAttribute("value"))
            .IsEqualTo("hot-tub");
    }

    [Test]
    public async Task ProcessAsync_WithIconsUrl_SetsUrlAndRendersNoTemplate()
    {
        // Arrange
        using var context = new RenderingContext();
        var sut = CreateSut(context);
        sut.IconsUrl = "/admin/icons?v=3f2a";

        // Act
        using var html = await TagHelperRenderer.RenderAsync(sut);

        // Assert
        await Assert
            .That(html.QuerySelector("sel-icon-picker")?.GetAttribute("data-icons-url"))
            .IsEqualTo("/admin/icons?v=3f2a");
        await Assert.That(html.QuerySelector("template")).IsNull();
    }

    [Test]
    public async Task ProcessAsync_WithChildIconsAndIconsUrl_Throws()
    {
        // Arrange
        using var context = new RenderingContext();
        var sut = CreateSut(context);
        sut.IconsUrl = "/admin/icons";

        // Act
        var act = async () => await TagHelperRenderer.RenderAsync(sut, Icons(context, "wifi"));

        // Assert
        await Assert.That(act).Throws<InvalidOperationException>();
    }

    [Test]
    public async Task ProcessAsync_WithoutChildIconsOrIconsUrl_Throws()
    {
        // Arrange
        using var context = new RenderingContext();
        var sut = CreateSut(context);

        // Act
        var act = async () => await TagHelperRenderer.RenderAsync(sut);

        // Assert
        await Assert.That(act).Throws<InvalidOperationException>();
    }

    [Test]
    public async Task ProcessAsync_InlineWithIconsUrl_Throws()
    {
        // Arrange
        using var context = new RenderingContext();
        var sut = CreateSut(context);
        sut.Layout = IconPickerLayout.Inline;
        sut.IconsUrl = "/admin/icons";

        // Act
        var act = async () => await TagHelperRenderer.RenderAsync(sut);

        // Assert
        await Assert.That(act).Throws<InvalidOperationException>();
    }

    [Test]
    public async Task ProcessAsync_ChildIconWithoutName_Throws()
    {
        // Arrange
        using var context = new RenderingContext();
        var sut = CreateSut(context);

        // Act
        var act = async () =>
            await TagHelperRenderer.RenderAsync(sut, Icons(context, "wifi", null));

        // Assert
        await Assert.That(act).Throws<InvalidOperationException>();
    }

    [Test]
    public async Task ProcessAsync_BoundToRequiredProperty_LabelTargetsTriggerWithoutClear()
    {
        // Arrange
        using var context = new RenderingContext();
        var sut = CreateSut(context);
        sut.For = new ModelExpression(
            "Icon",
            context
                .Metadata.GetModelExplorerForType(typeof(Amenity), new Amenity { Icon = "wifi" })
                .GetExplorerForProperty(nameof(Amenity.Icon))
        );

        // Act
        using var html = await TagHelperRenderer.RenderAsync(sut, Icons(context, "wifi"));

        // Assert
        var trigger = html.QuerySelector("[data-slot=icon-picker-trigger]");
        await Assert.That(trigger?.Id).IsEqualTo("Icon");
        await Assert
            .That(html.QuerySelector("label[data-slot=field-label]")?.GetAttribute("for"))
            .IsEqualTo("Icon");
        await Assert
            .That(html.QuerySelector("[data-slot=icon-picker-value]")?.GetAttribute("value"))
            .IsEqualTo("wifi");
        await Assert
            .That(html.QuerySelector("sel-icon-picker")?.GetAttribute("data-required"))
            .IsEqualTo("true");
        await Assert.That(html.QuerySelector("[data-action=clear]")).IsNull();
    }

    [Test]
    public async Task ProcessAsync_Optional_RendersClearButton()
    {
        // Arrange
        using var context = new RenderingContext();
        var sut = CreateSut(context);

        // Act
        using var html = await TagHelperRenderer.RenderAsync(sut, Icons(context, "wifi"));

        // Assert
        await Assert
            .That(html.QuerySelector("[data-slot=icon-picker-status] [data-action=clear]"))
            .IsNotNull();
        await Assert
            .That(html.QuerySelector("sel-icon-picker")?.HasAttribute("data-required"))
            .IsFalse();
    }

    [Test]
    public async Task ProcessAsync_WithLabel_MovesIdToTrigger()
    {
        // Arrange
        using var context = new RenderingContext();
        var sut = CreateSut(context);
        sut.Label = "Amenity icon";

        // Act
        using var html = await TagHelperRenderer.RenderAsync(sut, Icons(context, "wifi"));

        // Assert
        var trigger = html.QuerySelector("[data-slot=icon-picker-trigger]");
        await Assert.That(trigger?.Id).IsNotNull();
        await Assert
            .That(html.QuerySelector("label[data-slot=field-label]")?.GetAttribute("for"))
            .IsEqualTo(trigger?.Id);
        await Assert.That(html.QuerySelector("sel-icon-picker")?.HasAttribute("id")).IsFalse();
    }

    [Test]
    public async Task ProcessAsync_Disabled_DisablesTriggerAndValue()
    {
        // Arrange
        using var context = new RenderingContext();
        var sut = CreateSut(context);
        sut.Value = "wifi";
        sut.Disabled = true;

        // Act
        using var html = await TagHelperRenderer.RenderAsync(sut, Icons(context, "wifi"));

        // Assert
        await Assert
            .That(html.QuerySelector("sel-icon-picker")?.GetAttribute("data-disabled"))
            .IsEqualTo("true");
        await Assert
            .That(html.QuerySelector("[data-slot=icon-picker-trigger]")?.HasAttribute("disabled"))
            .IsTrue();
        await Assert
            .That(html.QuerySelector("[data-slot=icon-picker-value]")?.HasAttribute("disabled"))
            .IsTrue();
    }

    [Test]
    public async Task ProcessAsync_Inline_RendersToggleGroupOfRadios()
    {
        // Arrange
        using var context = new RenderingContext();
        var sut = CreateSut(context);
        sut.Name = "TripStyleIcon";
        sut.Value = "tent";
        sut.Layout = IconPickerLayout.Inline;

        // Act
        using var html = await TagHelperRenderer.RenderAsync(
            sut,
            Icons(context, "sailboat", "tent")
        );

        // Assert
        var group = html.QuerySelector(
            "sel-icon-picker[data-layout=inline] > [data-slot=toggle-group]"
        );
        await Assert.That(group?.GetAttribute("data-variant")).IsEqualTo("outline");
        var items = group?.QuerySelectorAll("label[data-slot=toggle-group-item]");
        await Assert.That(items?.Length).IsEqualTo(2);
        await Assert.That(items?[1].GetAttribute("title")).IsEqualTo("tent");
        var radio = items?[1].QuerySelector("input[type=radio]");
        await Assert.That(radio?.GetAttribute("name")).IsEqualTo("TripStyleIcon");
        await Assert.That(radio?.GetAttribute("value")).IsEqualTo("tent");
        await Assert.That(radio?.GetAttribute("aria-label")).IsEqualTo("tent");
        await Assert.That(radio?.HasAttribute("checked")).IsTrue();
        await Assert.That(items?[1].QuerySelector("svg")).IsNotNull();
        var fallback = html.QuerySelector("input[type=hidden][data-slot=icon-picker-value]");
        await Assert.That(fallback?.GetAttribute("name")).IsEqualTo("TripStyleIcon");
        await Assert.That(fallback?.HasAttribute("disabled")).IsTrue();
        await Assert.That(html.QuerySelector("[popover]")).IsNull();
    }

    [Test]
    public async Task ProcessAsync_InlineValueIsNotATile_PostsStoredValue()
    {
        // Arrange
        using var context = new RenderingContext();
        var sut = CreateSut(context);
        sut.Name = "TripStyleIcon";
        sut.Value = "hot-tub";
        sut.Layout = IconPickerLayout.Inline;

        // Act
        using var html = await TagHelperRenderer.RenderAsync(
            sut,
            Icons(context, "sailboat", "tent")
        );

        // Assert
        await Assert.That(html.QuerySelector("input[type=radio][checked]")).IsNull();
        var fallback = html.QuerySelector("input[type=hidden][data-slot=icon-picker-value]");
        await Assert.That(fallback?.GetAttribute("value")).IsEqualTo("hot-tub");
        await Assert.That(fallback?.HasAttribute("disabled")).IsFalse();
    }

    [Test]
    public async Task ProcessAsync_WithClassNames_AddsClassesToParts()
    {
        // Arrange
        using var context = new RenderingContext();
        var sut = CreateSut(context);
        sut.ClassNames = new IconPickerClassNames
        {
            Control = "max-w-sm",
            Trigger = "font-mono",
            Popover = "w-96",
        };

        // Act
        using var html = await TagHelperRenderer.RenderAsync(sut, Icons(context, "wifi"));

        // Assert
        await Assert
            .That(html.QuerySelector("sel-icon-picker")?.ClassName)
            .IsEqualTo("sa-icon-picker max-w-sm existing-class");
        await Assert
            .That(
                html.QuerySelector("[data-slot=icon-picker-trigger]")
                    ?.ClassList.Contains("font-mono")
            )
            .IsTrue();
        await Assert
            .That(html.QuerySelector("[data-slot=icon-picker-popover]")?.ClassList.Contains("w-96"))
            .IsTrue();
    }

    [Test]
    public async Task ProcessAsync_InlineWithItemClassNames_AddsClassToTiles()
    {
        // Arrange
        using var context = new RenderingContext();
        var sut = CreateSut(context);
        sut.Layout = IconPickerLayout.Inline;
        sut.ClassNames = new IconPickerClassNames { Item = "size-12" };

        // Act
        using var html = await TagHelperRenderer.RenderAsync(sut, Icons(context, "wifi"));

        // Assert
        await Assert
            .That(
                html.QuerySelector("[data-slot=toggle-group-item]")?.ClassList.Contains("size-12")
            )
            .IsTrue();
    }

    private static IconPickerTagHelper CreateSut(RenderingContext context) =>
        new(context.Generator, context.Icons) { ViewContext = context.ViewContext };

    private static Func<TagHelperContext, Task<string>> Icons(
        RenderingContext context,
        params string?[] names
    ) =>
        async parent =>
        {
            var html = string.Empty;
            foreach (var name in names)
            {
                html += await TagHelperRenderer.RenderChildAsync(
                    new IconTagHelper(context.Icons) { Name = name },
                    parent
                );
            }

            return html;
        };

    private sealed class Amenity
    {
        [Required]
        public string? Icon { get; set; }
    }
}
