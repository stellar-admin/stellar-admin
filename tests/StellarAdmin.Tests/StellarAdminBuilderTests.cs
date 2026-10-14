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
    public async Task AddSidebarLink_WithBlankLabel_ThrowsArgumentException(string label)
    {
        // Arrange
        var sut = new ServiceCollection().AddStellarAdmin();

        // Act
        Action act = () => sut.AddSidebarLink(label, SidebarLinkTarget.Url("/reports"));

        // Assert
        await Assert.That(act).Throws<ArgumentException>();
    }
}
