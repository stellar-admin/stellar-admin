using System.Net;
using Microsoft.AspNetCore.TestHost;
using StellarAdmin.Dashboard.IntegrationTests.Fixtures;
using StellarAdmin.Dashboard.IntegrationTests.Infrastructure;
using StellarAdmin.Dashboard.Resources;
using StellarAdmin.Dashboard.Resources.Builders;
using static StellarAdmin.Dashboard.IntegrationTests.Infrastructure.FormTestHelpers;

namespace StellarAdmin.Dashboard.IntegrationTests.Resources;

public class ResourceSearchTests
{
    [Test]
    [Arguments("  LAMP  ", "LAMP", "Blue lamp", "1–2 of 3")]
    [Arguments("   ", null, "Blue lamp", "1–2 of 4")]
    [Arguments("missing", "missing", "No records found", "0 records")]
    public async Task Search_FiltersBeforeCountingAndPaging(
        string term,
        string? expectedSearch,
        string? firstName,
        string range
    )
    {
        // Arrange
        var state = CreateState();
        await using var sut = await DashboardTestHost.CreateAsync(state, ConfigureSearch);
        using var client = sut.GetTestClient();

        // Act
        var document = await client.GetDocumentAsync(
            "/stellaradmin/Product?search=" + Uri.EscapeDataString(term)
        );

        // Assert
        await Assert.That(state.ListRequests.Single().Search).IsEqualTo(expectedSearch);
        await Assert
            .That(document.QuerySelector("tbody td")?.TextContent.Trim())
            .IsEqualTo(firstName);
        await Assert
            .That(document.RequiredElement("[data-slot='data-grid-range']").TextContent.Trim())
            .IsEqualTo(range);
        await Assert
            .That(document.RequiredElement("input[name='search']").GetAttribute("value"))
            .IsEqualTo(expectedSearch ?? "");
    }

    [Test]
    public async Task DisabledSearch_IgnoresTermAndHidesInput()
    {
        // Arrange
        var state = CreateState();
        await using var sut = await DashboardTestHost.CreateAsync(state);
        using var client = sut.GetTestClient();

        // Act
        var document = await client.GetDocumentAsync("/stellaradmin/Product?search=missing");

        // Assert
        await Assert.That(state.ListRequests.Single().Search).IsNull();
        await Assert.That(document.QuerySelectorAll("tbody tr").Length).IsEqualTo(4);
        await Assert.That(document.QuerySelector("input[name='search']")).IsNull();
    }

    [Test]
    [Arguments(false, false, "Search Products...")]
    [Arguments(true, false, "Find Products")]
    [Arguments(true, true, "Find a lamp")]
    public async Task SearchPlaceholder_UsesGlobalDefaultAndLocalOverride(
        bool global,
        bool local,
        string expected
    )
    {
        // Arrange
        await using var sut = await DashboardTestHost.CreateAsync(
            CreateState(),
            resource =>
                resource.Index(index =>
                {
                    index.EnableSearch(search => search.Placeholder = local ? "Find a lamp" : null);
                }),
            dashboard =>
            {
                if (global)
                {
                    dashboard.ConfigureResourceLabels(labels =>
                        labels.IndexSearchPlaceholder = resource => $"Find {resource.PluralLabel}"
                    );
                }
            }
        );
        using var client = sut.GetTestClient();

        // Act
        var document = await client.GetDocumentAsync("/stellaradmin/Product");

        // Assert
        var input = document.RequiredElement("input[name='search']");
        await Assert.That(input.GetAttribute("placeholder")).IsEqualTo(expected);
        await Assert.That(input.GetAttribute("aria-label")).IsEqualTo(expected);
    }

