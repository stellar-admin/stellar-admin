using StellarAdmin.UI.Icons;

namespace StellarAdmin.UI.Tests.Icons;

public class IconDefinitionTests
{
    [Test]
    public async Task Constructor_WhenInputsChangeAfterwards_KeepsOriginalAttributesAndShapes()
    {
        // Arrange
        var attributes = new Dictionary<string, string> { ["viewBox"] = "0 0 24 24" };
        var shapes = new List<SvgShape>
        {
            new("path", new Dictionary<string, string> { ["d"] = "M12 9v4" }),
        };
        var sut = new IconDefinition(attributes, shapes);

        // Act
        attributes["viewBox"] = "0 0 16 16";
        shapes.Clear();

        // Assert
        await Assert.That(sut.Attributes["viewBox"]).IsEqualTo("0 0 24 24");
        await Assert.That(sut.Shapes.Count).IsEqualTo(1);
    }

    [Test]
    public async Task Shapes_WhenCastToList_CannotBeModified()
    {
        // Arrange
        var sut = new IconDefinition(
            new Dictionary<string, string>(),
            [new SvgShape("path", new Dictionary<string, string> { ["d"] = "M12 9v4" })]
        );

        // Act
        var isReadOnly = ((ICollection<SvgShape>)sut.Shapes).IsReadOnly;

        // Assert
        await Assert.That(isReadOnly).IsTrue();
    }
}
