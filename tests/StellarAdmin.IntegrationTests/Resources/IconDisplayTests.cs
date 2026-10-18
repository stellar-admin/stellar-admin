using AngleSharp.Dom;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.TestHost;
using StellarAdmin.IntegrationTests.Fixtures;
using StellarAdmin.IntegrationTests.Infrastructure;
using StellarAdmin.Resources.Displays;

namespace StellarAdmin.IntegrationTests.Resources;

public class IconDisplayTests
{
    [Test]
    public async Task Index_WithIconDisplay_RendersIconWithNameOnHover()
    {
        // Arrange
        await using var sut = await CreateGridAsync(null);
        using var client = sut.GetTestClient();

        // Act
        var document = await client.GetDocumentAsync("/stellaradmin/products");

        // Assert
        var display = Display(document, "wifi");
        await Assert.That(display.GetAttribute("title")).IsEqualTo("wifi");
        await Assert.That(display.QuerySelector("svg")).IsNotNull();
        await Assert.That(display.RequiredElement("span").ClassList.Contains("sr-only")).IsTrue();
    }

    [Test]
    public async Task Index_WithShowName_RendersNameBesideIcon()
    {
        // Arrange
        await using var sut = await CreateGridAsync(true);
        using var client = sut.GetTestClient();

        // Act
        var document = await client.GetDocumentAsync("/stellaradmin/products");

        // Assert
        var display = Display(document, "wifi");
        await Assert.That(display.HasAttribute("title")).IsFalse();
        var name = display.RequiredElement("span");
        await Assert.That(name.ClassList.Contains("sr-only")).IsFalse();
        await Assert.That(name.TextContent).IsEqualTo("wifi");
    }

    [Test]
    public async Task Index_WithUnregisteredName_RendersDashedBox()
    {
        // Arrange
        await using var sut = await CreateGridAsync(null);
        using var client = sut.GetTestClient();

        // Act
        var document = await client.GetDocumentAsync("/stellaradmin/products");

        // Assert
        var display = Display(document, "spa-flower");
        await Assert.That(display.GetAttribute("title")).IsEqualTo("spa-flower");
        await Assert.That(display.QuerySelector("svg")).IsNull();
        await Assert.That(display.QuerySelector(".sa-icon-missing")).IsNotNull();
    }

    [Test]
    [Arguments(null, false)]
    [Arguments(false, true)]
    public async Task View_WithIconDisplay_ShowsNameByDefault(bool? showName, bool hidden)
    {
        // Arrange
        await using var sut = await StellarAdminTestHost.CreateAsync(
            new([new(7, "wifi", 8.50m)]),
            resource =>
            {
                resource.UseKey(product => product.Id);
                resource.AllowView(view =>
                    view.Fields(fields =>
                        fields
                            .Add(product => product.Name)
                            .UseDisplay<IconDisplay>(icon => icon.ShowName = showName)
                    )
                );
            }
        );
        using var client = sut.GetTestClient();

        // Act
        var document = await client.GetDocumentAsync("/stellaradmin/products/view/7");

        // Assert
        var display = document.RequiredElement(".sa-icon-display");
        await Assert.That(display.QuerySelector("svg")).IsNotNull();
        await Assert
            .That(display.RequiredElement("span").ClassList.Contains("sr-only"))
            .IsEqualTo(hidden);
        await Assert.That(display.HasAttribute("title")).IsEqualTo(hidden);
    }

    // The display of the row whose value is the name
    private static IElement Display(IDocument document, string name) =>
        document
            .QuerySelectorAll(".sa-icon-display")
            .Single(display => display.LastElementChild?.TextContent == name);

    private static Task<WebApplication> CreateGridAsync(bool? showName) =>
        StellarAdminTestHost.CreateAsync(
            new([new(7, "wifi", 8.50m), new(8, "spa-flower", 9.50m)]),
            resource =>
            {
                resource.UseKey(product => product.Id);
                resource.Index(index =>
                    index.Columns(columns =>
                    {
                        columns.Clear();
                        columns.Add(
                            product => product.Name,
                            column =>
                                column.UseDisplay<IconDisplay>(icon => icon.ShowName = showName)
                        );
                    })
                );
            }
        );
}
