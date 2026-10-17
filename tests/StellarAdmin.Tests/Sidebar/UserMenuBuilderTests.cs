using Microsoft.Extensions.DependencyInjection;
using StellarAdmin.Sidebar;

namespace StellarAdmin.Tests.Sidebar;

public class UserMenuBuilderTests
{
    [Test]
    public async Task Add_WithNullItem_ThrowsArgumentNullException()
    {
        // Arrange
        var sut = new ServiceCollection().AddStellarAdmin();

        // Act
        Action act = () => sut.ConfigureUserMenu(menu => menu.Add(null!));

        // Assert
        await Assert.That(act).Throws<ArgumentNullException>();
    }

    [Test]
    [Arguments("")]
    [Arguments(" ")]
    public async Task AddAuthenticationScheme_WithBlankScheme_ThrowsArgumentException(string scheme)
    {
        // Arrange
        var sut = new ServiceCollection().AddStellarAdmin();

        // Act
        Action act = () =>
            sut.ConfigureUserMenu(menu =>
                menu.Logout(logout => logout.AddAuthenticationScheme(scheme))
            );

        // Assert
        await Assert.That(act).Throws<ArgumentException>();
    }

    [Test]
    [Arguments("")]
    [Arguments(" ")]
    public async Task LogoutPath_WithBlankPath_ThrowsArgumentException(string path)
    {
        // Arrange
        var sut = new ServiceCollection().AddStellarAdmin();

        // Act
        Action act = () => sut.ConfigureUserMenu(menu => menu.Logout(logout => logout.Path = path));

        // Assert
        await Assert.That(act).Throws<ArgumentException>();
    }
}
