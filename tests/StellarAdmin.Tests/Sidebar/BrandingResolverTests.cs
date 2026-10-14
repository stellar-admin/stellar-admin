using StellarAdmin.Sidebar;

namespace StellarAdmin.Tests.Sidebar;

public class BrandingResolverTests
{
    [Test]
    [Arguments("Voyager.Admin", "Voyager Admin")]
    [Arguments("AdminPlayground", "Admin Playground")]
    [Arguments("voyager-admin_portal", "Voyager Admin Portal")]
    [Arguments("Voyager.API", "Voyager API")]
    public async Task TitleFromApplicationName_SplitsWordsAndCapitalizes(
        string applicationName,
        string expectedTitle
    )
    {
        // Arrange

        // Act
        var title = BrandingResolver.TitleFromApplicationName(applicationName);

        // Assert
        await Assert.That(title).IsEqualTo(expectedTitle);
    }

    [Test]
    [Arguments(null)]
    [Arguments("")]
    [Arguments(".-_")]
    public async Task TitleFromApplicationName_WithoutWords_ReturnsAdmin(string? applicationName)
    {
        // Arrange

        // Act
        var title = BrandingResolver.TitleFromApplicationName(applicationName);

        // Assert
        await Assert.That(title).IsEqualTo("Admin");
    }
}
