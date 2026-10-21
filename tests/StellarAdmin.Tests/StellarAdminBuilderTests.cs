using Microsoft.Extensions.DependencyInjection;
using StellarAdmin.Diagnostics;
using StellarAdmin.Sidebar;

namespace StellarAdmin.Tests;

public class StellarAdminBuilderTests
{
    [Test]
    [Arguments("")]
    [Arguments(" ")]
    public async Task AddSidebarLink_WithBlankGroup_UsesDefaultGroup(string group)
    {
        // Arrange
        var sut = new ServiceCollection().AddStellarAdmin();

        // Act
        sut.AddSidebarLink(
            "Reports",
            SidebarLinkTarget.Url("/reports"),
            link => link.Group = group
        );

        // Assert
        await Assert.That(sut.Options.SidebarLinks.Single().Group).IsNull();
    }

    [Test]
    [Arguments("")]
    [Arguments(" ")]
    public async Task AddSidebarLink_WithBlankIcon_ShowsNoIcon(string icon)
    {
        // Arrange
        var sut = new ServiceCollection().AddStellarAdmin();

        // Act
        sut.AddSidebarLink("Reports", SidebarLinkTarget.Url("/reports"), link => link.Icon = icon);

        // Assert
        await Assert.That(sut.Options.SidebarLinks.Single().Icon).IsNull();
    }

    [Test]
    [Arguments("")]
    [Arguments(" ")]
    public async Task AddSidebarLink_WithBlankLabel_LeavesOutLink(string label)
    {
        // Arrange
        var sut = new ServiceCollection().AddStellarAdmin();

        // Act
        sut.AddSidebarLink(label, SidebarLinkTarget.Url("/reports"));

        // Assert
        await Assert.That(sut.Options.SidebarLinks).IsEmpty();
        await Assert
            .That(sut.Options.Problems.Single().Kind)
            .IsEqualTo(ConfigurationProblemKind.InvalidLink);
    }

    [Test]
    public async Task AddSidebarLink_WithBlankTarget_LeavesOutLink()
    {
        // Arrange
        var sut = new ServiceCollection().AddStellarAdmin();

        // Act
        sut.AddSidebarLink("Reports", SidebarLinkTarget.Page(" "));

        // Assert
        await Assert.That(sut.Options.SidebarLinks).IsEmpty();
        await Assert
            .That(sut.Options.Problems.Single().Kind)
            .IsEqualTo(ConfigurationProblemKind.InvalidLink);
    }

    [Test]
    public async Task ConfigureBranding_WithWordmarkAndSubtitle_DropsSubtitle()
    {
        // Arrange
        var sut = new ServiceCollection().AddStellarAdmin();

        // Act
        sut.ConfigureBranding(branding =>
        {
            branding.Subtitle = "Admin Console";
            branding.Logo = BrandingLogo.Wordmark("~/img/wordmark.svg");
        });

        // Assert
        await Assert.That(sut.Options.Branding.Subtitle).IsNull();
        await Assert
            .That(sut.Options.Problems.Single().Kind)
            .IsEqualTo(ConfigurationProblemKind.WordmarkWithSubtitle);
    }

    [Test]
    public async Task ConfigureDiagnostics_WithSettings_SetsOptions()
    {
        // Arrange
        var sut = new ServiceCollection().AddStellarAdmin();

        // Act
        sut.ConfigureDiagnostics(diagnostics =>
        {
            diagnostics.ShowIndicator = false;
            diagnostics.ThrowOnProblems = true;
        });

        // Assert
        await Assert.That(sut.Options.Diagnostics.ShowIndicator).IsFalse();
        await Assert.That(sut.Options.Diagnostics.ThrowOnProblems).IsTrue();
    }

    [Test]
    public async Task EnableEnvironmentIndicator_WithBlankLabel_ShowsNoIndicator()
    {
        // Arrange
        var sut = new ServiceCollection().AddStellarAdmin();

        // Act
        sut.EnableEnvironmentIndicator(EnvironmentIndicator.Stripe(" "));

        // Assert
        await Assert.That(sut.Options.EnvironmentIndicator).IsNull();
    }

    [Test]
    public async Task EnableEnvironmentIndicator_WithUndefinedTabSize_ReportsProblem()
    {
        // Arrange
        var sut = new ServiceCollection().AddStellarAdmin();
        var indicator = EnvironmentIndicator.Stripe("Testing");
        indicator.TabSize = (EnvironmentIndicatorTabSize)99;

        // Act
        sut.EnableEnvironmentIndicator(indicator);

        // Assert
        await Assert.That(sut.Options.EnvironmentIndicator).IsSameReferenceAs(indicator);
    }
}
