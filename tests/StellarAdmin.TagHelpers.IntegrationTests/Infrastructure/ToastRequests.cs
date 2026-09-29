namespace StellarAdmin.TagHelpers.IntegrationTests.Infrastructure;

internal static class ToastRequests
{
    public const string HeaderName = "SA-Toasts";

    // secFetchMode is the value browsers send; null sends no Sec-Fetch-Mode header, like an older client.
    public static Task<HttpResponseMessage> SendAsync(
        this HttpClient client,
        HttpMethod method,
        string url,
        string? secFetchMode
    )
    {
        var request = new HttpRequestMessage(method, url);
        if (secFetchMode is not null)
        {
            request.Headers.Add("Sec-Fetch-Mode", secFetchMode);
        }

        return client.SendAsync(request);
    }

    public static string? GetToastHeader(this HttpResponseMessage response) =>
        response.Headers.TryGetValues(HeaderName, out var values) ? string.Join(",", values) : null;
}
