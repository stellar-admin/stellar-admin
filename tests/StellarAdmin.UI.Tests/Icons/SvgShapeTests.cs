using StellarAdmin.UI.Icons;

namespace StellarAdmin.UI.Tests.Icons;

public class SvgShapeTests
{
    [Test]
    public async Task Constructor_WhenAttributesChangeAfterwards_KeepsOriginalAttributes()
    {
        // Arrange
        var attributes = new Dictionary<string, string> { ["d"] = "M12 9v4" };
        var sut = new SvgShape("path", attributes);

        // Act
        attributes["d"] = "M12 17h.01";

        // Assert
        await Assert.That(sut.Attributes["d"]).IsEqualTo("M12 9v4");
    }

    [Test]
    public async Task Attributes_WhenCastToDictionary_CannotBeModified()
    {
        // Arrange
        var sut = new SvgShape("path", new Dictionary<string, string> { ["d"] = "M12 9v4" });

        // Act
        var isReadOnly = ((ICollection<KeyValuePair<string, string>>)sut.Attributes).IsReadOnly;

        // Assert
        await Assert.That(isReadOnly).IsTrue();
    }
}
