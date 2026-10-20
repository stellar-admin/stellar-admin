using StellarAdmin.UI.Tests.Support;

namespace StellarAdmin.UI.Tests.TagHelpers.Checklist;

public class ChecklistItemStatusTagHelperTests
{
    [Test]
    [Arguments(null)]
    [Arguments(" ")]
    public async Task ProcessAsync_WhenLabelIsMissing_ThrowsInvalidOperationException(string? label)
    {
        // Arrange
        var sut = new ChecklistItemStatusTagHelper { Label = label };

        // Act
        Func<Task> act = () => TagHelperRenderer.RenderAsync(sut);

        // Assert
        await Assert.That(act).Throws<InvalidOperationException>();
    }
}
