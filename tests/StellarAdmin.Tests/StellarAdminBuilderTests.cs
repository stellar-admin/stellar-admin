using Microsoft.Extensions.DependencyInjection;
using StellarAdmin.Sidebar;

namespace StellarAdmin.Tests;

public class StellarAdminBuilderTests
{
    [Test]
    [Arguments("")]
    [Arguments(" ")]
    public async Task AddSidebarLink_WithBlankGroup_ThrowsArgumentException(string group)
    {
        // Arrange
        var sut = new ServiceCollection().AddStellarAdmin();

        // Act
        Action act = () =>
            sut.AddSidebarLink(
                "Reports",
                SidebarLinkTarget.Url("/reports"),
                link => link.Group = group
            );

        // Assert
        await Assert.That(act).Throws<ArgumentException>();
    }

    [Test]
    [Arguments("")]
    [Arguments(" ")]
    public async Task AddSidebarLink_WithBlankIcon_ThrowsArgumentException(string icon)
    {
        // Arrange
        var sut = new ServiceCollection().AddStellarAdmin();

        // Act
        Action act = () =>
            sut.AddSidebarLink(
                "Reports",
                SidebarLinkTarget.Url("/reports"),
                link => link.Icon = icon
            );

        // Assert
        await Assert.That(act).Throws<ArgumentException>();
    }

    [Test]
    [Arguments("")]
    [Arguments(" ")]
    public async Task AddSidebarLink_WithBlankLabel_ThrowsArgumentException(string label)
    {
        // Arrange
        var sut = new ServiceCollection().AddStellarAdmin();

        // Act
        Action act = () => sut.AddSidebarLink(label, SidebarLinkTarget.Url("/reports"));

        // Assert
        await Assert.That(act).Throws<ArgumentException>();
    }

    [Test]
    public async Task ConfigureBranding_WithWordmarkAndSubtitle_ThrowsInvalidOperationException()
    {
        // Arrange
        var sut = new ServiceCollection().AddStellarAdmin();

        // Act
        Action act = () =>
            sut.ConfigureBranding(branding =>
            {
                branding.Subtitle = "Admin Console";
                branding.Logo = BrandingLogo.Wordmark("~/img/wordmark.svg");
            });

        // Assert
        await Assert.That(act).Throws<InvalidOperationException>();
    }
}
