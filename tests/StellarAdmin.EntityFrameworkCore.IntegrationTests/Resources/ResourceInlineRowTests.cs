using StellarAdmin.EntityFrameworkCore.IntegrationTests.Infrastructure;
using StellarAdmin.Resources;
using StellarAdmin.Testing;
using static StellarAdmin.Testing.FormTestHelpers;

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
                index.AddTable(
                    "table",
                    table =>
                    {
                        table.Columns(columns =>
                            columns.Add(
                                product => product.Category!.Name,
                                column => column.Title = "Category"
                            )
                        );
                        table.EnableInlineEditing(inline =>
                            inline.AfterSave = InlineRowAfterSave.KeepInPlace
                        );
                    }
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
            });
        });
        using var client = sut.CreateClient();
        var values = await PrepareForm(client, "/stellaradmin/products");
        values["rows[2].Name"] = "Apple pie";
        values["rows[2].Price"] = "25";
        using var content = new FormUrlEncodedContent(values);

        // Act
        using var response = await client.PostAsync("/stellaradmin/products/editrow/2", content);

        // Assert
        var rows = (await response.ReadRowsAsync()).QuerySelectorAll("tbody > tr");
        await Assert.That(rows.Length).IsEqualTo(1);
        await Assert.That(rows[0].GetAttribute("data-key")).IsEqualTo("2");
        await Assert
            .That(rows[0].QuerySelectorAll("td").Take(4).Select(cell => cell.TextContent.Trim()))
            .IsEquivalentTo(["2", "Apple pie", "25", "Technology"]);
    }
}
