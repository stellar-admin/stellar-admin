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
    [Arguments(3, 2, null, null, "--sa-span-md:2;--sa-span-lg:2")]
    [Arguments(3, 5, null, null, "--sa-span-md:3;--sa-span-lg:3")]
    [Arguments(3, null, true, null, "--sa-span-md:3;--sa-span-lg:3")]
    [Arguments(3, null, true, 2, "--sa-span-md:3;--sa-span-lg:2")]
    [Arguments(1, 2, null, null, null)]
    [Arguments(3, 1, null, null, null)]
    public async Task ProcessAsync_WhenInsideDetailsWithColumnSpan_LimitsSpanToColumnCount(
        int columns,
        int? columnSpan,
        bool? columnSpanFull,
        int? columnSpanLarge,
        string? expected
    )
    {
        // Arrange
        using var context = new RenderingContext();
        var sut = CreateSut(context);
        sut.Label = "Notes";
        sut.ColumnSpan = columnSpan;
        sut.ColumnSpanFull = columnSpanFull;
        sut.ColumnSpanLarge = columnSpanLarge;
        var details = new DetailsTagHelper { Columns = columns };

        // Act
        using var html = await TagHelperRenderer.RenderAsync(
            details,
            detailsContext => TagHelperRenderer.RenderChildAsync(sut, detailsContext)
        );

        // Assert
        await Assert
            .That(html.QuerySelector("[data-slot=display-field]")?.GetAttribute("style"))
            .IsEqualTo(expected);
    }

    [Test]
    public async Task ProcessAsync_WhenInsideDetailsWithBreakpointSpans_WritesSpanPerBreakpoint()
    {
        // Arrange
        using var context = new RenderingContext();
        var sut = CreateSut(context);
        sut.Label = "Notes";
        sut.ColumnSpanSmall = 2;
        sut.ColumnSpanLarge = 4;
        var details = new DetailsTagHelper
        {
            ColumnsSmall = 2,
            ColumnsMedium = 3,
            ColumnsLarge = 6,
        };

        // Act
        using var html = await TagHelperRenderer.RenderAsync(
            details,
            detailsContext => TagHelperRenderer.RenderChildAsync(sut, detailsContext)
        );

        // Assert
        await Assert
            .That(html.QuerySelector("[data-slot=display-field]")?.GetAttribute("style"))
            .IsEqualTo("--sa-span-sm:2;--sa-span-md:2;--sa-span-lg:4");
    }

    [Test]
    public async Task ProcessAsync_WhenOutsideDetailsWithColumnSpan_WritesNoSpan()
    {
        // Arrange
        using var context = new RenderingContext();
        var sut = CreateSut(context);
        sut.Label = "Notes";
        sut.ColumnSpan = 2;

        // Act
        using var html = await TagHelperRenderer.RenderAsync(sut);

        // Assert
        await Assert.That(html.Body?.FirstElementChild?.HasAttribute("style")).IsFalse();
    }

    [Test]
    [Arguments(0, null)]
    [Arguments(13, null)]
    [Arguments(null, 0)]
    public async Task ProcessAsync_WhenColumnSpanIsOutOfRange_ThrowsInvalidOperationException(
        int? columnSpan,
        int? columnSpanMedium
    )
    {
        // Arrange
        using var context = new RenderingContext();
        var sut = CreateSut(context);
        sut.Label = "Notes";
        sut.ColumnSpan = columnSpan;
        sut.ColumnSpanMedium = columnSpanMedium;

        // Act
        Func<Task> act = () => TagHelperRenderer.RenderAsync(sut);

        // Assert
        await Assert.That(act).Throws<InvalidOperationException>();
    }

    [Test]
    public async Task ProcessAsync_WhenColumnSpanAndColumnSpanFullAreSet_ThrowsInvalidOperationException()
    {
        // Arrange
        using var context = new RenderingContext();
        var sut = CreateSut(context);
        sut.Label = "Notes";
        sut.ColumnSpan = 2;
        sut.ColumnSpanFull = true;

        // Act
        Func<Task> act = () => TagHelperRenderer.RenderAsync(sut);

        // Assert
        await Assert.That(act).Throws<InvalidOperationException>();
    }
}
