using Microsoft.Extensions.Options;
using StellarAdmin.Icons;
using StellarAdmin.TagHelpers.Tests.Support;

namespace StellarAdmin.TagHelpers.Tests.TagHelpers.Icon;

public partial class IconTagHelperTests
{
    [Test]
    public async Task Constructor_WhenGivenOptions_ResolvesValueOnce()
    {
        // Arrange
        var options = new TrackingIconOptions();

        // Act
        var sut = new IconTagHelper(options);

        // Assert
        await Assert.That(options.ReadCount).IsEqualTo(1);
    }

    [Test]
    public async Task Process_WhenIconIsMissing_RendersFallbackGlyph()
    {
        // Arrange
        var sut = new IconTagHelper(Options.Create(new IconOptions()))
        {
            Name = "missing-icon-for-test",
        };

        // Act
        using var html = await TagHelperRenderer.RenderAsync(sut);

        // Assert
        await Assert.That(html.QuerySelector("svg path[d='M12 9v4']")).IsNotNull();
    }

    [Test]
    public async Task Process_WhenIconIsRegistered_RendersSvgShapes()
    {
        // Arrange
        var sut = new IconTagHelper(Options.Create(new IconOptions())) { Name = "check" };

        // Act
        using var html = await TagHelperRenderer.RenderAsync(sut);

        // Assert
        await Assert.That(html.QuerySelector("svg")).IsNotNull();
        await Assert.That(html.QuerySelector("svg path")).IsNotNull();
    }

    [Test]
    [Arguments("check")]
    [Arguments("missing-icon-for-test")]
    public async Task Process_WhenRendering_UsesAlreadyResolvedOptions(string name)
    {
        // Arrange
        var options = new TrackingIconOptions();
        var sut = new IconTagHelper(options) { Name = name };

        // Act
        using var html = await TagHelperRenderer.RenderAsync(sut);

        // Assert
        await Assert.That(options.ReadCount).IsEqualTo(1);
    }

    private sealed class TrackingIconOptions : IOptions<IconOptions>
    {
        private readonly IconOptions _value = new();
        public int ReadCount { get; private set; }
        public IconOptions Value
        {
            get
            {
                ReadCount++;
                return _value;
            }
        }
    }
}
