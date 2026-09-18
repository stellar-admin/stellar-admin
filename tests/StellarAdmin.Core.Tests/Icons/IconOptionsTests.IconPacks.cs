using StellarAdmin.Icons;

namespace StellarAdmin.Core.Tests.Icons;

public partial class IconOptionsTests
{
    [Test]
    public async Task AddIconPack_WhenDefaultsWereCleared_RegistersOnlyReplacementIcons()
    {
        // Arrange
        var sut = new IconOptions();
        sut.ClearIcons();

        // Act
        sut.AddIconPack<ReplacementPack>();

        // Assert
        await Assert.That(sut.GetIconNames()).IsEquivalentTo(new[] { "test-dots" });
        await Assert.That(sut.TryGetIcon("TEST-DOTS", out var icon)).IsTrue();
        await Assert.That(icon).IsSameReferenceAs(ReplacementPack.Icon);
        await Assert.That(sut.TryGetIcon("ellipsis", out _)).IsFalse();
    }

    [Test]
    public async Task AddIconPack_WhenLaterMappingIsInvalid_PreservesExistingRegistrations()
    {
        // Arrange
        var sut = new IconOptions();
        var originalNames = sut.GetIconNames();
        sut.TryGetIcon("x", out var originalIcon);

        // Act
        Action act = () => sut.AddIconPack<PartiallyValidPack>();

        // Assert
        await Assert.That(act).Throws<ArgumentException>();
        await Assert.That(sut.GetIconNames()).IsEquivalentTo(originalNames);
        await Assert.That(sut.GetSemanticIconName(SemanticIconRole.Close)).IsEqualTo("x");
        await Assert
            .That(sut.GetSemanticIconName(SemanticIconRole.PaginationEllipsis))
            .IsEqualTo("ellipsis");
        await Assert.That(sut.TryGetIcon("x", out var icon)).IsTrue();
        await Assert.That(icon).IsSameReferenceAs(originalIcon);
    }

    [Test]
    public async Task AddIconPack_WhenMappingCasingDiffers_ImportsMapping()
    {
        // Arrange
        var sut = new IconOptions();

        // Act
        sut.AddIconPack<ReplacementPack>();

        // Assert
        await Assert
            .That(sut.GetSemanticIconName(SemanticIconRole.PaginationEllipsis))
            .IsEqualTo("TEST-DOTS");
        await Assert.That(sut.TryGetIcon("TEST-DOTS", out var icon)).IsTrue();
        await Assert.That(icon).IsSameReferenceAs(ReplacementPack.Icon);
    }

    [Test]
    public async Task AddIconPack_WhenMappingImportIsDisabled_DoesNotReadMappings()
    {
        // Arrange
        var sut = new IconOptions();

        // Act
        sut.AddIconPack<UnreadableMappingsPack>(pack => pack.ImportSemanticMappings = false);

        // Assert
        await Assert.That(sut.TryGetIcon("test-dots", out var icon)).IsTrue();
        await Assert.That(icon).IsSameReferenceAs(ReplacementPack.Icon);
    }

    [Test]
    public async Task AddIconPack_WhenMappingImportIsDisabled_DoesNotValidateMappings()
    {
        // Arrange
        var sut = new IconOptions();

        // Act
        sut.AddIconPack<InvalidPack>(pack => pack.ImportSemanticMappings = false);

        // Assert
        await Assert.That(sut.TryGetIcon("new-icon", out _)).IsTrue();
        await Assert.That(sut.GetSemanticIconName(SemanticIconRole.Close)).IsEqualTo("x");
    }

    [Test]
    public async Task AddIconPack_WhenMappingImportIsDisabled_PreservesMappings()
    {
        // Arrange
        var sut = new IconOptions();

        // Act
        sut.AddIconPack<ReplacementPack>(pack =>
        {
            pack.Prefix = "extra:";
            pack.ImportSemanticMappings = false;
        });

        // Assert
        await Assert.That(sut.TryGetIcon("extra:test-dots", out _)).IsTrue();
        await Assert
            .That(sut.GetSemanticIconName(SemanticIconRole.PaginationEllipsis))
            .IsEqualTo("ellipsis");
    }

    [Test]
    public async Task AddIconPack_WhenMappingTargetExistsOnlyInRegistry_RejectsPack()
    {
        // Arrange
        var sut = new IconOptions();
        sut.ClearIcons();
        sut.AddIcon("test-dots", ReplacementPack.Icon);

        // Act
        Action act = () => sut.AddIconPack<MappingOnlyPack>();

        // Assert
        await Assert.That(act).Throws<ArgumentException>();
        await Assert.That(sut.GetSemanticIconName(SemanticIconRole.Close)).IsNull();
        await Assert.That(sut.TryGetIcon("test-dots", out var icon)).IsTrue();
        await Assert.That(icon).IsSameReferenceAs(ReplacementPack.Icon);
    }

