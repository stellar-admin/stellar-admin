using AngleSharp.Html.Dom;
using AngleSharp.Html.Parser;

namespace StellarAdmin.Scenarios.Tests.Infrastructure;

internal static class HtmlTestExtensions
{
    extension(HttpClient client)
    {
        public async Task<IHtmlDocument> GetDocumentAsync(string url)
        {
            using var response = await client.GetAsync(url);
            response.EnsureSuccessStatusCode();

            return await new HtmlParser().ParseDocumentAsync(
                await response.Content.ReadAsStringAsync()
            );
        }
    }
}
