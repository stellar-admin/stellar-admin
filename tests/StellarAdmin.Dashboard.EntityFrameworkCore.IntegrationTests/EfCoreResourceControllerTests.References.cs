using System.Net;
using AngleSharp.Html.Parser;
using IdentitySimplePlayground.Data;
using Microsoft.EntityFrameworkCore;
using StellarAdmin.Dashboard.Testing;

namespace StellarAdmin.Dashboard.EntityFrameworkCore.IntegrationTests;

public partial class EfCoreResourceControllerTests
{
    [Test]
    public async Task CreatePost_WhenOtherFieldIsInvalid_PreservesSelectedReference()
    {
        // Arrange
        await using var app = await TestApplication.CreateAsync();
        var alpha = new Category { Name = "Reference Alpha" };
        var zulu = new Category { Name = "Reference Zulu" };
        var hidden = new Category { Name = "Hidden category" };
        app.Db.Categories.AddRange(zulu, alpha, hidden);
        await app.Db.SaveChangesAsync();
        var token = await app.GetTokenAsync("/admin/test-references/Create");

        // Act
        using var response = await app.PostAsync(
            "/admin/test-references/Create",
            token,
            ("Entity.Name", ""),
            ("Entity.Sku", "REF-Z"),
            ("Entity.CategoryId", zulu.Id.ToString())
        );

        // Assert
        await Assert.That(response.StatusCode).IsEqualTo(HttpStatusCode.OK);
        using var html = new HtmlParser().ParseDocument(await response.Content.ReadAsStringAsync());
        await Assert
            .That(
                html.QuerySelector("select[name='Entity.CategoryId'] option[selected]")
                    ?.GetAttribute("value")
            )
            .IsEqualTo(zulu.Id.ToString());
    }

    [Test]
    public async Task CreatePost_WhenReferenceIsAllowed_PersistsForeignKey()
    {
        // Arrange
        await using var app = await TestApplication.CreateAsync();
        var alpha = new Category { Name = "Reference Alpha" };
        var zulu = new Category { Name = "Reference Zulu" };
        var hidden = new Category { Name = "Hidden category" };
        app.Db.Categories.AddRange(zulu, alpha, hidden);
        await app.Db.SaveChangesAsync();
        var token = await app.GetTokenAsync("/admin/test-references/Create");
        var category = zulu.Id.ToString();

        // Act
        using var response = await app.PostAsync(
            "/admin/test-references/Create",
            token,
            ("Entity.Name", "Zulu product"),
            ("Entity.Sku", "REF-Z"),
            ("Entity.CategoryId", category)
        );

        // Assert
        await Assert.That(response.StatusCode).IsEqualTo(HttpStatusCode.Redirect);
        await Assert
            .That((await app.Db.Products.AsNoTracking().SingleAsync()).CategoryId)
            .IsEqualTo(zulu.Id);
    }

    [Test]
    [Arguments("99999999")]
    [Arguments("hidden")]
    [Arguments("not-an-id")]
    public async Task CreatePost_WhenReferenceIsInvalid_ReloadsChoicesWithoutSaving(string value)
    {
        // Arrange
        await using var app = await TestApplication.CreateAsync();
        var alpha = new Category { Name = "Reference Alpha" };
        var zulu = new Category { Name = "Reference Zulu" };
        var hidden = new Category { Name = "Hidden category" };
        app.Db.Categories.AddRange(zulu, alpha, hidden);
        await app.Db.SaveChangesAsync();
        var token = await app.GetTokenAsync("/admin/test-references/Create");
        var category = value == "hidden" ? hidden.Id.ToString() : value;

        // Act
        using var response = await app.PostAsync(
            "/admin/test-references/Create",
            token,
            ("Entity.Name", "Zulu product"),
            ("Entity.Sku", "REF-Z"),
            ("Entity.CategoryId", category)
        );

        // Assert
        await Assert.That(response.StatusCode).IsEqualTo(HttpStatusCode.OK);
        await Assert.That(await response.Content.ReadAsStringAsync()).Contains("Reference Alpha");
        await Assert.That(await app.Db.Products.AnyAsync()).IsFalse();
    }

