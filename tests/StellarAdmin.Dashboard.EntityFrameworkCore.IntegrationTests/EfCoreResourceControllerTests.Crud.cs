using System.Net;
using IdentitySimplePlayground.Data;
using Microsoft.EntityFrameworkCore;
using StellarAdmin.Dashboard.Testing;

namespace StellarAdmin.Dashboard.EntityFrameworkCore.IntegrationTests;

public partial class EfCoreResourceControllerTests
{
    [Test]
    public async Task CreatePost_WhenAntiforgeryTokenIsMissing_RejectsRequest()
    {
        // Arrange
        await using var app = await TestApplication.CreateAsync();

        // Act
        using var response = await app.PostAsync(
            "/admin/categories/Create",
            null,
            ("Entity.Name", "Rejected")
        );

        // Assert
        await Assert.That(response.StatusCode).IsEqualTo(HttpStatusCode.BadRequest);
    }

    [Test]
    [Arguments(0, 0)]
    [Arguments(101, 501)]
    public async Task CreatePost_WhenInputIsInvalid_RedisplaysWithoutPersisting(
        int nameLength,
        int descriptionLength
    )
    {
        // Arrange
        var name = new string('n', nameLength);
        var description = new string('d', descriptionLength);
        await using var app = await TestApplication.CreateAsync();
        var token = await app.GetTokenAsync("/admin/categories/Create");

        // Act
        using var response = await app.PostAsync(
            "/admin/categories/Create",
            token,
            ("Entity.Name", name),
            ("Entity.Description", description)
        );

        // Assert
        await Assert.That(response.StatusCode).IsEqualTo(HttpStatusCode.OK);
        await Assert.That(await app.Db.Categories.CountAsync()).IsEqualTo(0);
    }

    [Test]
    public async Task CreatePost_WhenKeyIsForged_PersistsOnlyConfiguredProperties()
    {
        // Arrange
        await using var app = await TestApplication.CreateAsync();
        var token = await app.GetTokenAsync("/admin/categories/Create");

        // Act
        using var response = await app.PostAsync(
            "/admin/categories/Create",
            token,
            ("Entity.Name", "Books"),
            ("Entity.Description", "Read me"),
            ("Entity.Id", "999")
        );

        // Assert
        await Assert.That(response.StatusCode).IsEqualTo(HttpStatusCode.Redirect);
        var saved = await app.Db.Categories.AsNoTracking().SingleAsync();
        await Assert.That(saved.Name).IsEqualTo("Books");
        await Assert.That(saved.Id).IsNotEqualTo(999);
    }

    [Test]
    public async Task CreatePost_WhenRequiredNameIsEmpty_RendersValidationError()
    {
        // Arrange
        await using var app = await TestApplication.CreateAsync();
        var token = await app.GetTokenAsync("/admin/categories/Create");

        // Act
        using var response = await app.PostAsync(
            "/admin/categories/Create",
            token,
            ("Entity.Name", "")
        );

        // Assert
        await Assert.That(await response.Content.ReadAsStringAsync()).Contains("required");
    }

    [Test]
    public async Task Delete_WhenAntiforgeryTokenIsMissing_RejectsRequest()
    {
        // Arrange
        await using var app = await TestApplication.CreateAsync();
        var category = new Category { Name = "Original" };
        app.Db.Categories.Add(category);
        await app.Db.SaveChangesAsync();

        // Act
        using var response = await app.PostAsync($"/admin/categories/Delete/{category.Id}", null);

        // Assert
        await Assert.That(response.StatusCode).IsEqualTo(HttpStatusCode.BadRequest);
        await Assert.That(await app.Db.Categories.AnyAsync()).IsTrue();
    }

    [Test]
    public async Task Delete_WhenAntiforgeryTokenIsValid_RemovesRecord()
    {
        // Arrange
        await using var app = await TestApplication.CreateAsync();
        var category = new Category { Name = "Original" };
        app.Db.Categories.Add(category);
        await app.Db.SaveChangesAsync();
        var token = await app.GetTokenAsync($"/admin/categories/Edit/{category.Id}");

        // Act
        using var response = await app.PostAsync($"/admin/categories/Delete/{category.Id}", token);

        // Assert
        await Assert.That(response.StatusCode).IsEqualTo(HttpStatusCode.Redirect);
        await Assert.That(await app.Db.Categories.AnyAsync()).IsFalse();
    }

