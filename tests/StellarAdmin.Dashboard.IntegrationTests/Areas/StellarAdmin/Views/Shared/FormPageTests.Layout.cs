using AngleSharp.Html.Parser;
using IdentitySimplePlayground.Data;
using StellarAdmin.Dashboard.Testing;
using StellarAdmin.TagHelpers;

namespace StellarAdmin.Dashboard.IntegrationTests.Areas.StellarAdmin.Views.Shared;

public partial class FormPageTests
{
    [Test]
    [Arguments(false, FormSectionLayout.Split, "", "split")]
    [Arguments(false, FormSectionLayout.Card, "", "card")]
    [Arguments(false, FormSectionLayout.Card, "Stacked", "stacked")]
    [Arguments(false, FormSectionLayout.Card, "Split", "split")]
    [Arguments(false, FormSectionLayout.Split, "Card", "card")]
    [Arguments(true, FormSectionLayout.Split, "", "split")]
    [Arguments(true, FormSectionLayout.Card, "", "card")]
    [Arguments(true, FormSectionLayout.Card, "Stacked", "stacked")]
    [Arguments(true, FormSectionLayout.Card, "Split", "split")]
    [Arguments(true, FormSectionLayout.Split, "Card", "card")]
    public async Task ExecuteAsync_WhenFormAndAppLayoutsDiffer_AppliesCascadeWithinGroups(
        bool edit,
        FormSectionLayout appLayout,
        string form,
        string expected
    )
    {
        // Arrange
        await using var app = new TestApplication(appLayout);

        // Act
        using var html = await app.GetHtmlAsync($"/test-form-layout?edit={edit}&form={form}");

        // Assert
        await Assert.That(html.QuerySelector($"[data-layout='{expected}']")).IsNotNull();
        await Assert.That(html.QuerySelector($"[data-form-layout='{expected}']")).IsNotNull();
        await Assert.That(html.QuerySelector("input[name='Entity.Name']")).IsNotNull();
        await Assert.That(html.QuerySelector("input[name='Entity.Sku']")).IsNotNull();
    }
}
