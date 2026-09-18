using StellarAdmin.Icons;

namespace StellarAdmin.Core.Tests.Icons;

public partial class IconOptionsTests
{
    [Test]
    [Arguments("CHECK")]
    [Arguments("duplicate-custom")]
    public async Task AddIcon_WhenNameAlreadyExists_ThrowsArgumentException(string name)
    {
        // Arrange
        var sut = new IconOptions();
        var icon = new IconDefinition(new Dictionary<string, string>(), []);
        if (name == "duplicate-custom")
        {
            sut.AddIcon(name, icon);
        }

        // Act
        Action act = () => sut.AddIcon(name, icon);

        // Assert
        await Assert.That(act).Throws<ArgumentException>();
    }

    [Test]
    public async Task Constructor_WhenCreated_RegistersLucideShapes()
    {
        // Arrange

        // Act
        var sut = new IconOptions();

        // Assert
        await Assert.That(sut.TryGetIcon("check", out var icon)).IsTrue();
        await Assert.That(icon!.Shapes.Count).IsGreaterThan(0);
    }

    [Test]
    public async Task GetIconNames_WhenCustomIconIsRegistered_IncludesName()
    {
        // Arrange
        var sut = new IconOptions();
        sut.AddIcon("custom", new IconDefinition(new Dictionary<string, string>(), []));

        // Act
        var names = sut.GetIconNames();

        // Assert
        await Assert.That(names).Contains("custom");
    }

    [Test]
    public async Task TryGetIcon_WhenNameCasingDiffers_ReturnsRegisteredIcon()
    {
        // Arrange
        var sut = new IconOptions();
        var expected = new IconDefinition(new Dictionary<string, string>(), []);
        sut.AddIcon("custom", expected);

        // Act
        var found = sut.TryGetIcon("CUSTOM", out var icon);

        // Assert
        await Assert.That(found).IsTrue();
        await Assert.That(icon).IsSameReferenceAs(expected);
    }
}
