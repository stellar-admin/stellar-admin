using StellarAdmin.Sidebar;

namespace StellarAdmin.Tests.Sidebar;

public class SidebarLinkTargetTests
{
    [Test]
    [Arguments("", "Reports")]
    [Arguments(" ", "Reports")]
    [Arguments("Index", "")]
    [Arguments("Index", " ")]
    public async Task Action_WithBlankActionOrController_ThrowsArgumentException(
        string action,
        string controller
    )
    {
        // Arrange

        // Act
        Action act = () => SidebarLinkTarget.Action(action, controller);

        // Assert
        await Assert.That(act).Throws<ArgumentException>();
    }

    [Test]
    [Arguments("")]
    [Arguments(" ")]
    public async Task Action_WithBlankArea_ThrowsArgumentException(string area)
    {
        // Arrange

        // Act
        Action act = () => SidebarLinkTarget.Action("Index", "Reports", area);

        // Assert
        await Assert.That(act).Throws<ArgumentException>();
    }

    [Test]
    public async Task Action_WithNullArea_ThrowsArgumentNullException()
    {
        // Arrange

        // Act
        Action act = () => SidebarLinkTarget.Action("Index", "Reports", null!);

        // Assert
        await Assert.That(act).Throws<ArgumentNullException>();
    }

    [Test]
    [Arguments("")]
    [Arguments(" ")]
    public async Task Page_WithBlankArea_ThrowsArgumentException(string area)
    {
        // Arrange

        // Act
        Action act = () => SidebarLinkTarget.Page("/Reports/Index", area);

        // Assert
        await Assert.That(act).Throws<ArgumentException>();
    }

    [Test]
    [Arguments("")]
    [Arguments(" ")]
    public async Task Page_WithBlankPage_ThrowsArgumentException(string page)
    {
        // Arrange

        // Act
        Action act = () => SidebarLinkTarget.Page(page);

        // Assert
        await Assert.That(act).Throws<ArgumentException>();
    }

    [Test]
    public async Task Page_WithNullArea_ThrowsArgumentNullException()
    {
        // Arrange

        // Act
        Action act = () => SidebarLinkTarget.Page("/Reports/Index", null!);

        // Assert
        await Assert.That(act).Throws<ArgumentNullException>();
    }

    [Test]
    [Arguments("")]
    [Arguments(" ")]
    public async Task Url_WithBlankUrl_ThrowsArgumentException(string url)
    {
        // Arrange

        // Act
        Action act = () => SidebarLinkTarget.Url(url);

        // Assert
        await Assert.That(act).Throws<ArgumentException>();
    }
}
