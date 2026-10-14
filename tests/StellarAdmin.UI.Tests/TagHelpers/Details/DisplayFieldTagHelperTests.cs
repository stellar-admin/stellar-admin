using System.ComponentModel.DataAnnotations;
using AngleSharp.Dom;
using AngleSharp.Html.Dom;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewEngines;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using StellarAdmin.UI.Tests.Support;

namespace StellarAdmin.UI.Tests.TagHelpers.Details;

public partial class DisplayFieldTagHelperTests
{
    [Test]
    public async Task ProcessAsync_WhenLabelAndForAreMissing_ThrowsInvalidOperationException()
    {
        // Arrange
        using var context = new RenderingContext();
        var sut = CreateSut(context);

        // Act
        Func<Task> act = () => TagHelperRenderer.RenderAsync(sut);

        // Assert
        await Assert.That(act).Throws<InvalidOperationException>();
    }

    [Test]
    public async Task ProcessAsync_WhenModelBound_UsesDisplayNameAsLabel()
    {
        // Arrange
        using var context = new RenderingContext();
        var sut = CreateSut(context);
        sut.For = CreateFor(context, new Booking(), nameof(Booking.LeadTraveller));

        // Act
        using var html = await TagHelperRenderer.RenderAsync(sut);

        // Assert
        await Assert
            .That(html.QuerySelector("[data-slot=display-field-label]")?.TextContent)
            .IsEqualTo("Lead traveller");
    }

    [Test]
    public async Task ProcessAsync_WhenLabelIsSet_OverridesDisplayName()
    {
        // Arrange
        using var context = new RenderingContext();
        var sut = CreateSut(context);
        sut.For = CreateFor(context, new Booking(), nameof(Booking.LeadTraveller));
        sut.Label = "Traveller";

        // Act
        using var html = await TagHelperRenderer.RenderAsync(sut);

        // Assert
        await Assert
            .That(html.QuerySelector("[data-slot=display-field-label]")?.TextContent)
            .IsEqualTo("Traveller");
    }

    private static DisplayFieldTagHelper CreateSut(
        RenderingContext context,
        params string[] templates
    )
    {
        return new DisplayFieldTagHelper(new FakeViewEngine(templates), context.Icons)
        {
            ViewContext = context.ViewContext,
        };
    }

    private static ModelExpression CreateFor(
        RenderingContext context,
        Booking booking,
        string propertyName
    )
    {
        return new ModelExpression(
            propertyName,
            context
                .Metadata.GetModelExplorerForType(typeof(Booking), booking)
                .GetExplorerForProperty(propertyName)
        );
    }

    private static IElement? Value(IHtmlDocument html)
    {
        return html.QuerySelector("[data-slot=display-field-value]");
    }

    internal sealed class Booking
    {
        [DisplayFormat(NullDisplayText = "Never")]
        public DateTime? ArchivedAt { get; init; }

        [DisplayFormat(DataFormatString = "{0:yyyy-MM-dd}")]
        public DateOnly Departure { get; init; }

        [EmailAddress]
        public string? Email { get; init; }

        [UIHint("StatusBadge")]
        public BookingStatus HintedStatus { get; init; }

        public int Infants { get; init; }

        [Display(Name = "Lead traveller")]
        public string? LeadTraveller { get; init; }

        [DataType(DataType.MultilineText)]
        public string? Notes { get; init; }

        public string? PromoCode { get; init; }

        public BookingStatus Status { get; init; }

        public IReadOnlyList<string> Tags { get; init; } = [];

        public bool TravelInsurance { get; init; }

        [Url]
        public string? Website { get; init; }
    }

    internal enum BookingStatus
    {
        [Display(Name = "On hold")]
        OnHold,

        Confirmed,
    }

    // Finds only the templates a test registers, by their DisplayTemplates/ name
    private sealed class FakeViewEngine(string[] templates) : ICompositeViewEngine
    {
        public IReadOnlyList<IViewEngine> ViewEngines => [];

        public ViewEngineResult FindView(ActionContext context, string viewName, bool isMainPage)
        {
            return templates.Any(template => $"DisplayTemplates/{template}" == viewName)
                ? ViewEngineResult.Found(viewName, new FakeView(viewName))
                : ViewEngineResult.NotFound(viewName, [viewName]);
        }

        public ViewEngineResult GetView(string? executingFilePath, string viewPath, bool isMainPage)
        {
            return ViewEngineResult.NotFound(viewPath, []);
        }
    }

    private sealed class FakeView(string path) : IView
    {
        public string Path => path;

        public async Task RenderAsync(ViewContext context)
        {
            await context.Writer.WriteAsync($"{path}:{context.ViewData.Model}");
        }
    }
}
