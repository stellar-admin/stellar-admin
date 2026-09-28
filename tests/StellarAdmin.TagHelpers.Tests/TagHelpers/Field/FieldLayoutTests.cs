namespace StellarAdmin.TagHelpers.Tests.TagHelpers.Field;

public class FieldLayoutTests
{
    [Test]
    public async Task Constructor_WhenPartIsRepeatedInOneZone_ThrowsArgumentException()
    {
        // Arrange
        FieldPart[] beforeControl = [FieldPart.Label, FieldPart.Label];

        // Act
        Action act = () => _ = new FieldLayout(FieldOrientation.Vertical, beforeControl, []);

        // Assert
        await Assert.That(act).Throws<ArgumentException>();
    }

    [Test]
    public async Task Constructor_WhenPartIsListedInBothZones_ThrowsArgumentException()
    {
        // Arrange
        FieldPart[] beforeControl = [FieldPart.Label, FieldPart.Error];
        FieldPart[] afterControl = [FieldPart.Error];

        // Act
        Action act = () =>
            _ = new FieldLayout(FieldOrientation.Vertical, beforeControl, afterControl);

        // Assert
        await Assert.That(act).Throws<ArgumentException>();
    }

    [Test]
    public async Task Constructor_WhenSourceListChangesAfterward_KeepsOriginalParts()
    {
        // Arrange
        List<FieldPart> afterControl = [FieldPart.Label];

        // Act
        var sut = new FieldLayout(FieldOrientation.Horizontal, [], afterControl);
        afterControl.Add(FieldPart.Error);

        // Assert
        await Assert.That(sut.AfterControl).IsEquivalentTo([FieldPart.Label]);
    }

    [Test]
    public async Task Without_WhenPartIsListed_RemovesItAndKeepsRemainingArrangement()
    {
        // Arrange
        var sut = new FieldLayout(
            FieldOrientation.Horizontal,
            [FieldPart.Label, FieldPart.Error],
            [FieldPart.Description]
        );

        // Act
        var result = sut.Without(FieldPart.Error);

        // Assert
        await Assert.That(result.Orientation).IsEqualTo(FieldOrientation.Horizontal);
        await Assert.That(result.BeforeControl).IsEquivalentTo([FieldPart.Label]);
        await Assert.That(result.AfterControl).IsEquivalentTo([FieldPart.Description]);
    }

    [Test]
    public async Task Without_WhenCalledOnPreset_LeavesPresetUnchanged()
    {
        // Arrange
        var sut = FieldLayout.ControlFirst;

        // Act
        _ = sut.Without(FieldPart.Error);

        // Assert
        await Assert
            .That(FieldLayout.ControlFirst.AfterControl)
            .IsEquivalentTo([FieldPart.Label, FieldPart.Description, FieldPart.Error]);
    }
}
