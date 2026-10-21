using StellarAdmin.Resources.Editors;

namespace StellarAdmin.Tests.Resources.Editors;

public class RichTextEditorTests
{
    [Test]
    [Arguments(0)]
    [Arguments(-1)]
    public async Task Rows_WhenLessThanOne_ThrowsArgumentOutOfRangeException(int rows)
    {
        // Arrange
        var sut = new RichTextEditor();

        // Act
        var act = () => sut.Rows = rows;

        // Assert
        await Assert.That(act).Throws<ArgumentOutOfRangeException>();
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
