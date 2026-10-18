using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Hosting;
using StellarAdmin.Home;
using StellarAdmin.Sidebar;

namespace StellarAdmin.Tests.Home;

public partial class SetupStatusResolverTests
{
    [Test]
    public async Task ResolveAsync_OutsideDevelopment_ReturnsNullWithoutResolvingServices()
    {
        // Arrange
        var sut = new SetupStatusResolver(
            new StellarAdminOptions(),
            new FakeHostEnvironment(Environments.Production),
            new ThrowingServiceProvider()
        );

        // Act
        var status = await sut.ResolveAsync(new DefaultHttpContext());

        // Assert
        await Assert.That(status).IsNull();
    }

    [Test]
    public async Task ResolveAsync_WhenChecklistIsTurnedOff_ReturnsNullWithoutResolvingServices()
    {
        // Arrange
        var options = new StellarAdminOptions();
        options.HomePage.ShowSetupChecklist = false;
        var sut = new SetupStatusResolver(
            options,
            new FakeHostEnvironment(Environments.Development),
            new ThrowingServiceProvider()
        );

        // Act
        var status = await sut.ResolveAsync(new DefaultHttpContext());

        // Assert
        await Assert.That(status).IsNull();
    }

    [Test]
    public async Task ResolveAsync_WithDefaultConfiguration_ReportsNothingDone()
    {
        // Arrange
        var services = CreateServices();
        services.AddStellarAdmin();
        using var provider = services.BuildServiceProvider();
        var sut = provider.GetRequiredService<SetupStatusResolver>();

        // Act
        var status = await sut.ResolveAsync(new DefaultHttpContext());

        // Assert
        await Assert.That(status).IsNotNull();
        await Assert.That(status!.HasAuthentication).IsFalse();
        await Assert.That(status.ResourceCount).IsEqualTo(0);
        await Assert.That(status.IsProtected).IsFalse();
        await Assert.That(status.HasBranding).IsFalse();
        await Assert.That(status.HasUserMenu).IsFalse();
        await Assert.That(status.HasTheme).IsFalse();
        await Assert.That(status.HasSidebarLayout).IsFalse();
    }

    [Test]
    public async Task ResolveAsync_WithResource_CountsIt()
    {
        // Arrange
        var services = CreateServices();
        services.AddStellarAdmin(admin => admin.AddResource<Booking>());
        using var provider = services.BuildServiceProvider();
        var sut = provider.GetRequiredService<SetupStatusResolver>();

        // Act
        var status = await sut.ResolveAsync(new DefaultHttpContext());

        // Assert
        await Assert.That(status!.ResourceCount).IsEqualTo(1);
    }

    [Test]
    public async Task ResolveAsync_WithAuthenticationScheme_ReportsAuthentication()
    {
        // Arrange
        var services = CreateServices();
        services.AddAuthentication().AddCookie();
        services.AddStellarAdmin();
        using var provider = services.BuildServiceProvider();
        var sut = provider.GetRequiredService<SetupStatusResolver>();

        // Act
        var status = await sut.ResolveAsync(new DefaultHttpContext());

        // Assert
        await Assert.That(status!.HasAuthentication).IsTrue();
    }

    [Test]
    public async Task ResolveAsync_WithAuthenticationButNoSchemes_ReportsNoAuthentication()
    {
        // Arrange
        var services = CreateServices();
        services.AddAuthentication();
        services.AddStellarAdmin();
        using var provider = services.BuildServiceProvider();
        var sut = provider.GetRequiredService<SetupStatusResolver>();

        // Act
        var status = await sut.ResolveAsync(new DefaultHttpContext());

        // Assert
        await Assert.That(status!.HasAuthentication).IsFalse();
    }

