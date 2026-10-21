using Microsoft.AspNetCore.TestHost;
using StellarAdmin.IntegrationTests.Infrastructure;
using StellarAdmin.Resources.Displays;

namespace StellarAdmin.IntegrationTests.Resources;

public class HtmlDisplayTests
{
    private const string Html =
        "<h2>Kyoto</h2><p>Walk <strong>Gion</strong> at dusk.</p><ul><li><p>Tea</p></li><li><p>Temples</p></li></ul><script>alert(1)</script>";

    [Test]
    public async Task Index_WithHtmlDisplay_ShowsClampedTextWithoutMarkup()
    {
        // Arrange
        await using var sut = await StellarAdminTestHost.CreateAsync(
            new([new(7, Html, 8.50m)]),
            resource =>
                resource.Index(index =>
                    index.Columns(columns =>
                        columns
                            .Add(product => product.Name)
                            .UseDisplay<HtmlDisplay>(display => display.LineClamp = 3)
                    )
                )
        );
        using var client = sut.GetTestClient();

        // Act
        var document = await client.GetDocumentAsync("/stellaradmin/products");

        // Assert
        var cell = document.RequiredElement("td .sa-text-display[data-line-clamp]");
        await Assert.That(cell.TextContent).IsEqualTo("Kyoto Walk Gion at dusk. Tea Temples");
        await Assert.That(cell.GetAttribute("data-line-clamp")).IsEqualTo("3");
        await Assert.That(cell.QuerySelector("strong")).IsNull();
    }

    [Test]
    public async Task View_WithHtmlDisplay_ShowsSanitizedFormattedText()
    {
        // Arrange
        await using var sut = await StellarAdminTestHost.CreateAsync(
            new([new(7, Html, 8.50m)]),
            resource =>
            {
                resource.UseKey(product => product.Id);
                resource.AllowView(view =>
                    view.Fields(fields =>
                        fields.Add(product => product.Name).UseDisplay<HtmlDisplay>()
                    )
                );
            }
        );
        using var client = sut.GetTestClient();

        // Act
        var document = await client.GetDocumentAsync("/stellaradmin/products/view/7");

        // Assert
        var display = document.RequiredElement(".sa-rich-text");
        await Assert
            .That(display.InnerHtml)
            .IsEqualTo(
                "<h2>Kyoto</h2><p>Walk <strong>Gion</strong> at dusk.</p><ul><li><p>Tea</p></li><li><p>Temples</p></li></ul>"
            );
    }
}
