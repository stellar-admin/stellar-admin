using StellarAdmin.UI.Tests.Support;

namespace StellarAdmin.UI.Tests.TagHelpers.Details;

public class DetailsTagHelperTests
{
    [Test]
    public async Task ProcessAsync_WhenRendered_WrapsChildrenInDescriptionList()
    {
        // Arrange
        var sut = new DetailsTagHelper();

        // Act
        using var html = await TagHelperRenderer.RenderAsync(
            sut,
            _ => Task.FromResult("<div id=\"child\"></div>")
        );

        // Assert
        var root = html.Body?.FirstElementChild;
        await Assert.That(root?.GetAttribute("data-slot")).IsEqualTo("details");
        await Assert.That(root?.ClassName).IsEqualTo("sa-details existing-class");
        await Assert
            .That(html.QuerySelector("[data-slot=details] > dl.sa-details-list > #child"))
            .IsNotNull();
    }

    [Test]
    public async Task ProcessAsync_WhenColumnsAndVariantAreOmitted_RendersOneColumnDefaultVariant()
    {
        // Arrange
        var sut = new DetailsTagHelper();

        // Act
        using var html = await TagHelperRenderer.RenderAsync(sut);

        // Assert
        await Assert.That(html.QuerySelector("dl")?.HasAttribute("style")).IsFalse();
        await Assert
            .That(html.Body?.FirstElementChild?.GetAttribute("data-variant"))
            .IsEqualTo("default");
    }

    [Test]
    public async Task ProcessAsync_WhenVariantIsSet_RendersIt()
    {
        // Arrange
        var sut = new DetailsTagHelper { Variant = DetailsVariant.Divided };

        // Act
        using var html = await TagHelperRenderer.RenderAsync(sut);

        // Assert
        await Assert
            .That(html.Body?.FirstElementChild?.GetAttribute("data-variant"))
            .IsEqualTo("divided");
    }

    [Test]
    [Arguments(null, null, 3, null, "--sa-cols-md:3;--sa-cols-lg:3")]
    [Arguments(null, 2, 3, 4, "--sa-cols-sm:2;--sa-cols-md:3;--sa-cols-lg:4")]
    [Arguments(null, 2, null, null, "--sa-cols-sm:2;--sa-cols-md:2;--sa-cols-lg:2")]
    [Arguments(2, null, null, null, "--sa-cols:2;--sa-cols-sm:2;--sa-cols-md:2;--sa-cols-lg:2")]
    [Arguments(2, null, 3, null, "--sa-cols:2;--sa-cols-sm:2;--sa-cols-md:3;--sa-cols-lg:3")]
    public async Task ProcessAsync_WhenColumnsAreSet_WritesColumnVariablesOnList(
        int? columns,
        int? columnsSmall,
        int? columnsMedium,
        int? columnsLarge,
        string expected
    )
    {
        // Arrange
        var sut = new DetailsTagHelper
        {
            Columns = columns,
            ColumnsSmall = columnsSmall,
            ColumnsMedium = columnsMedium,
            ColumnsLarge = columnsLarge,
        };

        // Act
        using var html = await TagHelperRenderer.RenderAsync(sut);

        // Assert
        await Assert.That(html.QuerySelector("dl")?.GetAttribute("style")).IsEqualTo(expected);
    }

    [Test]
    [Arguments(0, null)]
    [Arguments(13, null)]
    [Arguments(null, 0)]
    [Arguments(null, 13)]
    public async Task ProcessAsync_WhenColumnsAreOutOfRange_ThrowsInvalidOperationException(
        int? columns,
        int? columnsLarge
    )
    {
        // Arrange
        var sut = new DetailsTagHelper { Columns = columns, ColumnsLarge = columnsLarge };

        // Act
        Func<Task> act = () => TagHelperRenderer.RenderAsync(sut);

        // Assert
        await Assert.That(act).Throws<InvalidOperationException>();
    }
}