    [Test]
    [Arguments("Title")]
    [Arguments("Logo")]
    public async Task ResolveAsync_WithBranding_ReportsBranding(string setting)
    {
        // Arrange
        var services = CreateServices();
        services.AddStellarAdmin(admin =>
            admin.ConfigureBranding(branding =>
            {
                if (setting == "Title")
                {
                    branding.Title = "Voyager Travel";
                }
                else
                {
                    branding.Logo = BrandingLogo.Icon("compass");
                }
            })
        );
        using var provider = services.BuildServiceProvider();
        var sut = provider.GetRequiredService<SetupStatusResolver>();

        // Act
        var status = await sut.ResolveAsync(new DefaultHttpContext());

        // Assert
        await Assert.That(status!.HasBranding).IsTrue();
    }

    [Test]
    [Arguments("Item")]
    [Arguments("NameClaimType")]
    [Arguments("DescriptionClaimType")]
    [Arguments("AvatarClaimType")]
    [Arguments("LogoutPath")]
    [Arguments("LogoutRedirectPath")]
    [Arguments("LogoutScheme")]
    [Arguments("LogoutHidden")]
    [Arguments("LogoutIcon")]
    public async Task ResolveAsync_WithUserMenuChange_ReportsUserMenu(string change)
    {
        // Arrange
        var services = CreateServices();
        services.AddStellarAdmin(admin =>
            admin.ConfigureUserMenu(menu =>
            {
                switch (change)
                {
                    case "Item":
                        menu.Add(UserMenuItem.Url("Help", "https://help.voyager.travel"));
                        break;
                    case "NameClaimType":
                        menu.NameClaimType = "name";
                        break;
                    case "DescriptionClaimType":
                        menu.DescriptionClaimType = "role";
                        break;
                    case "AvatarClaimType":
                        menu.AvatarClaimType = "picture";
                        break;
                    case "LogoutPath":
                        menu.Logout(logout => logout.Path = "/account/logout");
                        break;
                    case "LogoutRedirectPath":
                        menu.Logout(logout => logout.RedirectPath = "~/");
                        break;
                    case "LogoutScheme":
                        menu.Logout(logout => logout.AddAuthenticationScheme("oidc"));
                        break;
                    case "LogoutHidden":
                        menu.Logout(logout => logout.Show = false);
                        break;
                    default:
                        menu.Logout(logout => logout.Icon = "door-open");
                        break;
                }
            })
        );
        using var provider = services.BuildServiceProvider();
        var sut = provider.GetRequiredService<SetupStatusResolver>();

        // Act
        var status = await sut.ResolveAsync(new DefaultHttpContext());

        // Assert
        await Assert.That(status!.HasUserMenu).IsTrue();
    }

    [Test]
    public async Task ResolveAsync_WithAddedStylesheet_ReportsTheme()
    {
        // Arrange
        var services = CreateServices();
        services.AddStellarAdmin(admin => admin.AddStylesheet("~/css/theme.css"));
        using var provider = services.BuildServiceProvider();
        var sut = provider.GetRequiredService<SetupStatusResolver>();

        // Act
        var status = await sut.ResolveAsync(new DefaultHttpContext());

        // Assert
        await Assert.That(status!.HasTheme).IsTrue();
    }

    [Test]
    public async Task ResolveAsync_WithWebFontAndLocalStylesheet_NamesLocalStylesheet()
    {
        // Arrange
        var services = CreateServices();
        services.AddStellarAdmin(admin =>
        {
            admin.AddStylesheet("https://fonts.googleapis.com/css2?family=Lexend");
            admin.AddStylesheet("~/css/theme.css");
        });
        using var provider = services.BuildServiceProvider();
        var sut = provider.GetRequiredService<SetupStatusResolver>();

        // Act
        var status = await sut.ResolveAsync(new DefaultHttpContext());

        // Assert
        await Assert.That(status!.ThemeStylesheet).IsEqualTo("~/css/theme.css");
    }