    [Test]
    public async Task CreatePost_WhenRequiredReferenceIsEmpty_RejectsSelection()
    {
        // Arrange
        await using var app = await TestApplication.CreateAsync();
        var alpha = new Category { Name = "Reference Alpha" };
        var zulu = new Category { Name = "Reference Zulu" };
        var hidden = new Category { Name = "Hidden category" };
        app.Db.Categories.AddRange(zulu, alpha, hidden);
        await app.Db.SaveChangesAsync();
        var token = await app.GetTokenAsync("/admin/test-required-reference/Create");
        var category = "";

        // Act
        using var response = await app.PostAsync(
            "/admin/test-required-reference/Create",
            token,
            ("Entity.Name", "Zulu product"),
            ("Entity.Sku", "REF-Z"),
            ("Entity.CategoryId", category)
        );

        // Assert
        await Assert.That(response.StatusCode).IsEqualTo(HttpStatusCode.OK);
        await Assert
            .That(await response.Content.ReadAsStringAsync())
            .Contains("Select a valid option.");
        await Assert.That(await app.Db.Products.AnyAsync()).IsFalse();
    }

    [Test]
    public async Task CreatePost_WhenRequiredReferenceIsValid_PersistsSelection()
    {
        // Arrange
        await using var app = await TestApplication.CreateAsync();
        var alpha = new Category { Name = "Reference Alpha" };
        var zulu = new Category { Name = "Reference Zulu" };
        var hidden = new Category { Name = "Hidden category" };
        app.Db.Categories.AddRange(zulu, alpha, hidden);
        await app.Db.SaveChangesAsync();
        var token = await app.GetTokenAsync("/admin/test-required-reference/Create");
        var category = alpha.Id.ToString();

        // Act
        using var response = await app.PostAsync(
            "/admin/test-required-reference/Create",
            token,
            ("Entity.Name", "Zulu product"),
            ("Entity.Sku", "REF-Z"),
            ("Entity.CategoryId", category)
        );

        // Assert
        await Assert.That(response.StatusCode).IsEqualTo(HttpStatusCode.Redirect);
        await Assert
            .That((await app.Db.Products.AsNoTracking().SingleAsync()).CategoryId)
            .IsEqualTo(alpha.Id);
    }

    [Test]
    public async Task Create_WhenReferenceIsNullable_RendersFilteredSortedChoices()
    {
        // Arrange
        await using var app = await TestApplication.CreateAsync();
        var alpha = new Category { Name = "Reference Alpha" };
        var zulu = new Category { Name = "Reference Zulu" };
        var hidden = new Category { Name = "Hidden category" };
        app.Db.Categories.AddRange(zulu, alpha, hidden);
        await app.Db.SaveChangesAsync();

        // Act
        using var html = await app.GetHtmlAsync("/admin/test-references/Create");

        // Assert
        await Assert
            .That(html.Body!.TextContent)
            .Contains("Choose the category that best describes this product.");
        await Assert.That(html.Body.TextContent).Contains("Not set");
        await Assert.That(html.Body.TextContent).DoesNotContain("Hidden category");
        await Assert
            .That(
                string.Join(
                    ",",
                    html.QuerySelectorAll("select[name='Entity.CategoryId'] option")
                        .Select(option => option.TextContent.Trim())
                )
            )
            .IsEqualTo("Not set,Reference Alpha,Reference Zulu");
    }

    [Test]
    public async Task Create_WhenReferenceIsRequired_RendersPrompt()
    {
        // Arrange
        await using var app = await TestApplication.CreateAsync();
        var alpha = new Category { Name = "Reference Alpha" };
        var zulu = new Category { Name = "Reference Zulu" };
        var hidden = new Category { Name = "Hidden category" };
        app.Db.Categories.AddRange(zulu, alpha, hidden);
        await app.Db.SaveChangesAsync();

        // Act
        using var html = await app.GetHtmlAsync("/admin/test-required-reference/Create");

        // Assert
        await Assert.That(html.Body!.TextContent).Contains("Select an option");
        await Assert.That(html.Body.TextContent).DoesNotContain("Not set");
    }

