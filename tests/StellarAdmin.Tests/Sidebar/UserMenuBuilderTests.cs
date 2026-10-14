using Microsoft.Extensions.DependencyInjection;
using StellarAdmin.Sidebar;

namespace StellarAdmin.Tests.Sidebar;

public class UserMenuBuilderTests
{
    [Test]
    public async Task Items_ByDefault_ContainsLogoutWithIcon()
    {
        // Arrange
        IList<UserMenuItem> items = [];

        // Act
        new ServiceCollection()
            .AddStellarAdmin()
            .ConfigureUserMenu(menu => items = menu.Items);

        // Assert
        await Assert.That(items).HasSingleItem();
        await Assert.That(items[0]).IsTypeOf<LogoutUserMenuItem>();
        await Assert.That(((LogoutUserMenuItem)items[0]).Icon).IsEqualTo("log-out");
        await Assert.That(((LogoutUserMenuItem)items[0]).Path).IsNull();
    }

    [Test]
    public async Task Items_WhenAssignedListContainsNull_ThrowsArgumentException()
    {
        // Arrange
        var sut = new ServiceCollection().AddStellarAdmin();

        // Act
        Action act = () =>
            sut.ConfigureUserMenu(menu => menu.Items = [UserMenuItem.Separator(), null!]);

        // Assert
        await Assert.That(act).Throws<ArgumentException>();
    }
}
