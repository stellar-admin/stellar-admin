using StellarAdmin.Resources.Editors;

namespace StellarAdmin.Tests.Resources.Editors;

public class RichTextEditorTests
{
    [Test]
    [Arguments(0)]
    [Arguments(-1)]
    public async Task Rows_WhenLessThanOne_KeepsValue(int rows)
    {
        // Arrange
        var sut = new RichTextEditor();

        // Act
        sut.Rows = rows;

        // Assert
        await Assert.That(sut.Rows).IsNull();
    }

    [Test]
    public async Task StoreImage_WithDelegate_LetsImagesBeAdded()
    {
        // Arrange
        var sut = new RichTextEditor();

        // Act
        sut.StoreImage((_, _, _) => Task.FromResult("https://cdn.voyager.example/lake.png"));

        // Assert
        await Assert.That(sut.HasStore).IsTrue();
    }
}
