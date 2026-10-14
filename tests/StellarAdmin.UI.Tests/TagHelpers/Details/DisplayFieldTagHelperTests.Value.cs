using StellarAdmin.UI.Tests.Support;

namespace StellarAdmin.UI.Tests.TagHelpers.Details;

public partial class DisplayFieldTagHelperTests
{
    [Test]
    public async Task ProcessAsync_WhenChildContentIsPresent_ReplacesValue()
    {
        // Arrange
        using var context = new RenderingContext();
        var sut = CreateSut(context);
        sut.For = CreateFor(
            context,
            new Booking { LeadTraveller = "Kenji Watanabe" },
            nameof(Booking.LeadTraveller)
        );

        // Act
        using var html = await TagHelperRenderer.RenderAsync(
            sut,
            _ => Task.FromResult("<b>Custom</b>")
        );

        // Assert
        await Assert.That(Value(html)?.InnerHtml).IsEqualTo("<b>Custom</b>");
    }

    [Test]
    public async Task ProcessAsync_WhenValueIsText_RendersEncodedText()
    {
        // Arrange
        using var context = new RenderingContext();
        var sut = CreateSut(context);
        sut.For = CreateFor(
            context,
            new Booking { LeadTraveller = "<Kenji>" },
            nameof(Booking.LeadTraveller)
        );

        // Act
        using var html = await TagHelperRenderer.RenderAsync(sut);

        // Assert
        await Assert.That(Value(html)?.InnerHtml).IsEqualTo("&lt;Kenji&gt;");
        await Assert.That(Value(html)?.HasAttribute("data-empty")).IsFalse();
    }

    [Test]
    [Arguments(null)]
    [Arguments("")]
    [Arguments("  ")]
    public async Task ProcessAsync_WhenValueIsEmpty_RendersDefaultEmptyText(string? promoCode)
    {
        // Arrange
        using var context = new RenderingContext();
        var sut = CreateSut(context);
        sut.For = CreateFor(
            context,
            new Booking { PromoCode = promoCode },
            nameof(Booking.PromoCode)
        );

        // Act
        using var html = await TagHelperRenderer.RenderAsync(sut);

        // Assert
        await Assert.That(Value(html)?.TextContent).IsEqualTo("—");
        await Assert.That(Value(html)?.GetAttribute("data-empty")).IsEqualTo("true");
    }

    [Test]
    public async Task ProcessAsync_WhenCollectionIsEmpty_RendersEmptyText()
    {
        // Arrange
        using var context = new RenderingContext();
        var sut = CreateSut(context);
        sut.For = CreateFor(context, new Booking(), nameof(Booking.Tags));

        // Act
        using var html = await TagHelperRenderer.RenderAsync(sut);

        // Assert
        await Assert.That(Value(html)?.GetAttribute("data-empty")).IsEqualTo("true");
    }

    [Test]
    public async Task ProcessAsync_WhenUnboundWithoutChildContent_RendersEmptyText()
    {
        // Arrange
        using var context = new RenderingContext();
        var sut = CreateSut(context);
        sut.Label = "Promo code";

        // Act
        using var html = await TagHelperRenderer.RenderAsync(sut);

        // Assert
        await Assert.That(Value(html)?.GetAttribute("data-empty")).IsEqualTo("true");
    }

    [Test]
    public async Task ProcessAsync_WhenEmptyTextIsSet_RendersItForEmptyValue()
    {
        // Arrange
        using var context = new RenderingContext();
        var sut = CreateSut(context);
        sut.For = CreateFor(context, new Booking(), nameof(Booking.PromoCode));
        sut.EmptyText = "None";

        // Act
        using var html = await TagHelperRenderer.RenderAsync(sut);

        // Assert
        await Assert.That(Value(html)?.TextContent).IsEqualTo("None");
    }

