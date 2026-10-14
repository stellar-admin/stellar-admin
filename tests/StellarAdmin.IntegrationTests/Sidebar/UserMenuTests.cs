using System.Security.Claims;
using Microsoft.AspNetCore.TestHost;
using StellarAdmin.IntegrationTests.Infrastructure;
using StellarAdmin.Sidebar;
using TUnit.Assertions.Enums;

namespace StellarAdmin.IntegrationTests.Sidebar;

public class UserMenuTests
{
    private const string Footer = "[data-slot='sidebar-footer']";

    private const string MenuEntries =
        "#sa-user-menu [data-slot='dropdown-menu-item'], #sa-user-menu [data-slot='dropdown-menu-separator']";

    [Test]
    public async Task AnonymousRequest_ShowsNoUserMenu()
    {
        // Arrange
        await using var sut = await StellarAdminTestHost.CreateAsync(
            new([]),
            configureServices: TestAuthenticationHandler.Register
        );
        using var client = sut.GetTestClient();

        // Act
        var document = await client.GetDocumentAsync("/stellaradmin");

        // Assert
        await Assert.That(document.QuerySelector(Footer)).IsNull();
    }

    [Test]
    public async Task SignedInUser_ShowsNameInitialsAndLogOut()
    {
        // Arrange
        await using var sut = await StellarAdminTestHost.CreateAsync(
            new([]),
            configureServices: TestAuthenticationHandler.Register
        );
        using var client = sut.GetTestClient();
        TestAuthenticationHandler.SignIn(client, "Ada Lovelace");

        // Act
        var document = await client.GetDocumentAsync("/stellaradmin");

        // Assert
        var footer = document.RequiredElement(Footer);
        await Assert
            .That(
                footer
                    .RequiredElement("[data-slot='sidebar-menu-button'] .font-semibold")
                    .TextContent
            )
            .IsEqualTo("Ada Lovelace");
        await Assert
            .That(
                footer
                    .RequiredElement(
                        "[data-slot='sidebar-menu-button'] [data-slot='avatar-fallback']"
                    )
                    .TextContent
            )
            .IsEqualTo("AL");
        await Assert
            .That(document.TextContents(MenuEntries))
            .IsEquivalentTo(["", "Log out"], CollectionOrdering.Matching);

        var logout = footer.RequiredElement("form [data-user-menu-logout]");
        await Assert.That(logout.GetAttribute("data-variant")).IsEqualTo("destructive");
        await Assert
            .That(logout.Closest("form")!.GetAttribute("action"))
            .IsEqualTo("/stellaradmin/logout");
        await Assert
            .That(logout.Closest("form")!.QuerySelector("input[name='__RequestVerificationToken']"))
            .IsNotNull();
    }

    [Test]
    public async Task ConfiguredClaims_ShowDescriptionAndPicture()
    {
        // Arrange
        await using var sut = await StellarAdminTestHost.CreateAsync(
            new([]),
            configureAdmin: admin =>
                admin.ConfigureUserMenu(menu =>
                {
                    menu.NameClaimType = "nickname";
                    menu.DescriptionClaimType = "department";
                    menu.AvatarClaimType = "photo";
                }),
            configureServices: TestAuthenticationHandler.Register
        );
        using var client = sut.GetTestClient();
        TestAuthenticationHandler.SignIn(
            client,
            "ada",
            new Dictionary<string, string>
            {
                ["nickname"] = "Countess",
                ["department"] = "Analytics",
                ["photo"] = "https://example.com/ada.png",
            }
        );

        // Act
        var document = await client.GetDocumentAsync("/stellaradmin");

        // Assert
        var button = document.RequiredElement($"{Footer} [data-slot='sidebar-menu-button']");
        await Assert
            .That(button.TextContents(".grid > span"))
            .IsEquivalentTo(["Countess", "Analytics"], CollectionOrdering.Matching);
        await Assert
            .That(button.RequiredElement("[data-slot='avatar-image']").GetAttribute("src"))
            .IsEqualTo("https://example.com/ada.png");
    }

