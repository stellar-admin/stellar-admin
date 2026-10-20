using Microsoft.AspNetCore.Razor.TagHelpers;
using Microsoft.Extensions.Options;
using StellarAdmin.UI.Icons;
using StellarAdmin.UI.Tests.Support;

namespace StellarAdmin.UI.Tests.TagHelpers.Checklist;

public class ChecklistItemTagHelperTests
{
    [Test]
    public async Task ProcessAsync_WhenStatusIsOmitted_RendersTodoStatus()
    {
        // Arrange
        var sut = CreateSut();
        sut.Title = "Buy travel insurance";

        // Act
        using var html = await TagHelperRenderer.RenderAsync(sut);

        // Assert
        var item = html.QuerySelector("li");
        await Assert.That(item?.GetAttribute("data-slot")).IsEqualTo("checklist-item");
        await Assert.That(item?.GetAttribute("data-status")).IsEqualTo("todo");
        await Assert.That(item?.ClassName).IsEqualTo("sa-checklist-item existing-class");
        await Assert.That(html.QuerySelector(".sa-checklist-item-status > svg")).IsNotNull();
        await Assert
            .That(html.QuerySelector(".sa-checklist-item-status > .sr-only")?.TextContent)
            .IsEqualTo("To do: ");
    }

    [Test]
    [Arguments(ChecklistStatus.InProgress, "in-progress", "In progress: ")]
    [Arguments(ChecklistStatus.Done, "done", "Done: ")]
    public async Task ProcessAsync_WhenStatusIsSet_RendersItsAttributeAndText(
        ChecklistStatus status,
        string expectedAttribute,
        string expectedText
    )
    {
        // Arrange
        var sut = CreateSut();
        sut.Status = status;

        // Act
        using var html = await TagHelperRenderer.RenderAsync(sut);

        // Assert
        await Assert
            .That(html.QuerySelector("li")?.GetAttribute("data-status"))
            .IsEqualTo(expectedAttribute);
        await Assert.That(html.QuerySelector(".sr-only")?.TextContent).IsEqualTo(expectedText);
    }

    [Test]
    public async Task ProcessAsync_WhenTitleAndDescriptionAreSet_RendersThemBeforeChildren()
    {
        // Arrange
        var sut = CreateSut();
        sut.Title = "Apply for visa";
        sut.Description = "Usually five working days.";

        // Act
        using var html = await TagHelperRenderer.RenderAsync(
            sut,
            _ => Task.FromResult("<div id=\"child\"></div>")
        );

        // Assert
        var slots = string.Join(
            " ",
            html.QuerySelectorAll("li > *")
                .Select(element => element.GetAttribute("data-slot") ?? element.Id)
        );
        await Assert
            .That(slots)
            .IsEqualTo(
                "checklist-item-status checklist-item-title checklist-item-description child"
            );
        await Assert
            .That(html.QuerySelector("p.sa-checklist-item-description")?.TextContent)
            .IsEqualTo("Usually five working days.");
    }

    [Test]
    public async Task ProcessAsync_WhenStatusIsAChildPart_RendersNoBuiltInStatus()
    {
        // Arrange
        var sut = CreateSut();

        // Act
        using var html = await TagHelperRenderer.RenderAsync(
            sut,
            parent =>
                TagHelperRenderer.RenderChildAsync(
                    new ChecklistItemStatusTagHelper { Label = "Failed" },
                    parent,
                    _ => Task.FromResult("<span id=\"icon\"></span>")
                )
        );

        // Assert
        await Assert.That(html.QuerySelector("li")?.HasAttribute("data-status")).IsFalse();
        await Assert.That(html.QuerySelectorAll(".sa-checklist-item-status").Length).IsEqualTo(1);
        await Assert.That(html.QuerySelector(".sa-checklist-item-status > #icon")).IsNotNull();
        await Assert.That(html.QuerySelector(".sr-only")?.TextContent).IsEqualTo("Failed: ");
    }

    [Test]
    [Arguments("status")]
    [Arguments("title")]
    [Arguments("description")]
    public async Task ProcessAsync_WhenAttributeAndItsChildPartAreBothSet_ThrowsInvalidOperationException(
        string part
    )
    {
        // Arrange
        var sut = CreateSut();
        TagHelper child;
        switch (part)
        {
            case "status":
                sut.Status = ChecklistStatus.Done;
                child = new ChecklistItemStatusTagHelper { Label = "Failed" };
                break;
            case "title":
                sut.Title = "Apply for visa";
                child = new ChecklistItemTitleTagHelper();
                break;
            default:
                sut.Description = "Usually five working days.";
                child = new ChecklistItemDescriptionTagHelper();
                break;
        }

        // Act
        Func<Task> act = () =>
            TagHelperRenderer.RenderAsync(
                sut,
                parent => TagHelperRenderer.RenderChildAsync(child, parent)
            );

        // Assert
        await Assert.That(act).Throws<InvalidOperationException>();
    }

    private static ChecklistItemTagHelper CreateSut()
    {
        return new ChecklistItemTagHelper(Options.Create(new IconOptions()));
    }
}
