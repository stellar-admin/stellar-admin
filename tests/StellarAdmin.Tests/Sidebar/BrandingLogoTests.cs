using StellarAdmin.Sidebar;

namespace StellarAdmin.Tests.Sidebar;

public class BrandingLogoTests
{
    [Test]
    public async Task Wordmark_WithoutLayout_CentersWordmark()
    {
        // Act
        var result = BrandingLogo.Wordmark("~/img/wordmark.svg", "~/img/wordmark-dark.svg");

        // Assert
        await Assert.That(result.Layout).IsEqualTo(WordmarkLayout.Center);
    }

    [Test]
    public async Task Wordmark_WithUndefinedLayout_RecordsProblem()
    {
        // Act
        var result = BrandingLogo.Wordmark("~/img/wordmark.svg", (WordmarkLayout)99);

        // Assert
        await Assert.That(result.Problem).IsNotNull();
    }
}