    [Test]
    public async Task DefaultClaims_ShowEmailUnderName()
    {
        // Arrange
        await using var sut = await StellarAdminTestHost.CreateAsync(
            new([]),
            configureServices: TestAuthenticationHandler.Register
        );
        using var client = sut.GetTestClient();
        TestAuthenticationHandler.SignIn(
            client,
            "Ada Lovelace",
            new Dictionary<string, string> { [ClaimTypes.Email] = "ada@voyager.travel" }
        );

        // Act
        var document = await client.GetDocumentAsync("/stellaradmin");

        // Assert
        await Assert
            .That(document.TextContents($"{Footer} [data-slot='sidebar-menu-button'] .grid > span"))
            .IsEquivalentTo(["Ada Lovelace", "ada@voyager.travel"], CollectionOrdering.Matching);
    }

    [Test]
    public async Task Items_RenderInListOrder()
    {
        // Arrange
        await using var sut = await StellarAdminTestHost.CreateAsync(
            new([]),
            configureAdmin: admin =>
                admin.ConfigureUserMenu(menu =>
                {
                    menu.Items.Insert(
                        0,
                        UserMenuItem.Url("Help", "~/help", item => item.Icon = "life-buoy")
                    );
                    menu.Items.Insert(1, UserMenuItem.Separator());
                    menu.Items.Add(UserMenuItem.Separator());
                    menu.Items.Add(UserMenuItem.Url("Status", "https://status.example.com"));
                }),
            configureServices: TestAuthenticationHandler.Register
        );
        using var client = sut.GetTestClient();
        TestAuthenticationHandler.SignIn(client, "ada");

        // Act
        var document = await client.GetDocumentAsync("/stellaradmin");

        // Assert
        await Assert
            .That(document.TextContents(MenuEntries))
            .IsEquivalentTo(["", "Help", "", "Log out", "", "Status"], CollectionOrdering.Matching);
        var help = document.RequiredElement("#sa-user-menu a[data-slot='dropdown-menu-item']");
        await Assert.That(help.GetAttribute("href")).IsEqualTo("/help");
        await Assert.That(help.QuerySelector("svg")).IsNotNull();
    }

    [Test]
    public async Task ReplacedItemsWithHostLogout_PostToHostEndpoint()
    {
        // Arrange
        await using var sut = await StellarAdminTestHost.CreateAsync(
            new([]),
            configureAdmin: admin =>
                admin.ConfigureUserMenu(menu =>
                    menu.Items = [
                        UserMenuItem.Action("Profile", "Index", "Home", "StellarAdmin"),
                        UserMenuItem.Logout("~/Identity/Account/Logout"),
                    ]
                ),
            configureServices: TestAuthenticationHandler.Register
        );
        using var client = sut.GetTestClient();
        TestAuthenticationHandler.SignIn(client, "ada");

        // Act
        var document = await client.GetDocumentAsync("/stellaradmin");

        // Assert
        await Assert
            .That(document.TextContents(MenuEntries))
            .IsEquivalentTo(["", "Profile", "Log out"], CollectionOrdering.Matching);
        await Assert
            .That(
                document
                    .RequiredElement("#sa-user-menu a[data-slot='dropdown-menu-item']")
                    .GetAttribute("href")
            )
            .IsEqualTo("/stellaradmin");
        var form = document.RequiredElement("#sa-user-menu form");
        await Assert.That(form.GetAttribute("action")).IsEqualTo("/Identity/Account/Logout");
        await Assert
            .That(form.QuerySelector("input[name='__RequestVerificationToken']"))
            .IsNotNull();
    }

    [Test]
    public async Task ItemsWithoutLogout_ShowNoLogOut()
    {
        // Arrange
        await using var sut = await StellarAdminTestHost.CreateAsync(
            new([]),
            configureAdmin: admin => admin.ConfigureUserMenu(menu => menu.Items.Clear()),
            configureServices: TestAuthenticationHandler.Register
        );
        using var client = sut.GetTestClient();
        TestAuthenticationHandler.SignIn(client, "ada");

        // Act
        var document = await client.GetDocumentAsync("/stellaradmin");

        // Assert
        await Assert.That(document.QuerySelector(Footer)).IsNotNull();
        await Assert.That(document.TextContents(MenuEntries)).IsEmpty();
    }
}
