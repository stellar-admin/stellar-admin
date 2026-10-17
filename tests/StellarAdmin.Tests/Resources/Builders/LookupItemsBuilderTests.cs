using StellarAdmin.Resources.Builders;
using StellarAdmin.Resources.Editors;

namespace StellarAdmin.Tests.Resources.Builders;

public class LookupItemsBuilderTests
{
    [Test]
    public async Task UseMethods_ReturnTheSameBuilder()
    {
        // Arrange
        var sut = new LookupItemsBuilder<string>();

        // Act
        var result = sut.UseDescription(entity => entity).UseIcon(_ => "plane");

        // Assert
        await Assert.That(result).IsSameReferenceAs(sut);
        await Assert.That(sut.Description!("lisbon")).IsEqualTo("lisbon");
        await Assert.That(sut.MediaType).IsEqualTo(typeof(ItemMedia.Icon));
    }

    [Test]
    [Arguments("")]
    [Arguments(null)]
    public async Task UseCode_EmptyCode_LeavesOutMedia(string? code)
    {
        // Arrange
        var sut = new LookupItemsBuilder<string>();

        // Act
        sut.UseCode(_ => code);

        // Assert
        await Assert.That(sut.Media!("entity")).IsNull();
        await Assert.That(sut.MediaType).IsEqualTo(typeof(ItemMedia.Code));
    }

    [Test]
    public async Task UseAvatar_NullUrl_KeepsAvatarForInitials()
    {
        // Arrange
        var sut = new LookupItemsBuilder<string>();

        // Act
        sut.UseAvatar(_ => null);

        // Assert
        await Assert.That(sut.Media!("entity")).IsEqualTo(new ItemMedia.Avatar(null));
    }

    [Test]
    public async Task UseImage_AfterUseIcon_ReplacesMedia()
    {
        // Arrange
        var sut = new LookupItemsBuilder<string>();
        sut.UseIcon(_ => "plane");

        // Act
        sut.UseImage(entity => $"/images/{entity}.jpg");

        // Assert
        await Assert
            .That(sut.Media!("lisbon"))
            .IsEqualTo(new ItemMedia.Image("/images/lisbon.jpg"));
        await Assert.That(sut.MediaType).IsEqualTo(typeof(ItemMedia.Image));
    }
}
