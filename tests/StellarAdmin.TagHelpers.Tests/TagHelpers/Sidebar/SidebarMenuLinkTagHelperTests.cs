using Microsoft.AspNetCore.Http;
using StellarAdmin.TagHelpers.Tests.Support;

namespace StellarAdmin.TagHelpers.Tests.TagHelpers.Sidebar;

public class SidebarMenuLinkTagHelperTests
{
    [Test]
    [Arguments("", null, true)]
    [Arguments("", "StellarAdmin", false)]
    [Arguments(null, null, true)]
    [Arguments(null, "StellarAdmin", false)]
    [Arguments("StellarAdmin", "StellarAdmin", true)]
    public async Task ProcessAsync_WithArea_MarksActiveWhenCurrentAreaMatches(
        string? area,
        string? currentArea,
        bool expectedActive
    )
    {
        // Arrange
        using var context = new RenderingContext();
        context.ViewContext.HttpContext.SetEndpoint(new Endpoint(null, null, "Test"));
        context.ViewContext.RouteData.Values["area"] = currentArea;
        context.ViewContext.RouteData.Values["controller"] = "Invoices";
        context.ViewContext.RouteData.Values["action"] = "Index";
        var sut = new SidebarMenuLinkTagHelper(context.Generator)
        {
            ViewContext = context.ViewContext,
            Action = "Index",
            Area = area,
            Controller = "Invoices",
        };

        // Act
        using var html = await TagHelperRenderer.RenderAsync(sut);

        // Assert
        var link = html.QuerySelector("[data-slot=sidebar-menu-button]");
        await Assert.That(link?.HasAttribute("data-active")).IsEqualTo(expectedActive);
    }
}
