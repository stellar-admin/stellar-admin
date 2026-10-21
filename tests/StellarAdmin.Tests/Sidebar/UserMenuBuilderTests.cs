using Microsoft.Extensions.DependencyInjection;
using StellarAdmin.Diagnostics;
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
    public async Task AddAuthenticationScheme_WithBlankScheme_LeavesOutScheme(string scheme)
    {
        // Arrange
        var sut = new ServiceCollection().AddStellarAdmin();

        // Act
        sut.ConfigureUserMenu(menu =>
            menu.Logout(logout => logout.AddAuthenticationScheme(scheme))
        );

        // Assert
        await Assert.That(sut.Options.UserMenu.Logout.AuthenticationSchemes).IsEmpty();
        await Assert
            .That(sut.Options.Problems.Single().Effect)
            .IsEqualTo(ConfigurationProblemEffect.Ignored);
    }

    [Test]
    public async Task Add_WithBlankItemText_LeavesOutItem()
    {
        // Arrange
        var sut = new ServiceCollection().AddStellarAdmin();

        // Act
        sut.ConfigureUserMenu(menu => menu.Add(UserMenuItem.Url(" ", "/profile")));

        // Assert
        await Assert.That(sut.Options.UserMenu.Items).IsEmpty();
        await Assert
            .That(sut.Options.Problems.Single().Kind)
            .IsEqualTo(ConfigurationProblemKind.InvalidLink);
    }

    [Test]
    [Arguments("")]
    [Arguments(" ")]
    public async Task LogoutPath_WithBlankPath_UsesDefaultPath(string path)
    {
        // Arrange
        var sut = new ServiceCollection().AddStellarAdmin();

        // Act
        sut.ConfigureUserMenu(menu => menu.Logout(logout => logout.Path = path));

        // Assert
        await Assert.That(sut.Options.UserMenu.Logout.Path).IsNull();
        await Assert
            .That(sut.Options.Problems.Single().Effect)
            .IsEqualTo(ConfigurationProblemEffect.Ignored);
    }
}
