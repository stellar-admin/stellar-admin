using StellarAdmin.Sidebar;

namespace StellarAdmin.Tests.Sidebar;

public class UserMenuItemTests
{
    [Test]
    public async Task Logout_WhenConfiguredWithoutIcon_RemovesDefaultIcon()
    {
        // Arrange

        // Act
        var item = UserMenuItem.Logout(item => item.Icon = null);

        // Assert
        await Assert.That(item.Icon).IsNull();
    }

    [Test]
    public async Task Page_WhenConfiguredWithIcon_SetsIcon()
    {
        // Arrange

        // Act
        var item = UserMenuItem.Page(
            "Account",
            "/Account/Manage/Index",
            "Identity",
            item => item.Icon = "badge-check"
        );

        // Assert
        await Assert.That(item.Icon).IsEqualTo("badge-check");
        await Assert.That(item.Area).IsEqualTo("Identity");
    }
}