    [Test]
    [Arguments("")]
    [Arguments("bad:")]
    public async Task AddIconPack_WhenMappingTargetIsMissing_RejectsEntirePack(string prefix)
    {
        // Arrange
        var sut = new IconOptions();
        sut.ClearIcons();

        // Act
        Action act = () => sut.AddIconPack<InvalidPack>(pack => pack.Prefix = prefix);

        // Assert
        await Assert.That(act).Throws<ArgumentException>();
        await Assert.That(sut.GetIconNames()).IsEmpty();
        await Assert.That(sut.GetSemanticIconName(SemanticIconRole.Close)).IsNull();
    }

    [Test]
    public async Task AddIconPack_WhenNameAlreadyExists_ReplacesDefinition()
    {
        // Arrange
        var sut = new IconOptions();
        sut.AddIconPack<ReplacementPack>();
        sut.MapSemanticIcon(SemanticIconRole.BreadcrumbEllipsis, "test-dots");

        // Act
        sut.AddIconPack<OverridePack>();

        // Assert
        await Assert.That(sut.TryGetIcon("test-dots", out var icon)).IsTrue();
        await Assert.That(icon!.Attributes["data-test-icon"]).IsEqualTo("override");
        await Assert
            .That(sut.GetSemanticIconName(SemanticIconRole.BreadcrumbEllipsis))
            .IsEqualTo("test-dots");
    }

    [Test]
    public async Task AddIconPack_WhenOnlySomeRolesAreMapped_PreservesOtherMappings()
    {
        // Arrange
        var sut = new IconOptions();

        // Act
        sut.AddIconPack<ReplacementPack>();

        // Assert
        await Assert.That(sut.GetSemanticIconName(SemanticIconRole.Close)).IsEqualTo("x");
    }

    [Test]
    public async Task AddIconPack_WhenPackHasNoMappings_PreservesExistingMappings()
    {
        // Arrange
        var sut = new IconOptions();
        sut.AddIconPack<ReplacementPack>();

        // Act
        sut.AddIconPack<LegacyPack>();

        // Assert
        await Assert
            .That(sut.GetSemanticIconName(SemanticIconRole.PaginationEllipsis))
            .IsEqualTo("TEST-DOTS");
    }

    [Test]
    public async Task AddIconPack_WhenPrefixIsEmpty_LeavesNamesUnchanged()
    {
        // Arrange
        var sut = new IconOptions();

        // Act
        sut.AddIconPack<ReplacementPack>(pack =>
        {
            pack.Prefix = "";
            pack.ImportSemanticMappings = false;
        });

        // Assert
        await Assert.That(sut.TryGetIcon("test-dots", out _)).IsTrue();
        await Assert
            .That(sut.GetSemanticIconName(SemanticIconRole.PaginationEllipsis))
            .IsEqualTo("ellipsis");
    }

    [Test]
    public async Task AddIconPack_WhenPrefixIsSpecified_QualifiesNamesAndMappings()
    {
        // Arrange
        var sut = new IconOptions();

        // Act
        sut.AddIconPack<ReplacementPack>(pack => pack.Prefix = "my:");

        // Assert
        await Assert.That(sut.TryGetIcon("MY:test-dots", out var icon)).IsTrue();
        await Assert.That(icon).IsSameReferenceAs(ReplacementPack.Icon);
        await Assert.That(sut.TryGetIcon("test-dots", out _)).IsFalse();
        await Assert.That(sut.GetIconNames()).Contains("my:test-dots");
        await Assert
            .That(sut.GetSemanticIconName(SemanticIconRole.PaginationEllipsis))
            .IsEqualTo("my:TEST-DOTS");
    }

    [Test]
    public async Task AddIconPack_WhenPrefixesDiffer_KeepsBothDefinitions()
    {
        // Arrange
        var sut = new IconOptions();
        sut.AddIconPack<ReplacementPack>(pack => pack.Prefix = "my:");

        // Act
        sut.AddIconPack<OverridePack>(pack => pack.Prefix = "other-");

        // Assert
        await Assert.That(sut.TryGetIcon("my:test-dots", out var original)).IsTrue();
        await Assert.That(original).IsSameReferenceAs(ReplacementPack.Icon);
        await Assert.That(sut.TryGetIcon("other-test-dots", out var added)).IsTrue();
        await Assert.That(added!.Attributes["data-test-icon"]).IsEqualTo("override");
    }

    [Test]
    public async Task AddIconPack_WhenRoleWasExplicitlyMapped_ReplacesMapping()
    {
        // Arrange
        var sut = new IconOptions();
        sut.AddIcon("test-dots", ReplacementPack.Icon);
        sut.MapSemanticIcon(SemanticIconRole.Close, "test-dots");

        // Act
        sut.AddIconPack<LucideIconPack>();

        // Assert
        await Assert.That(sut.GetSemanticIconName(SemanticIconRole.Close)).IsEqualTo("x");
    }
}
