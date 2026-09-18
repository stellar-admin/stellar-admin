using StellarAdmin.Icons;

namespace StellarAdmin.Core.Tests.Icons;

public partial class IconOptionsTests
{
    public static IEnumerable<SemanticIconRole> SemanticRoles() =>
        Enum.GetValues<SemanticIconRole>();

    [Test]
    public async Task ClearIcons_WhenSemanticIconsAreMapped_RemovesAllMappings()
    {
        // Arrange
        var sut = new IconOptions();

        // Act
        sut.ClearIcons();

        // Assert
        await Assert
            .That(Enum.GetValues<SemanticIconRole>().Select(sut.GetSemanticIconName))
            .IsEquivalentTo(new string?[Enum.GetValues<SemanticIconRole>().Length]);
    }

    [Test]
    [MethodDataSource(nameof(SemanticRoles))]
    public async Task Constructor_ForEachSemanticRole_RegistersResolvableDefault(
        SemanticIconRole role
    )
    {
        // Arrange

        // Act
        var sut = new IconOptions();

        // Assert
        var name = sut.GetSemanticIconName(role);
        await Assert.That(name).IsNotNull();
        await Assert.That(sut.TryGetIcon(name!, out _)).IsTrue();
    }

    [Test]
    public async Task MapSemanticIcon_WhenAnotherInstanceExists_DoesNotChangeItsMapping()
    {
        // Arrange
        var sut = new IconOptions();
        var other = new IconOptions();

        // Act
        sut.MapSemanticIcon(SemanticIconRole.BreadcrumbEllipsis, "x");

        // Assert
        await Assert
            .That(other.GetSemanticIconName(SemanticIconRole.BreadcrumbEllipsis))
            .IsEqualTo("ellipsis");
        await Assert
            .That(sut.GetSemanticIconName(SemanticIconRole.BreadcrumbEllipsis))
            .IsEqualTo("x");
    }

    [Test]
    public async Task MapSemanticIcon_WhenIconIsRegistered_AssignsRole()
    {
        // Arrange
        var sut = new IconOptions();
        sut.AddIcon("test-dots", ReplacementPack.Icon);

        // Act
        sut.MapSemanticIcon(SemanticIconRole.Close, "TEST-DOTS");

        // Assert
        await Assert.That(sut.GetSemanticIconName(SemanticIconRole.Close)).IsEqualTo("TEST-DOTS");
    }

    [Test]
    public async Task MapSemanticIcon_WhenIconIsUnregistered_ThrowsArgumentException()
    {
        // Arrange
        var sut = new IconOptions();

        // Act
        Action act = () => sut.MapSemanticIcon(SemanticIconRole.Close, "unregistered");

        // Assert
        await Assert.That(act).Throws<ArgumentException>();
        await Assert.That(sut.GetSemanticIconName(SemanticIconRole.Close)).IsEqualTo("x");
    }

    [Test]
    [Arguments("")]
    [Arguments("my:")]
    public async Task RemoveIcon_WhenNameCasingDiffers_RemovesAllAssociatedMappings(string prefix)
    {
        // Arrange
        var sut = new IconOptions();
        sut.AddIconPack<ReplacementPack>(pack => pack.Prefix = prefix);
        sut.MapSemanticIcon(SemanticIconRole.Close, prefix + "test-dots");
        sut.MapSemanticIcon(SemanticIconRole.BreadcrumbEllipsis, prefix + "test-dots");

        // Act
        var removed = sut.RemoveIcon((prefix + "test-dots").ToUpperInvariant());

        // Assert
        await Assert.That(removed).IsTrue();
        await Assert.That(sut.GetSemanticIconName(SemanticIconRole.Close)).IsNull();
        await Assert.That(sut.GetSemanticIconName(SemanticIconRole.BreadcrumbEllipsis)).IsNull();
        await Assert.That(sut.GetSemanticIconName(SemanticIconRole.PaginationEllipsis)).IsNull();
        await Assert
            .That(sut.GetSemanticIconName(SemanticIconRole.AccordionIndicator))
            .IsEqualTo("chevron-down");
    }
}
