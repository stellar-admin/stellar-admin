using StellarAdmin.UI.Tests.Support;

namespace StellarAdmin.UI.Tests.TagHelpers.Details;

public partial class DisplayFieldTagHelperTests
{
    [Test]
    public async Task ProcessAsync_WhenTemplateIsSet_RendersNamedDisplayTemplateWithValue()
    {
        // Arrange
        using var context = new RenderingContext();
        var sut = CreateSut(context, "StatusBadge");
        sut.For = CreateFor(
            context,
            new Booking { Status = BookingStatus.Confirmed },
            nameof(Booking.Status)
        );
        sut.Template = "StatusBadge";

        // Act
        using var html = await TagHelperRenderer.RenderAsync(sut);

        // Assert
        await Assert
            .That(Value(html)?.TextContent)
            .IsEqualTo("DisplayTemplates/StatusBadge:Confirmed");
    }

    [Test]
    public async Task ProcessAsync_WhenPropertyHasUIHint_RendersHintedDisplayTemplate()
    {
        // Arrange
        using var context = new RenderingContext();
        var sut = CreateSut(context, "StatusBadge");
        sut.For = CreateFor(
            context,
            new Booking { HintedStatus = BookingStatus.Confirmed },
            nameof(Booking.HintedStatus)
        );

        // Act
        using var html = await TagHelperRenderer.RenderAsync(sut);

        // Assert
        await Assert
            .That(Value(html)?.TextContent)
            .IsEqualTo("DisplayTemplates/StatusBadge:Confirmed");
    }

    [Test]
    public async Task ProcessAsync_WhenTypeHasDisplayTemplate_DoesNotUseIt()
    {
        // Arrange
        using var context = new RenderingContext();
        var sut = CreateSut(context, "Boolean");
        sut.For = CreateFor(
            context,
            new Booking { TravelInsurance = true },
            nameof(Booking.TravelInsurance)
        );

        // Act
        using var html = await TagHelperRenderer.RenderAsync(sut);

        // Assert
        await Assert.That(Value(html)?.GetAttribute("data-boolean")).IsEqualTo("true");
    }

    [Test]
    public async Task ProcessAsync_WhenValueIsEmpty_RendersEmptyTextInsteadOfTemplate()
    {
        // Arrange
        using var context = new RenderingContext();
        var sut = CreateSut(context, "Code");
        sut.For = CreateFor(context, new Booking(), nameof(Booking.PromoCode));
        sut.Template = "Code";

        // Act
        using var html = await TagHelperRenderer.RenderAsync(sut);

        // Assert
        await Assert.That(Value(html)?.GetAttribute("data-empty")).IsEqualTo("true");
    }

    [Test]
    public async Task ProcessAsync_WhenTemplateRendersEmptyAndValueIsEmpty_RendersTemplate()
    {
        // Arrange
        using var context = new RenderingContext();
        var sut = CreateSut(context, "Code");
        sut.For = CreateFor(context, new Booking(), nameof(Booking.PromoCode));
        sut.Template = "Code";
        sut.TemplateRendersEmpty = true;

        // Act
        using var html = await TagHelperRenderer.RenderAsync(sut);

        // Assert
        var value = Value(html);
        await Assert.That(value?.HasAttribute("data-empty")).IsFalse();
        await Assert.That(value?.TextContent).IsEqualTo("DisplayTemplates/Code:");
    }

    [Test]
    public async Task ProcessAsync_WhenTemplateIsMissing_ThrowsInvalidOperationException()
    {
        // Arrange
        using var context = new RenderingContext();
        var sut = CreateSut(context);
        sut.For = CreateFor(
            context,
            new Booking { Status = BookingStatus.Confirmed },
            nameof(Booking.Status)
        );
        sut.Template = "Missing";

        // Act
        Func<Task> act = () => TagHelperRenderer.RenderAsync(sut);

        // Assert
        await Assert.That(act).Throws<InvalidOperationException>();
    }
}