    [Test]
    public async Task ProcessAsync_WhenPropertyHasNullDisplayText_RendersItForEmptyValue()
    {
        // Arrange
        using var context = new RenderingContext();
        var sut = CreateSut(context);
        sut.For = CreateFor(context, new Booking(), nameof(Booking.ArchivedAt));

        // Act
        using var html = await TagHelperRenderer.RenderAsync(sut);

        // Assert
        await Assert.That(Value(html)?.TextContent).IsEqualTo("Never");
    }

    [Test]
    public async Task ProcessAsync_WhenValueIsZero_RendersNumber()
    {
        // Arrange
        using var context = new RenderingContext();
        var sut = CreateSut(context);
        sut.For = CreateFor(context, new Booking { Infants = 0 }, nameof(Booking.Infants));

        // Act
        using var html = await TagHelperRenderer.RenderAsync(sut);

        // Assert
        await Assert.That(Value(html)?.TextContent).IsEqualTo("0");
        await Assert.That(Value(html)?.GetAttribute("data-kind")).IsEqualTo("number");
        await Assert.That(Value(html)?.HasAttribute("data-empty")).IsFalse();
    }

    [Test]
    public async Task ProcessAsync_WhenPropertyHasDisplayFormat_FormatsValue()
    {
        // Arrange
        using var context = new RenderingContext();
        var sut = CreateSut(context);
        sut.For = CreateFor(
            context,
            new Booking { Departure = new DateOnly(2026, 11, 14) },
            nameof(Booking.Departure)
        );

        // Act
        using var html = await TagHelperRenderer.RenderAsync(sut);

        // Assert
        await Assert.That(Value(html)?.TextContent).IsEqualTo("2026-11-14");
    }

    [Test]
    public async Task ProcessAsync_WhenFormatIsSet_OverridesDisplayFormat()
    {
        // Arrange
        using var context = new RenderingContext();
        var sut = CreateSut(context);
        sut.For = CreateFor(
            context,
            new Booking { Departure = new DateOnly(2026, 11, 14) },
            nameof(Booking.Departure)
        );
        sut.Format = "{0:yyyy}";

        // Act
        using var html = await TagHelperRenderer.RenderAsync(sut);

        // Assert
        await Assert.That(Value(html)?.TextContent).IsEqualTo("2026");
    }

    [Test]
    public async Task ProcessAsync_WhenValueIsEnum_RendersDisplayName()
    {
        // Arrange
        using var context = new RenderingContext();
        var sut = CreateSut(context);
        sut.For = CreateFor(
            context,
            new Booking { Status = BookingStatus.OnHold },
            nameof(Booking.Status)
        );

        // Act
        using var html = await TagHelperRenderer.RenderAsync(sut);

        // Assert
        await Assert.That(Value(html)?.TextContent).IsEqualTo("On hold");
    }

    [Test]
    public async Task ProcessAsync_WhenValueIsCollection_JoinsItems()
    {
        // Arrange
        using var context = new RenderingContext();
        var sut = CreateSut(context);
        sut.For = CreateFor(
            context,
            new Booking { Tags = ["Culture", "Food"] },
            nameof(Booking.Tags)
        );

        // Act
        using var html = await TagHelperRenderer.RenderAsync(sut);

        // Assert
        await Assert.That(Value(html)?.TextContent).IsEqualTo("Culture, Food");
    }

    [Test]
    public async Task ProcessAsync_WhenPropertyIsMultilineText_MarksTextKind()
    {
        // Arrange
        using var context = new RenderingContext();
        var sut = CreateSut(context);
        sut.For = CreateFor(context, new Booking { Notes = "Line" }, nameof(Booking.Notes));

        // Act
        using var html = await TagHelperRenderer.RenderAsync(sut);

        // Assert
        await Assert.That(Value(html)?.GetAttribute("data-kind")).IsEqualTo("text");
    }

