using System.Net;
using System.Text.Json;
using StellarAdmin.TagHelpers.IntegrationTests.Infrastructure;

namespace StellarAdmin.TagHelpers.IntegrationTests.Toasts;

public class ToastDeliveryTests
{
    [Test]
    [Arguments("cors")]
    [Arguments("same-origin")]
    [Arguments("no-cors")]
    public async Task FetchResponse_SendsToastsInHeader(string secFetchMode)
    {
        // Arrange
        await using var app = await ToastTestHost.CreateAsync();
        using var client = app.CreateClient();

        // Act
        using var response = await client.SendAsync(
            HttpMethod.Post,
            "/toasts/success",
            secFetchMode
        );

        // Assert
        await Assert.That(response.StatusCode).IsEqualTo(HttpStatusCode.OK);
        await Assert
            .That(response.GetToastHeader())
            .IsEqualTo(
                """[{"title":"Booking saved","description":"Your trip to Lisbon is confirmed.","type":"success"}]"""
            );
    }

    [Test]
    public async Task FetchResponse_DoesNotKeepHeaderToastsForNextResponse()
    {
        // Arrange
        await using var app = await ToastTestHost.CreateAsync();
        using var client = app.CreateClient();
        using var first = await client.SendAsync(HttpMethod.Post, "/toasts/success", "cors");

        // Act
        using var next = await client.SendAsync(HttpMethod.Get, "/toasts/none", "cors");

        // Assert
        await Assert.That(first.GetToastHeader()).IsNotNull();
        await Assert.That(next.GetToastHeader()).IsNull();
    }

    [Test]
    [Arguments("navigate")]
    [Arguments("cors")]
    public async Task Redirect_KeepsToastsForNextResponse(string secFetchMode)
    {
        // Arrange
        await using var app = await ToastTestHost.CreateAsync();
        using var client = app.CreateClient();
        using var redirect = await client.SendAsync(
            HttpMethod.Post,
            "/toasts/redirect",
            secFetchMode
        );

        // Act
        using var next = await client.SendAsync(HttpMethod.Get, "/toasts/none", "cors");

        // Assert
        await Assert.That(redirect.StatusCode).IsEqualTo(HttpStatusCode.Redirect);
        await Assert.That(redirect.GetToastHeader()).IsNull();
        await Assert
            .That(next.GetToastHeader())
            .IsEqualTo("""[{"title":"Booking saved","type":"success"}]""");
    }

    [Test]
    [Arguments("navigate")]
    [Arguments(null)]
    public async Task Navigation_KeepsToastsInTempData(string? secFetchMode)
    {
        // Arrange
        await using var app = await ToastTestHost.CreateAsync();
        using var client = app.CreateClient();
        using var navigation = await client.SendAsync(
            HttpMethod.Post,
            "/toasts/success",
            secFetchMode
        );

        // Act
        using var next = await client.SendAsync(HttpMethod.Get, "/toasts/none", "cors");

        // Assert
        await Assert.That(navigation.GetToastHeader()).IsNull();
        await Assert.That(ReadTitles(next.GetToastHeader())).IsEquivalentTo(["Booking saved"]);
    }

    [Test]
    public async Task DetailedToast_SerializesDurationInMillisecondsAndLinkAction()
    {
        // Arrange
        await using var app = await ToastTestHost.CreateAsync();
        using var client = app.CreateClient();

        // Act
        using var response = await client.SendAsync(HttpMethod.Post, "/toasts/detailed", "cors");

        // Assert
        await Assert
            .That(response.GetToastHeader())
            .IsEqualTo(
                """[{"title":"Itinerary archived","description":"3 bookings moved to archive.","type":"info","duration":8000,"action":{"label":"Undo","href":"/itineraries/42/restore"}}]"""
            );
    }

    [Test]
    public async Task NonAsciiAndHtml_AreEscapedToAsciiJson()
    {
        // Arrange
        await using var app = await ToastTestHost.CreateAsync();
        using var client = app.CreateClient();

        // Act
        using var response = await client.SendAsync(HttpMethod.Post, "/toasts/unicode", "cors");

        // Assert
        var header = response.GetToastHeader();
        await Assert.That(header!.All(char.IsAscii)).IsTrue();
        await Assert.That(ReadTitles(header)).IsEquivalentTo(["Café in Zürich ✓"]);
        await Assert.That(header).DoesNotContain("<b>");
    }

    [Test]
    public async Task ToastsOverHeaderBudget_OverflowToNextResponseInOrder()
    {
        // Arrange
        await using var app = await ToastTestHost.CreateAsync();
        using var client = app.CreateClient();
        using var first = await client.SendAsync(
            HttpMethod.Post,
            "/toasts/many?count=6&descriptionLength=1000",
            "cors"
        );

        // Act
        using var next = await client.SendAsync(HttpMethod.Get, "/toasts/none", "cors");

        // Assert
        await Assert.That(first.GetToastHeader()!.Length).IsLessThanOrEqualTo(4096);
        await Assert
            .That(ReadTitles(first.GetToastHeader()))
            .IsEquivalentTo(
                ["Toast 1", "Toast 2", "Toast 3"],
                TUnit.Assertions.Enums.CollectionOrdering.Matching
            );
        await Assert
            .That(ReadTitles(next.GetToastHeader()))
            .IsEquivalentTo(
                ["Toast 4", "Toast 5", "Toast 6"],
                TUnit.Assertions.Enums.CollectionOrdering.Matching
            );
    }

    [Test]
    public async Task ToastOverHeaderBudget_IsStillSent()
    {
        // Arrange
        await using var app = await ToastTestHost.CreateAsync();
        using var client = app.CreateClient();

        // Act
        using var response = await client.SendAsync(
            HttpMethod.Post,
            "/toasts/many?count=1&descriptionLength=5000",
            "cors"
        );

        // Assert
        await Assert.That(ReadTitles(response.GetToastHeader())).IsEquivalentTo(["Toast 1"]);
    }

    [Test]
    public async Task RazorPageHandler_SendsToastsInHeader()
    {
        // Arrange
        await using var app = await ToastTestHost.CreateAsync();
        using var client = app.CreateClient();

        // Act
        using var response = await client.SendAsync(
            HttpMethod.Post,
            "/Bookings?handler=Save",
            "cors"
        );

        // Assert
        await Assert
            .That(response.GetToastHeader())
            .IsEqualTo("""[{"title":"Booking saved","type":"success"}]""");
    }

    [Test]
    public async Task RazorPageRedirect_KeepsToastsForNextResponse()
    {
        // Arrange
        await using var app = await ToastTestHost.CreateAsync();
        using var client = app.CreateClient();
        using var redirect = await client.SendAsync(
            HttpMethod.Post,
            "/Bookings?handler=Redirect",
            "navigate"
        );

        // Act
        using var next = await client.SendAsync(HttpMethod.Get, "/Bookings", "cors");

        // Assert
        await Assert.That(redirect.StatusCode).IsEqualTo(HttpStatusCode.Redirect);
        await Assert.That(redirect.GetToastHeader()).IsNull();
        await Assert.That(ReadTitles(next.GetToastHeader())).IsEquivalentTo(["Booking saved"]);
    }

    private static List<string> ReadTitles(string? header)
    {
        if (header is null)
        {
            return [];
        }

        using var document = JsonDocument.Parse(header);

        return document
            .RootElement.EnumerateArray()
            .Select(toast => toast.GetProperty("title").GetString()!)
            .ToList();
    }
}
