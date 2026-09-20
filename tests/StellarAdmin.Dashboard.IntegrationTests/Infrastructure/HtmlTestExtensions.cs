using AngleSharp.Dom;
using AngleSharp.Html.Dom;
using AngleSharp.Html.Parser;

namespace StellarAdmin.Dashboard.IntegrationTests.Infrastructure;

internal static class HtmlTestExtensions
{
    extension(HttpClient client)
    {
        public async Task<IHtmlDocument> GetDocumentAsync(string url)
        {
            using var response = await client.GetAsync(url);
            response.EnsureSuccessStatusCode();

            return await response.ReadDocumentAsync();
        }
    }

    extension(HttpResponseMessage response)
    {
        public async Task<IHtmlDocument> ReadDocumentAsync()
        {
            return await new HtmlParser().ParseDocumentAsync(
                await response.Content.ReadAsStringAsync()
            );
        }
    }

    extension(IParentNode node)
    {
        public IElement RequiredElement(string selector)
        {
            return node.QuerySelector(selector)
                ?? throw new InvalidOperationException(
                    $"Expected an HTML element matching '{selector}', but none was found."
                );
        }

        public string[] TextContents(string selector)
        {
            return node.QuerySelectorAll(selector)
                .Select(element => element.TextContent.Trim())
                .ToArray();
        }
    }
}