    [Test]
    public async Task EditPost_WhenCurrentReferenceIsFiltered_RejectsAllChanges()
    {
        // Arrange
        await using var app = await TestApplication.CreateAsync();
        var alpha = new Category { Name = "Reference Alpha" };
        var zulu = new Category { Name = "Reference Zulu" };
        var hidden = new Category { Name = "Hidden category" };
        app.Db.Categories.AddRange(zulu, alpha, hidden);
        await app.Db.SaveChangesAsync();
        var product = new Product
        {
            Name = "Zulu product",
            Sku = "REF-Z",
            CategoryId = zulu.Id,
        };
        app.Db.Products.Add(product);
        await app.Db.SaveChangesAsync();
        await app.Db.Products.ExecuteUpdateAsync(set =>
            set.SetProperty(p => p.CategoryId, hidden.Id)
        );
        var token = await app.GetTokenAsync($"/admin/test-references/Edit/{product.Id}");

        // Act
        using var response = await app.PostAsync(
            $"/admin/test-references/Edit/{product.Id}",
            token,
            ("Entity.Name", "Changed name"),
            ("Entity.Sku", "REF-Z"),
            ("Entity.CategoryId", hidden.Id.ToString())
        );

        // Assert
        await Assert.That(response.StatusCode).IsEqualTo(HttpStatusCode.OK);
        await Assert
            .That(await response.Content.ReadAsStringAsync())
            .Contains("Select a valid option.");
        await Assert
            .That((await app.Db.Products.AsNoTracking().SingleAsync()).Name)
            .IsEqualTo("Zulu product");
    }

    [Test]
    public async Task EditPost_WhenReadOnlyReferenceIsForged_UpdatesOnlyEditableFields()
    {
        // Arrange
        await using var app = await TestApplication.CreateAsync();
        var alpha = new Category { Name = "Reference Alpha" };
        var zulu = new Category { Name = "Reference Zulu" };
        var hidden = new Category { Name = "Hidden category" };
        app.Db.Categories.AddRange(zulu, alpha, hidden);
        await app.Db.SaveChangesAsync();
        var product = new Product
        {
            Name = "Zulu product",
            Sku = "REF-Z",
            CategoryId = zulu.Id,
        };
        app.Db.Products.Add(product);
        await app.Db.SaveChangesAsync();
        var token = await app.GetTokenAsync($"/admin/test-required-reference/Edit/{product.Id}");

        // Act
        using var response = await app.PostAsync(
            $"/admin/test-required-reference/Edit/{product.Id}",
            token,
            ("Entity.Name", "Updated"),
            ("Entity.Sku", "REF-Z"),
            ("Entity.CategoryId", alpha.Id.ToString())
        );

        // Assert
        await Assert.That(response.StatusCode).IsEqualTo(HttpStatusCode.Redirect);
        var saved = await app.Db.Products.AsNoTracking().SingleAsync();
        await Assert.That(saved.CategoryId).IsEqualTo(zulu.Id);
        await Assert.That(saved.Name).IsEqualTo("Updated");
    }

