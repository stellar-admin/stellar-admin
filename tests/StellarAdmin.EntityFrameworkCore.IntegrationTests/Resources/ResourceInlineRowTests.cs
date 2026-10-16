using AngleSharp.Dom;
using AngleSharp.Html.Parser;
using Microsoft.AspNetCore.TestHost;
using StellarAdmin.EntityFrameworkCore.IntegrationTests.Infrastructure;
using StellarAdmin.Resources;

namespace StellarAdmin.EntityFrameworkCore.IntegrationTests.Resources;

public class ResourceInlineRowTests
{
    [Test]
    public async Task EditRow_SaveKeepingInPlace_RendersRowThroughIndexQuery()
    {
        // Arrange
        await using var sut = await EfCoreTestHost.CreateAsync(resource =>
        {
            resource.Index(index =>
                index.Columns(columns =>
                    columns.Add(
                        product => product.Category!.Name,
                        column => column.Title = "Category"
                    )
                )
            );
            resource.AllowDelete();
            resource.AllowEdit(edit =>
            {
                edit.Fields(fields =>
                {
                    fields.Add(product => product.Name);
                    fields.Add(product => product.Price);
                });
                edit.UseInlineRow(inline => inline.AfterSave = InlineRowAfterSave.KeepInPlace);
            });
        });
        using var client = sut.GetTestClient();
        var values = await PrepareForm(client, "/stellaradmin/products");
        values["rows[2].Name"] = "Apple pie";
        values["rows[2].Price"] = "25";
        using var content = new FormUrlEncodedContent(values);

        // Act
        using var response = await client.PostAsync("/stellaradmin/products/editrow/2", content);

        // Assert
        var rows = (await ReadRows(response)).QuerySelectorAll("tbody > tr");
        await Assert.That(rows.Length).IsEqualTo(1);
        await Assert.That(rows[0].GetAttribute("data-key")).IsEqualTo("2");
        await Assert
            .That(rows[0].QuerySelectorAll("td").Take(4).Select(cell => cell.TextContent.Trim()))
            .IsEquivalentTo(["2", "Apple pie", "25", "Technology"]);
    }

    private static async Task<Dictionary<string, string>> PrepareForm(HttpClient client, string url)
    {
        using var response = await client.GetAsync(url);
        response.EnsureSuccessStatusCode();
        var document = await new HtmlParser().ParseDocumentAsync(
            await response.Content.ReadAsStringAsync()
        );
        var token = document
            .QuerySelector("input[name='__RequestVerificationToken']")!
            .GetAttribute("value")!;
        client.DefaultRequestHeaders.Add(
            "Cookie",
            response.Headers.GetValues("Set-Cookie").Select(cookie => cookie.Split(';')[0])
        );

        return new() { ["__RequestVerificationToken"] = token };
    }

    // A response of table rows parses inside a table body, where the rows belong
    private static async Task<IDocument> ReadRows(HttpResponseMessage response) =>
        await new HtmlParser().ParseDocumentAsync(
            $"<table><tbody>{await response.Content.ReadAsStringAsync()}</tbody></table>"
        );
}
