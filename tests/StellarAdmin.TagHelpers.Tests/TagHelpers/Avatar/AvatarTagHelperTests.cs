using StellarAdmin.TagHelpers.Tests.Support;

namespace StellarAdmin.TagHelpers.Tests.TagHelpers.Avatar;

public class AvatarTagHelperTests
{
    [Test]
    [Arguments("Amsterdam", "AM")]
    [Arguments("Hong Kong", "HK")]
    [Arguments("los angeles international", "LA")]
    [Arguments("  Jane   Doe ", "JD")]
    [Arguments("X", "X")]
    [Arguments("Bath & Personal Care", "BP")]
    [Arguments("& Co", "CO")]
    public async Task ProcessAsync_WithNameAndNoSource_RendersInitials(string name, string expected)
    {
        // Arrange
        var sut = new AvatarTagHelper { Name = name };

        // Act
        using var html = await TagHelperRenderer.RenderAsync(sut);

        // Assert
        await Assert
            .That(html.QuerySelector("[data-slot=avatar-fallback]")?.TextContent)
            .IsEqualTo(expected);
    }

    [Test]
    public async Task ProcessAsync_WithInitials_RendersThemOverTheName()
    {
        // Arrange
        var sut = new AvatarTagHelper { Name = "Hong Kong", Initials = "hk" };

        // Act
        using var html = await TagHelperRenderer.RenderAsync(sut);

        // Assert
        await Assert
            .That(html.QuerySelector("[data-slot=avatar-fallback]")?.TextContent)
            .IsEqualTo("hk");
    }

    [Test]
    [Arguments("A<b", null)]
    [Arguments(null, "<b>")]
    public async Task ProcessAsync_WithMarkupInNameOrInitials_EncodesIt(
        string? name,
        string? initials
    )
    {
        // Arrange
        var sut = new AvatarTagHelper { Name = name, Initials = initials };

        // Act
        using var html = await TagHelperRenderer.RenderAsync(sut);

        // Assert
        var fallback = html.QuerySelector("[data-slot=avatar-fallback]")!;
        await Assert.That(fallback.Children.Length).IsEqualTo(0);
        await Assert.That(fallback.TextContent).IsEqualTo(initials ?? "A<");
    }
}
