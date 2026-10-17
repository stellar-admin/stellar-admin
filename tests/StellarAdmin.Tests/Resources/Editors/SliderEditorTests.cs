using StellarAdmin.Resources.Editors;

namespace StellarAdmin.Tests.Resources.Editors;

public class SliderEditorTests
{
    [Test]
    public async Task AddMark_WithLabel_AddsLabelledMark()
    {
        // Arrange
        var sut = new SliderEditor();

        // Act
        sut.AddMark(5, "Mid");

        // Assert
        await Assert.That(sut.Marks).IsEquivalentTo([new SliderEditorMark(5, "Mid")]);
    }

    [Test]
    public async Task AddMark_WithNullLabel_ThrowsArgumentNullException()
    {
        // Arrange
        var sut = new SliderEditor();

        // Act
        Action act = () => sut.AddMark(5, null!);

        // Assert
        await Assert.That(act).Throws<ArgumentNullException>();
    }

    [Test]
    public async Task AddMark_WithoutLabel_AddsUnlabelledMark()
    {
        // Arrange
        var sut = new SliderEditor();

        // Act
        sut.AddMark(5);

        // Assert
        await Assert.That(sut.Marks).IsEquivalentTo([new SliderEditorMark(5, null)]);
    }
}