    [Test]
    public async Task ResolveAsync_WithSidebarLink_ReportsSidebarLayout()
    {
        // Arrange
        var services = CreateServices();
        services.AddStellarAdmin(admin =>
            admin.AddSidebarLink("Reports", SidebarLinkTarget.Url("/reports"))
        );
        using var provider = services.BuildServiceProvider();
        var sut = provider.GetRequiredService<SetupStatusResolver>();

        // Act
        var status = await sut.ResolveAsync(new DefaultHttpContext());

        // Assert
        await Assert.That(status!.HasSidebarLayout).IsTrue();
    }

    [Test]
    [Arguments("Group")]
    [Arguments("Order")]
    [Arguments("Icon")]
    public async Task ResolveAsync_WithResourceSidebarItemChange_ReportsSidebarLayout(string change)
    {
        // Arrange
        var services = CreateServices();
        services.AddStellarAdmin(admin =>
            admin.AddResource<Booking>(resource =>
                resource.SidebarItem(item =>
                {
                    switch (change)
                    {
                        case "Group":
                            item.Group = "Sales";
                            break;
                        case "Order":
                            item.Order = 1;
                            break;
                        default:
                            item.Icon = "plane";
                            break;
                    }
                })
            )
        );
        using var provider = services.BuildServiceProvider();
        var sut = provider.GetRequiredService<SetupStatusResolver>();

        // Act
        var status = await sut.ResolveAsync(new DefaultHttpContext());

        // Assert
        await Assert.That(status!.HasSidebarLayout).IsTrue();
    }

    [Test]
    public async Task ResolveAsync_WithEntityRegisteredAsResource_LeavesItOutOfCandidates()
    {
        // Arrange
        var services = CreateServices();
        services.AddSingleton(new DbContextOptions<TravelContext>());
        services.AddStellarAdmin(admin => admin.AddResource<Booking>());
        using var provider = services.BuildServiceProvider();
        var sut = provider.GetRequiredService<SetupStatusResolver>();

        // Act
        var status = await sut.ResolveAsync(new DefaultHttpContext());

        // Assert
        await Assert.That(status!.Entities.Contexts).Count().IsEqualTo(1);
        await Assert
            .That(status.Entities.Contexts[0].EntityTypes)
            .IsEquivalentTo([typeof(Customer)]);
    }

    [Test]
    public async Task ResolveAsync_WhenEveryEntityIsAResource_ListsNoContexts()
    {
        // Arrange
        var services = CreateServices();
        services.AddSingleton(new DbContextOptions<TravelContext>());
        services.AddStellarAdmin(admin =>
        {
            admin.AddResource<Booking>();
            admin.AddResource<Customer>();
        });
        using var provider = services.BuildServiceProvider();
        var sut = provider.GetRequiredService<SetupStatusResolver>();

        // Act
        var status = await sut.ResolveAsync(new DefaultHttpContext());

        // Assert
        await Assert.That(status!.Entities.Contexts).IsEmpty();
    }

    private static ServiceCollection CreateServices()
    {
        var services = new ServiceCollection();
        services.AddSingleton<IHostEnvironment>(new FakeHostEnvironment(Environments.Development));

        return services;
    }

    private static HttpContext CreateHttpContext(params object[] endpointMetadata)
    {
        var httpContext = new DefaultHttpContext();
        httpContext.SetEndpoint(
            new Endpoint(null, new EndpointMetadataCollection(endpointMetadata), "Home")
        );

        return httpContext;
    }

    public sealed class Booking;

    public sealed class Customer;

    public sealed class TravelContext
    {
        public DbSet<Booking>? Bookings { get; set; }

        public DbSet<Customer>? Customers { get; set; }
    }

    private sealed class FakeHostEnvironment(string environmentName) : IHostEnvironment
    {
        public string ApplicationName { get; set; } = "Voyager.Web";

        public IFileProvider ContentRootFileProvider { get; set; } = new NullFileProvider();

        public string ContentRootPath { get; set; } = "";

        public string EnvironmentName { get; set; } = environmentName;
    }

    private sealed class ThrowingServiceProvider : IServiceProvider
    {
        public object GetService(Type serviceType)
        {
            throw new InvalidOperationException($"{serviceType} was resolved.");
        }
    }
}