    [Test]
    [Arguments(true, "true", "Yes")]
    [Arguments(false, "false", "No")]
    public async Task ProcessAsync_WhenValueIsBoolean_RendersIconWithScreenReaderText(
        bool value,
        string expectedState,
        string expectedText
    )
    {
        // Arrange
        using var context = new RenderingContext();
        var sut = CreateSut(context);
        sut.For = CreateFor(
            context,
            new Booking { TravelInsurance = value },
            nameof(Booking.TravelInsurance)
        );

        // Act
        using var html = await TagHelperRenderer.RenderAsync(sut);

        // Assert
        await Assert.That(Value(html)?.GetAttribute("data-boolean")).IsEqualTo(expectedState);
        await Assert.That(Value(html)?.QuerySelector("svg[aria-hidden=true]")).IsNotNull();
        await Assert
            .That(Value(html)?.QuerySelector(".sr-only")?.TextContent)
            .IsEqualTo(expectedText);
    }

    [Test]
    public async Task ProcessAsync_WhenPropertyIsEmailAddress_LinksToMailto()
    {
        // Arrange
        using var context = new RenderingContext();
        var sut = CreateSut(context);
        sut.For = CreateFor(
            context,
            new Booking { Email = "kenji@example.com" },
            nameof(Booking.Email)
        );

        // Act
        using var html = await TagHelperRenderer.RenderAsync(sut);

        // Assert
        await Assert
            .That(Value(html)?.QuerySelector("a")?.GetAttribute("href"))
            .IsEqualTo("mailto:kenji@example.com");
    }

    [Test]
    [Arguments("https://voyager.example/trips", "https://voyager.example/trips")]
    [Arguments("javascript:alert(1)", null)]
    public async Task ProcessAsync_WhenPropertyIsUrl_LinksOnlyWebUrls(
        string website,
        string? expectedHref
    )
    {
        // Arrange
        using var context = new RenderingContext();
        var sut = CreateSut(context);
        sut.For = CreateFor(context, new Booking { Website = website }, nameof(Booking.Website));

        // Act
        using var html = await TagHelperRenderer.RenderAsync(sut);

        // Assert
        await Assert
            .That(Value(html)?.QuerySelector("a")?.GetAttribute("href"))
            .IsEqualTo(expectedHref);
        await Assert.That(Value(html)?.TextContent).IsEqualTo(website);
    }

    [Test]
    public async Task ProcessAsync_WhenHrefIsSet_LinksValue()
    {
        // Arrange
        using var context = new RenderingContext();
        var sut = CreateSut(context);
        sut.For = CreateFor(
            context,
            new Booking { LeadTraveller = "Kenji Watanabe" },
            nameof(Booking.LeadTraveller)
        );
        sut.Href = "/travellers/7";

        // Act
        using var html = await TagHelperRenderer.RenderAsync(sut);

        // Assert
        var link = Value(html)?.QuerySelector("a");
        await Assert.That(link?.GetAttribute("href")).IsEqualTo("/travellers/7");
        await Assert.That(link?.TextContent).IsEqualTo("Kenji Watanabe");
    }

    [Test]
    public async Task ProcessAsync_WhenSecondaryIsSet_AppendsSecondaryText()
    {
        // Arrange
        using var context = new RenderingContext();
        var sut = CreateSut(context);
        sut.For = CreateFor(
            context,
            new Booking { LeadTraveller = "Kenji Watanabe" },
            nameof(Booking.LeadTraveller)
        );
        sut.Secondary = "by Amelia Hart";

        // Act
        using var html = await TagHelperRenderer.RenderAsync(sut);

        // Assert
        await Assert
            .That(Value(html)?.QuerySelector("[data-slot=display-field-secondary]")?.TextContent)
            .IsEqualTo("by Amelia Hart");
    }

    [Test]
    public async Task ProcessAsync_WhenSecondaryIsSetAndValueIsEmpty_OmitsSecondaryText()
    {
        // Arrange
        using var context = new RenderingContext();
        var sut = CreateSut(context);
        sut.For = CreateFor(context, new Booking(), nameof(Booking.PromoCode));
        sut.Secondary = "by Amelia Hart";

        // Act
        using var html = await TagHelperRenderer.RenderAsync(sut);

        // Assert
        await Assert
            .That(Value(html)?.QuerySelector("[data-slot=display-field-secondary]"))
            .IsNull();
    }
}
