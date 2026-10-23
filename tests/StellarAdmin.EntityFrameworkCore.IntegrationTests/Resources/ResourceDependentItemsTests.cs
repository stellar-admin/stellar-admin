using System.Net;
using AngleSharp.Dom;
using AngleSharp.Html.Parser;
using Microsoft.AspNetCore.TestHost;
using StellarAdmin.EntityFrameworkCore.IntegrationTests.Fixtures;
using StellarAdmin.EntityFrameworkCore.IntegrationTests.Infrastructure;
using StellarAdmin.Resources.Editors;
using TUnit.Assertions.Enums;

namespace StellarAdmin.EntityFrameworkCore.IntegrationTests.Resources;

// Items filtered by the form's values: categories whose parent is the product's number
public class ResourceDependentItemsTests
{
    [Test]
    public async Task ChoiceWhere_ReadsTheForm_FiltersTheChoices()
    {
        // Arrange
        await using var sut = await EfCoreTestHost.CreateAsync(resource =>
            resource.AllowEdit(edit =>
                edit.Fields(fields =>
                {
                    fields.Add(product => product.Number);
                    fields
                        .Add(product => product.CategoryId)
                        .DependsOn(product => product.Number)
                        .UseSelectEditor(select =>
                            select.UseItems<Category>(
                                category => category.Id,
                                category => category.Name,
                                items =>
                                    items.Where(
                                        (Category category, Product product) =>
                                            category.ParentId == product.Number
                                    )
                            )
                        );
                })
            )
        );
        using var client = sut.GetTestClient();

        // Act
        using var response = await client.GetAsync("/stellaradmin/products/edit/1");
        var document = await ReadDocument(response);

        // Assert
        await Assert.That(response.StatusCode).IsEqualTo(HttpStatusCode.OK);
        await Assert
            .That(
                document
                    .QuerySelectorAll("select[name='Entity.CategoryId'] option")
                    .Select(option => option.TextContent.Trim())
            )
            .IsEquivalentTo(["Not set", "Technology"], CollectionOrdering.Matching);
    }

    [Test]
    public async Task LookupWhere_ReadsTheSentValues_FiltersTheSearch()
    {
        // Arrange
        await using var sut = await EfCoreTestHost.CreateAsync(resource =>
            resource.AllowEdit(edit =>
                edit.Fields(fields =>
                {
                    fields.Add(product => product.Number);
                    fields
                        .Add(product => product.CategoryId)
                        .DependsOn(product => product.Number)
                        .UseLookupSheetEditor(lookup =>
                            lookup.UseItems<Category>(
                                category => category.Id,
                                category => category.Name,
                                items =>
                                    items.Where(
                                        (Category category, Product product) =>
                                            category.ParentId == product.Number
                                    )
                            )
                        );
                })
            )
        );
        using var client = sut.GetTestClient();

        // Act
        using var response = await client.GetAsync(
            "/stellaradmin/products/lookup/1?form=edit&field=CategoryId&Model.Number=1"
        );
        var document = await ReadDocument(response);

        // Assert
        await Assert.That(response.StatusCode).IsEqualTo(HttpStatusCode.OK);
        await Assert
            .That(
                document
                    .QuerySelectorAll("[data-lookup='item']")
                    .Select(item => item.GetAttribute("data-value"))
            )
            .IsEquivalentTo(["3"]);
    }

    private static async Task<IDocument> ReadDocument(HttpResponseMessage response) =>
        await new HtmlParser().ParseDocumentAsync(await response.Content.ReadAsStringAsync());
}
