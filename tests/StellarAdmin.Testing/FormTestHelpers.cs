namespace StellarAdmin.Testing;

public static class FormTestHelpers
{
    // Reads the antiforgery token of the form at the URL. The client must carry cookies (see
    // TestClientExtensions.CreateClient), so the matching antiforgery cookie goes with the post.
    public static async Task<Dictionary<string, string>> PrepareForm(
        HttpClient client,
        string url = "/stellaradmin/products/create"
    )
    {
        var document = await client.GetDocumentAsync(url);
        var token = document
            .RequiredElement("input[name='__RequestVerificationToken']")
            .GetAttribute("value")!;

        return new() { ["__RequestVerificationToken"] = token };
    }
}
