using StellarAdmin.Sidebar;

namespace StellarAdmin.Tests.Sidebar;

public class SidebarLinkTargetTests
{
    [Test]
    [Arguments("", "Reports")]
    [Arguments(" ", "Reports")]
    [Arguments("Index", "")]
    [Arguments("Index", " ")]
    public async Task Action_WithBlankActionOrController_RecordsProblem(
        string action,
        string controller
    )
    {
        // Arrange

        // Act
        var result = SidebarLinkTarget.Action(action, controller);

        // Assert
        await Assert.That(result.Problem).IsNotNull();
    }

    [Test]
    [Arguments("")]
    [Arguments(" ")]
    public async Task Action_WithBlankArea_RecordsProblem(string area)
    {
        // Arrange

        // Act
        var result = SidebarLinkTarget.Action("Index", "Reports", area);

        // Assert
        await Assert.That(result.Problem).IsNotNull();
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
    public async Task Page_WithBlankArea_RecordsProblem(string area)
    {
        // Arrange

        // Act
        var result = SidebarLinkTarget.Page("/Reports/Index", area);

        // Assert
        await Assert.That(result.Problem).IsNotNull();
    }

    [Test]
    [Arguments("")]
    [Arguments(" ")]
    public async Task Page_WithBlankPage_RecordsProblem(string page)
    {
        // Arrange

        // Act
        var result = SidebarLinkTarget.Page(page);

        // Assert
        await Assert.That(result.Problem).IsNotNull();
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
    public async Task Url_WithBlankUrl_RecordsProblem(string url)
    {
        // Arrange

        // Act
        var result = SidebarLinkTarget.Url(url);

        // Assert
        await Assert.That(result.Problem).IsNotNull();
    }
}