    [Test]
    [Arguments(false)]
    [Arguments(true)]
    public async Task EditPost_WhenReferenceChanges_PersistsNewOrEmptyKey(bool clear)
    {
        // Arrange
        await using var app = await TestApplication.CreateAsync();
        var alpha = new Category { Name = "Reference Alpha" };
        var zulu = new Category { Name = "Reference Zulu" };
        var hidden = new Category { Name = "Hidden category" };
        app.Db.Categories.AddRange(zulu, alpha, hidden);
        await app.Db.SaveChangesAsync();
        var product = new Product
        {
            Name = "Zulu product",
            Sku = "REF-Z",
            CategoryId = zulu.Id,
        };
        app.Db.Products.Add(product);
        await app.Db.SaveChangesAsync();
        var token = await app.GetTokenAsync($"/admin/test-references/Edit/{product.Id}");
        var category = clear ? "" : alpha.Id.ToString();

        // Act
        using var response = await app.PostAsync(
            $"/admin/test-references/Edit/{product.Id}",
            token,
            ("Entity.Name", "Zulu product"),
            ("Entity.Sku", "REF-Z"),
            ("Entity.CategoryId", category)
        );

        // Assert
        await Assert.That(response.StatusCode).IsEqualTo(HttpStatusCode.Redirect);
        await Assert
            .That((await app.Db.Products.AsNoTracking().SingleAsync()).CategoryId)
            .IsEqualTo(clear ? (int?)null : alpha.Id);
    }

    [Test]
    public async Task Edit_WhenCurrentReferenceIsFiltered_RendersAssignedLabel()
    {
        // Arrange
        await using var app = await TestApplication.CreateAsync();
        var alpha = new Category { Name = "Reference Alpha" };
        var zulu = new Category { Name = "Reference Zulu" };
        var hidden = new Category { Name = "Hidden category" };
        app.Db.Categories.AddRange(zulu, alpha, hidden);
        await app.Db.SaveChangesAsync();
        var product = new Product
        {
            Name = "Zulu product",
            Sku = "REF-Z",
            CategoryId = zulu.Id,
        };
        app.Db.Products.Add(product);
        await app.Db.SaveChangesAsync();
        await app.Db.Products.ExecuteUpdateAsync(set =>
            set.SetProperty(p => p.CategoryId, hidden.Id)
        );

        // Act
        using var html = await app.GetHtmlAsync($"/admin/test-references/Edit/{product.Id}");

        // Assert
        await Assert
            .That(
                html.QuerySelector("select[name='Entity.CategoryId'] option[selected]")
                    ?.GetAttribute("value")
            )
            .IsEqualTo(hidden.Id.ToString());
        await Assert.That(html.Body!.TextContent).Contains("Hidden category");
    }

    [Test]
    public async Task Edit_WhenReferenceIsAssigned_SelectsKeyAndProjectsOnlyChoiceKeyAndLabel()
    {
        // Arrange
        await using var app = await TestApplication.CreateAsync();
        var alpha = new Category { Name = "Reference Alpha" };
        var zulu = new Category { Name = "Reference Zulu" };
        var hidden = new Category { Name = "Hidden category" };
        app.Db.Categories.AddRange(zulu, alpha, hidden);
        await app.Db.SaveChangesAsync();
        var product = new Product
        {
            Name = "Zulu product",
            Sku = "REF-Z",
            CategoryId = zulu.Id,
        };
        app.Db.Products.Add(product);
        await app.Db.SaveChangesAsync();
        app.Commands.Sql.Clear();

        // Act
        using var html = await app.GetHtmlAsync($"/admin/test-references/Edit/{product.Id}");

        // Assert
        await Assert
            .That(
                html.QuerySelector("select[name='Entity.CategoryId'] option[selected]")
                    ?.GetAttribute("value")
            )
            .IsEqualTo(zulu.Id.ToString());
        await Assert.That(app.Commands.Sql.Count).IsEqualTo(2);
        await Assert
            .That(app.Commands.Sql.Any(sql => sql.Contains("JOIN \"Categories\"")))
            .IsTrue();
        var choices = app.Commands.Sql.Single(sql => sql.Contains("FROM \"Categories\""));
        await Assert.That(choices).Contains("SELECT \"c\".\"Id\", \"c\".\"Name\"");
    }

