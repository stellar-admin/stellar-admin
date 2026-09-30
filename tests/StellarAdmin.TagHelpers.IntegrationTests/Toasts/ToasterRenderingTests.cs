using System.Net;
using StellarAdmin.TagHelpers.IntegrationTests.Infrastructure;

namespace StellarAdmin.TagHelpers.IntegrationTests.Toasts;

public class ToasterRenderingTests
{
    [Test]
    public async Task Navigation_RendersQueuedToastsIntoPage()
    {
        // Arrange
        await using var app = await ToastTestHost.CreateAsync();
        using var client = app.CreateClient();
        using var _ = await client.SendAsync(HttpMethod.Post, "/toasts/success", "navigate");

        // Act
        using var page = await client.SendAsync(HttpMethod.Get, "/Toaster", "navigate");

        // Assert
        await Assert.That(page.StatusCode).IsEqualTo(HttpStatusCode.OK);
        await Assert
            .That(await page.Content.ReadAsStringAsync())
            .Contains(
                """<script type="application/json">[{"title":"Booking saved","description":"Your trip to Lisbon is confirmed.","type":"success"}]</script>"""
            );
    }

    [Test]
    public async Task Navigation_DoesNotKeepRenderedToastsForNextResponse()
    {
        // Arrange
        await using var app = await ToastTestHost.CreateAsync();
        using var client = app.CreateClient();
        using var _ = await client.SendAsync(HttpMethod.Post, "/toasts/success", "navigate");
        using var page = await client.SendAsync(HttpMethod.Get, "/Toaster", "navigate");

        // Act
        using var next = await client.SendAsync(HttpMethod.Get, "/toasts/none", "cors");

        // Assert
        await Assert.That(next.GetToastHeader()).IsNull();
    }

    [Test]
    public async Task RedirectToPage_RendersToastOnTargetPage()
    {
        // Arrange
        await using var app = await ToastTestHost.CreateAsync();
        using var client = app.CreateClient();
        using var redirect = await client.SendAsync(
            HttpMethod.Post,
            "/Toaster?handler=Redirect",
            "navigate"
        );

        // Act
        using var page = await client.SendAsync(
            HttpMethod.Get,
            redirect.Headers.Location!.ToString(),
            "navigate"
        );

        // Assert
        await Assert.That(redirect.StatusCode).IsEqualTo(HttpStatusCode.Redirect);
        await Assert
            .That(await page.Content.ReadAsStringAsync())
            .Contains("""[{"title":"Booking saved","type":"success"}]""");
    }

    [Test]
    public async Task NoQueuedToasts_RendersNoScript()
    {
        // Arrange
        await using var app = await ToastTestHost.CreateAsync();
        using var client = app.CreateClient();

        // Act
        using var page = await client.SendAsync(HttpMethod.Get, "/Toaster", "navigate");

        // Assert
        await Assert.That(await page.Content.ReadAsStringAsync()).DoesNotContain("<script");
    }

    [Test]
    public async Task Html_IsEscapedInsideScript()
    {
        // Arrange
        await using var app = await ToastTestHost.CreateAsync();
        using var client = app.CreateClient();
        using var _ = await client.SendAsync(HttpMethod.Post, "/toasts/unicode", "navigate");

        // Act
        using var page = await client.SendAsync(HttpMethod.Get, "/Toaster", "navigate");

        // Assert
        var html = await page.Content.ReadAsStringAsync();
        await Assert.That(html).DoesNotContain("<b>");
        await Assert.That(html).Contains("\\u003Cb\\u003E");
    }
}