    [Test]
    public async Task EditPost_WhenKeyIsForged_UpdatesOriginalRecord()
    {
        // Arrange
        await using var app = await TestApplication.CreateAsync();
        var category = new Category { Name = "Original" };
        app.Db.Categories.Add(category);
        await app.Db.SaveChangesAsync();
        var token = await app.GetTokenAsync($"/admin/categories/Edit/{category.Id}");

        // Act
        using var response = await app.PostAsync(
            $"/admin/categories/Edit/{category.Id}",
            token,
            ("Entity.Name", "Edited"),
            ("Entity.Id", "999")
        );

        // Assert
        await Assert.That(response.StatusCode).IsEqualTo(HttpStatusCode.Redirect);
        var saved = await app.Db.Categories.AsNoTracking().SingleAsync();
        await Assert.That(saved.Name).IsEqualTo("Edited");
        await Assert.That(saved.Id).IsEqualTo(category.Id);
    }

    [Test]
    public async Task EditPost_WhenStringKeyAndReadOnlyFieldArePosted_PreservesProtectedValue()
    {
        // Arrange
        await using var app = await TestApplication.CreateAsync();
        var role = new ApplicationRole { Name = "Existing", NormalizedName = "EXISTING" };
        app.Db.Roles.Add(role);
        await app.Db.SaveChangesAsync();
        var token = await app.GetTokenAsync($"/admin/test-roles/Edit/{role.Id}");

        // Act
        using var response = await app.PostAsync(
            $"/admin/test-roles/Edit/{role.Id}",
            token,
            ("Entity.Name", "Changed"),
            ("Entity.NormalizedName", "TAMPERED")
        );

        // Assert
        await Assert.That(response.StatusCode).IsEqualTo(HttpStatusCode.Redirect);
        var saved = await app.Db.Roles.AsNoTracking().SingleAsync();
        await Assert.That(saved.Name).IsEqualTo("Changed");
        await Assert.That(saved.NormalizedName).IsEqualTo("EXISTING");
    }

    [Test]
    [Arguments("not-a-key")]
    [Arguments("999999")]
    public async Task Edit_WhenKeyIsInvalidOrMissing_ReturnsNotFound(string key)
    {
        // Arrange
        await using var app = await TestApplication.CreateAsync();

        // Act
        using var response = await app.Client.GetAsync($"/admin/categories/Edit/{key}");

        // Assert
        await Assert.That(response.StatusCode).IsEqualTo(HttpStatusCode.NotFound);
    }

    [Test]
    public async Task Index_WhenPageExceedsLastPage_AppliesDescendingSortAndClampsPage()
    {
        // Arrange
        await using var app = await TestApplication.CreateAsync();
        app.Db.Categories.Add(new Category { Name = "Edited" });
        app.Db.Categories.AddRange(
            Enumerable.Range(0, 30).Select(i => new Category { Name = $"Item {i:D2}" })
        );
        await app.Db.SaveChangesAsync();

        // Act
        using var html = await app.GetHtmlAsync(
            "/admin/categories?PageSize=5&PageNo=999&SortBy=Name&SortDir=desc"
        );

        // Assert
        await Assert.That(html.Body!.TextContent).Contains("Edited");
        await Assert.That(html.Body.TextContent).DoesNotContain("Item 29");
    }

    [Test]
    [Arguments("/admin/categories", "Categories")]
    [Arguments("/admin/test-roles", "ApplicationRole")]
    public async Task Index_WhenResourceIsRegistered_RendersIndependentPage(
        string path,
        string title
    )
    {
        // Arrange
        await using var app = await TestApplication.CreateAsync();

        // Act
        using var response = await app.Client.GetAsync(path);

        // Assert
        await Assert.That(response.StatusCode).IsEqualTo(HttpStatusCode.OK);
        await Assert.That(await response.Content.ReadAsStringAsync()).Contains(title);
    }

    [Test]
    public async Task Index_WhenSearchIsSpecified_FiltersRows()
    {
        // Arrange
        await using var app = await TestApplication.CreateAsync();
        app.Db.Categories.AddRange(
            new Category { Name = "Item 28" },
            new Category { Name = "Item 29" }
        );
        await app.Db.SaveChangesAsync();

        // Act
        using var html = await app.GetHtmlAsync("/admin/categories?Search=Item%2029");

        // Assert
        await Assert.That(html.Body!.TextContent).Contains("Item 29");
        await Assert.That(html.Body.TextContent).DoesNotContain("Item 28");
    }
}
