using StellarAdmin.TagHelpers.Tests.Support;

namespace StellarAdmin.TagHelpers.Tests.TagHelpers.Details;

public partial class DisplayFieldTagHelperTests
{
    [Test]
    public async Task ProcessAsync_WhenInsideDetails_RendersTermAndDescription()
    {
        // Arrange
        using var context = new RenderingContext();
        var sut = CreateSut(context);
        sut.Label = "Party";
        var details = new DetailsTagHelper();

        // Act
        using var html = await TagHelperRenderer.RenderAsync(
            details,
            detailsContext => TagHelperRenderer.RenderChildAsync(sut, detailsContext)
        );

        // Assert
        await Assert
            .That(html.QuerySelector("dl > [data-slot=display-field] > dt.sa-display-field-label"))
            .IsNotNull();
        await Assert
            .That(html.QuerySelector("dl > [data-slot=display-field] > dd.sa-display-field-value"))
            .IsNotNull();
    }

    [Test]
    public async Task ProcessAsync_WhenOutsideDetails_RendersLabelAndValueAsDivs()
    {
        // Arrange
        using var context = new RenderingContext();
        var sut = CreateSut(context);
        sut.Label = "Party";

        // Act
        using var html = await TagHelperRenderer.RenderAsync(sut);

        // Assert
        await Assert.That(html.QuerySelector("dt, dd")).IsNull();
        await Assert.That(html.QuerySelector("div.sa-display-field-label")).IsNotNull();
        await Assert.That(html.QuerySelector("div.sa-display-field-value")).IsNotNull();
    }

    [Test]
    public async Task ProcessAsync_WhenRendered_RendersFieldRootWithUserClass()
    {
        // Arrange
        using var context = new RenderingContext();
        var sut = CreateSut(context);
        sut.Label = "Party";

        // Act
        using var html = await TagHelperRenderer.RenderAsync(sut);

        // Assert
        var root = html.Body?.FirstElementChild;
        await Assert.That(root?.GetAttribute("data-slot")).IsEqualTo("display-field");
        await Assert.That(root?.ClassName).IsEqualTo("sa-field sa-display-field existing-class");
        await Assert.That(root?.GetAttribute("data-orientation")).IsEqualTo("vertical");
    }

    [Test]
    [Arguments(null, FieldOrientation.Horizontal, "horizontal")]
    [Arguments(FieldOrientation.Responsive, FieldOrientation.Horizontal, "responsive")]
    public async Task ProcessAsync_WhenInsideDetailsWithOrientation_ResolvesFieldOrientation(
        FieldOrientation? fieldOrientation,
        FieldOrientation detailsOrientation,
        string expected
    )
    {
        // Arrange
        using var context = new RenderingContext();
        var sut = CreateSut(context);
        sut.Label = "Party";
        sut.Orientation = fieldOrientation;
        var details = new DetailsTagHelper { Orientation = detailsOrientation };

        // Act
        using var html = await TagHelperRenderer.RenderAsync(
            details,
            detailsContext => TagHelperRenderer.RenderChildAsync(sut, detailsContext)
        );

        // Assert
        await Assert
            .That(html.QuerySelector("[data-slot=display-field]")?.GetAttribute("data-orientation"))
            .IsEqualTo(expected);
    }

    [Test]
    [Arguments(3, 2, null, "2")]
    [Arguments(4, 3, null, "3")]
    [Arguments(2, 2, null, "full")]
    [Arguments(3, 5, null, "full")]
    [Arguments(3, null, true, "full")]
    [Arguments(1, 2, null, null)]
    [Arguments(3, 1, null, null)]
    public async Task ProcessAsync_WhenInsideDetailsWithColumnSpan_CapsSpanAtColumnCount(
        int columns,
        int? columnSpan,
        bool? columnSpanFull,
        string? expected
    )
    {
        // Arrange
        using var context = new RenderingContext();
        var sut = CreateSut(context);
        sut.Label = "Notes";
        sut.ColumnSpan = columnSpan;
        sut.ColumnSpanFull = columnSpanFull;
        var details = new DetailsTagHelper { Columns = columns };

        // Act
        using var html = await TagHelperRenderer.RenderAsync(
            details,
            detailsContext => TagHelperRenderer.RenderChildAsync(sut, detailsContext)
        );

        // Assert
        await Assert
            .That(html.QuerySelector("[data-slot=display-field]")?.GetAttribute("data-column-span"))
            .IsEqualTo(expected);
    }

    [Test]
    public async Task ProcessAsync_WhenColumnSpanIsLessThanOne_ThrowsInvalidOperationException()
    {
        // Arrange
        using var context = new RenderingContext();
        var sut = CreateSut(context);
        sut.Label = "Notes";
        sut.ColumnSpan = 0;

        // Act
        Func<Task> act = () => TagHelperRenderer.RenderAsync(sut);

        // Assert
        await Assert.That(act).Throws<InvalidOperationException>();
    }
}
