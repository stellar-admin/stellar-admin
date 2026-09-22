using System.Collections.Concurrent;
using System.Net;
using AngleSharp.Html.Parser;
using Microsoft.AspNetCore.TestHost;
using StellarAdmin.Dashboard.EntityFrameworkCore.IntegrationTests.Infrastructure;

namespace StellarAdmin.Dashboard.EntityFrameworkCore.IntegrationTests.Resources;

public class ResourceSortingTests
{
    [Test]
    [Arguments("?page=2", "Apple,Banana")]
    [Arguments("?page=2&sortBy=Name&sortDirection=desc", "Apple,Apple")]
    [Arguments("?page=2&scope=over-10&search=Apple&pageSize=1", "Apple")]
    public async Task SortExpression_OrdersInDatabaseBeforePaging(
        string query,
        string expectedNames
    )
    {
        // Arrange
        var commands = new ConcurrentQueue<string>();
        await using var sut = await EfCoreTestHost.CreateAsync(
            resource =>
                resource.Index(index =>
                {
                    index.Columns(columns =>
                    {
                        columns.Clear();
                        columns.Add(product => product.Number);
                        columns
                            .Add(product => product.Name)
                            .Sortable(product => product.Name.Length);
                    });
                    index.DefaultSortBy(product => product.Name);
                    index.EnableSearch(term => product => product.Name.Contains(term));
                    index.EnableScopes(scopes =>
                        scopes.Add("over-10", "Over 10", product => product.Price > 10)
                    );
                    index.EnablePaging(paging =>
                    {
                        paging.PageSize = 2;
                        paging.PageSizes = [1, 2];
                    });
                }),
            commands
        );
        using var client = sut.GetTestClient();

        // Act
        using var response = await client.GetAsync("/stellaradmin/Product" + query);
        var document = await new HtmlParser().ParseDocumentAsync(
            await response.Content.ReadAsStringAsync()
        );

        // Assert
        await Assert.That(response.StatusCode).IsEqualTo(HttpStatusCode.OK);
        await Assert
            .That(
                string.Join(
                    ',',
                    document
                        .QuerySelectorAll("tbody tr td:nth-child(2)")
                        .Select(cell => cell.TextContent.Trim())
                )
            )
            .IsEqualTo(expectedNames);
        await Assert.That(document.QuerySelector("thead a[href*='sortBy=Name']")).IsNotNull();
        await Assert
            .That(
                commands.Any(sql =>
                    sql.Contains("ORDER BY length(")
                    && sql.Contains(", \"p\".\"Number\"")
                    && sql.Contains("LIMIT")
                )
            )
            .IsTrue();
        await Assert
            .That(document.QuerySelector("tbody td")!.TextContent.Trim())
            .IsEqualTo(query.Contains("sortDirection=desc") ? "2" : "3");
    }

    [Test]
    [Arguments(false, "4,1,2,3", "Banana")]
    [Arguments(true, "1,4,2,3", "Zebra")]
    public async Task RepeatedSortable_UsesLastConfiguration(
        bool useField,
        string expectedKeys,
        string expectedName
    )
    {
        // Arrange
        await using var sut = await EfCoreTestHost.CreateAsync(resource =>
            resource.Index(index =>
                index.Columns(columns =>
                {
                    columns.Clear();
                    columns.Add(product => product.Number);
                    columns.Add(
                        product => product.Name,
                        column =>
                        {
                            column
                                .Sortable(product => product.Price)
                                .Sortable(product => product.Name.Length);
                            if (useField)
                            {
                                column.Sortable();
                            }
                        }
                    );
                })
            )
        );
        using var client = sut.GetTestClient();

        // Act
        using var response = await client.GetAsync(
            "/stellaradmin/Product?sortBy=Name&sortDirection=desc"
        );
        var document = await new HtmlParser().ParseDocumentAsync(
            await response.Content.ReadAsStringAsync()
        );

        // Assert
        await Assert.That(response.StatusCode).IsEqualTo(HttpStatusCode.OK);
        await Assert
            .That(
                string.Join(
                    ',',
                    document
                        .QuerySelectorAll("tbody tr td:first-child")
                        .Select(cell => cell.TextContent.Trim())
                )
            )
            .IsEqualTo(expectedKeys);
        await Assert
            .That(document.QuerySelector("tbody td:nth-child(2)")!.TextContent.Trim())
            .IsEqualTo(expectedName);
    }
}
