using StellarAdmin.UI.Tests.Support;

namespace StellarAdmin.UI.Tests.TagHelpers.Checklist;

public class ChecklistItemTitleTagHelperTests
{
    [Test]
    [Arguments(null, false)]
    [Arguments(false, false)]
    [Arguments(true, true)]
    public async Task ProcessAsync_WhenMutedIsSet_RendersMutedAttributeOnlyWhenTrue(
        bool? muted,
        bool expected
    )
    {
        // Arrange
        var sut = new ChecklistItemTitleTagHelper { Muted = muted };

        // Act
        using var html = await TagHelperRenderer.RenderAsync(sut);

        // Assert
        await Assert
            .That(
                html.QuerySelector("[data-slot=checklist-item-title]")?.HasAttribute("data-muted")
            )
            .IsEqualTo(expected);
    }
}
