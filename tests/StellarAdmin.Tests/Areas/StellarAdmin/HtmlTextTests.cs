using StellarAdmin.Areas.StellarAdmin;

namespace StellarAdmin.Tests.Areas.StellarAdmin;

public class HtmlTextTests
{
    [Test]
    [Arguments(null, "")]
    [Arguments("  ", "")]
    [Arguments("<p>One</p><p>Two</p>", "One Two")]
    [Arguments("<h2>Title</h2><ul><li><p>A</p></li><li><p>B</p></li></ul>", "Title A B")]
    [Arguments("<p>Line<br>break</p>", "Line break")]
    [Arguments("<table><tr><th>Day</th><td>Stop</td></tr></table>", "Day Stop")]
    [Arguments("<p>Walk <strong>Gion</strong> at <em>dusk</em>.</p>", "Walk Gion at dusk.")]
    [Arguments("<p>5 &gt; 3 &amp; done</p>", "5 > 3 & done")]
    [Arguments("<p>Safe</p><script>alert(1)</script><style>p{}</style>", "Safe")]
    [Arguments("Plain text\n\nwith   spaces", "Plain text with spaces")]
    public async Task ToPlainText_WithHtml_ReturnsTextWithSpacesBetweenBlocks(
        string? html,
        string expected
    )
    {
        // Arrange

        // Act
        var text = HtmlText.ToPlainText(html);

        // Assert
        await Assert.That(text).IsEqualTo(expected);
    }
}
