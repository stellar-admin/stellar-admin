namespace StellarAdmin.Dashboard.IntegrationTests.Infrastructure;

internal static class FormTestHelpers
{
    public static async Task<Dictionary<string, string>> PrepareForm(
        HttpClient client,
        string url = "/stellaradmin/Product/Create"
    )
    {
        using var response = await client.GetAsync(url);
        response.EnsureSuccessStatusCode();
        var document = await response.ReadDocumentAsync();
        var token = document
            .RequiredElement("input[name='__RequestVerificationToken']")
            .GetAttribute("value")!;
        client.DefaultRequestHeaders.Add(
            "Cookie",
            response.Headers.GetValues("Set-Cookie").Select(cookie => cookie.Split(';')[0])
        );

        return new() { ["__RequestVerificationToken"] = token };
    }
}