    [Test]
    public async Task SearchControls_ResetPageAndPreserveExplicitChoices()
    {
        // Arrange
        var state = CreateState();
        await using var sut = await DashboardTestHost.CreateAsync(state, ConfigureSearch);
        using var client = sut.GetTestClient();

        // Act
        var document = await client.GetDocumentAsync(
            "/stellaradmin/Product?page=2&pageSize=2&sortBy=Name&sortDirection=desc&search=lamp"
        );
        var target = document.RequiredElement("input[name='search']").GetAttribute("hx-get")!;
        var cleared = await client.GetDocumentAsync(target + "&search=");

        // Assert
        await Assert
            .That(target)
            .IsEqualTo("/stellaradmin/Product?sortBy=Name&sortDirection=desc&pageSize=2");
        await Assert.That(state.ListRequests.Last().Search).IsNull();
        await Assert.That(state.ListRequests.Last().Paging).IsEqualTo(new ResourcePaging(1, 2));
        await Assert
            .That(state.ListRequests.Last().Sort)
            .IsEqualTo(new ResourceSort("Name", ResourceSortDirection.Descending));
        await Assert
            .That(cleared.RequiredElement("[data-slot='data-grid-range']").TextContent)
            .Contains("of 4");
        foreach (
            var link in document.QuerySelectorAll(
                "[data-slot='data-grid-pager'] a[href], [data-slot='data-grid-page-size'] a[href], [data-slot='data-grid-sort-link']"
            )
        )
        {
            await Assert.That(link.GetAttribute("href")).Contains("search=lamp");
        }
    }

    [Test]
    [Arguments(false)]
    [Arguments(true)]
    public async Task Delete_PreservesSearch(bool rejected)
    {
        // Arrange
        var state = CreateState();
        if (rejected)
        {
            state.DeleteResult = ResourceOperationResult.ValidationFailed(null, "Still in use.");
        }

        await using var sut = await DashboardTestHost.CreateAsync(
            state,
            resource =>
            {
                ConfigureSearch(resource);
                resource.UseKey(product => product.Id).AllowDelete();
            }
        );
        using var client = sut.GetTestClient();
        const string url = "/stellaradmin/Product?search=lamp";
        var document = await client.GetDocumentAsync(url);
        var action = document.RequiredElement("form[data-resource-delete]").GetAttribute("action")!;
        using var content = new FormUrlEncodedContent(await PrepareForm(client, url));

        // Act
        using var response = await client.PostAsync(action, content);

        // Assert
        await Assert.That(action).IsEqualTo("/stellaradmin/Product/Delete/1?search=lamp");
        if (rejected)
        {
            var result = await response.ReadDocumentAsync();
            await Assert
                .That(result.RequiredElement("input[name='search']").GetAttribute("value"))
                .IsEqualTo("lamp");
            await Assert
                .That(result.RequiredElement(".validation-summary-errors").TextContent)
                .Contains("Still in use.");
            await Assert.That(state.ListRequests.Last().Search).IsEqualTo("lamp");
        }
        else
        {
            await Assert
                .That(response.Headers.Location?.OriginalString)
                .IsEqualTo("/stellaradmin/Product?search=lamp");
        }
    }

    [Test]
    public async Task OutOfRangePage_PreservesSearchWithoutAddingDefaults()
    {
        // Arrange
        await using var sut = await DashboardTestHost.CreateAsync(CreateState(), ConfigureSearch);
        using var client = sut.GetTestClient();

        // Act
        using var response = await client.GetAsync("/stellaradmin/Product?page=9&search=lamp");

        // Assert
        await Assert.That(response.StatusCode).IsEqualTo(HttpStatusCode.Redirect);
        await Assert
            .That(response.Headers.Location?.OriginalString)
            .IsEqualTo("/stellaradmin/Product?page=2&search=lamp");
    }

    private static void ConfigureSearch(ResourceBuilder<Product> resource) =>
        resource.Index(index =>
        {
            index.EnableSearch();
            index.Columns(columns =>
                columns.Clear().Add(product => product.Name, column => column.Sortable())
            );
            index.DefaultSortBy(product => product.Name);
            index.EnablePaging(paging =>
            {
                paging.PageSize = 2;
                paging.PageSizes = [2, 3];
            });
        });

    private static ProductState CreateState() =>
        new([
            new Product(1, "Blue lamp", 1),
            new Product(2, "Desk", 2),
            new Product(3, "Green lamp", 3),
            new Product(4, "Red lamp", 4),
        ]);
}
