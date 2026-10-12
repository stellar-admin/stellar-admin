using StellarAdmin.TagHelpers.Tests.Support;

namespace StellarAdmin.TagHelpers.Tests.TagHelpers.Details;

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
        var root = html.Body?.FirstElementChild;
        await Assert.That(root?.GetAttribute("data-columns")).IsEqualTo("1");
        await Assert.That(root?.GetAttribute("data-variant")).IsEqualTo("default");
    }

    [Test]
    public async Task ProcessAsync_WhenColumnsAndVariantAreSet_RendersThem()
    {
        // Arrange
        var sut = new DetailsTagHelper { Columns = 3, Variant = DetailsVariant.Divided };

        // Act
        using var html = await TagHelperRenderer.RenderAsync(sut);

        // Assert
        var root = html.Body?.FirstElementChild;
        await Assert.That(root?.GetAttribute("data-columns")).IsEqualTo("3");
        await Assert.That(root?.GetAttribute("data-variant")).IsEqualTo("divided");
    }

    [Test]
    [Arguments(0)]
    [Arguments(5)]
    public async Task ProcessAsync_WhenColumnsAreOutOfRange_ThrowsInvalidOperationException(
        int columns
    )
    {
        // Arrange
        var sut = new DetailsTagHelper { Columns = columns };

        // Act
        Func<Task> act = () => TagHelperRenderer.RenderAsync(sut);

        // Assert
        await Assert.That(act).Throws<InvalidOperationException>();
    }
}