    [Test]
    public async Task Edit_WhenReferenceIsReadOnly_RendersDisabledSelection()
    {
        // Arrange
        await using var app = await TestApplication.CreateAsync();
        var alpha = new Category { Name = "Reference Alpha" };
        var zulu = new Category { Name = "Reference Zulu" };
        var hidden = new Category { Name = "Hidden category" };
        app.Db.Categories.AddRange(zulu, alpha, hidden);
        await app.Db.SaveChangesAsync();
        var product = new Product
        {
            Name = "Zulu product",
            Sku = "REF-Z",
            CategoryId = zulu.Id,
        };
        app.Db.Products.Add(product);
        await app.Db.SaveChangesAsync();

        // Act
        using var html = await app.GetHtmlAsync(
            $"/admin/test-required-reference/Edit/{product.Id}"
        );

        // Assert
        await Assert
            .That(
                html.QuerySelector("select[name='Entity.CategoryId'] option[selected]")
                    ?.GetAttribute("value")
            )
            .IsEqualTo(zulu.Id.ToString());
        await Assert
            .That(html.QuerySelector("select[name='Entity.CategoryId']")!.HasAttribute("disabled"))
            .IsTrue();
    }

    [Test]
    public async Task Index_WhenReferenceColumnIsUnused_DoesNotLoadReferenceData()
    {
        // Arrange
        await using var app = await TestApplication.CreateAsync();
        var alpha = new Category { Name = "Reference Alpha" };
        var zulu = new Category { Name = "Reference Zulu" };
        var hidden = new Category { Name = "Hidden category" };
        app.Db.Categories.AddRange(zulu, alpha, hidden);
        await app.Db.SaveChangesAsync();
        var product = new Product
        {
            Name = "Zulu product",
            Sku = "REF-Z",
            CategoryId = zulu.Id,
        };
        app.Db.Products.Add(product);
        await app.Db.SaveChangesAsync();
        app.Commands.Sql.Clear();

        // Act
        using var html = await app.GetHtmlAsync("/admin/test-required-reference");

        // Assert
        await Assert.That(app.Commands.Sql.Count).IsEqualTo(2);
        await Assert.That(app.Commands.Sql.Any(sql => sql.Contains("Categories"))).IsFalse();
    }

    [Test]
    public async Task Index_WhenSortingByReference_UsesLabelsAndTwoQueries()
    {
        // Arrange
        await using var app = await TestApplication.CreateAsync();
        var alpha = new Category { Name = "Reference Alpha" };
        var zulu = new Category { Name = "Reference Zulu" };
        var hidden = new Category { Name = "Hidden category" };
        app.Db.Categories.AddRange(zulu, alpha, hidden);
        await app.Db.SaveChangesAsync();
        var product = new Product
        {
            Name = "Zulu product",
            Sku = "REF-Z",
            CategoryId = zulu.Id,
        };
        app.Db.Products.Add(product);
        await app.Db.SaveChangesAsync();
        app.Db.Products.Add(
            new Product
            {
                Name = "Alpha product",
                Sku = "REF-A",
                CategoryId = alpha.Id,
            }
        );
        await app.Db.SaveChangesAsync();
        app.Commands.Sql.Clear();

        // Act
        using var html = await app.GetHtmlAsync("/admin/test-references?SortBy=CategoryId");

        // Assert
        var text = html.Body!.TextContent;
        await Assert.That(text).Contains("Alpha product");
        await Assert.That(text).Contains("Zulu product");
        await Assert
            .That(text.IndexOf("Alpha product", StringComparison.Ordinal))
            .IsLessThan(text.IndexOf("Zulu product", StringComparison.Ordinal));
        await Assert.That(text).Contains("Reference Alpha");
        await Assert.That(text).Contains("Reference Zulu");
        await Assert.That(app.Commands.Sql.Count).IsEqualTo(2);
        await Assert
            .That(app.Commands.Sql.Any(sql => sql.Contains("JOIN \"Categories\"")))
            .IsTrue();
        await Assert
            .That(app.Commands.Sql.Any(sql => sql.Contains("FROM \"Categories\"")))
            .IsFalse();
    }
}
