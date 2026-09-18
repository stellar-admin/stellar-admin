using System.Net;
using IdentitySimplePlayground.Data;
using Microsoft.EntityFrameworkCore;
using StellarAdmin.Dashboard.Testing;

namespace StellarAdmin.Dashboard.EntityFrameworkCore.IntegrationTests;

public partial class EfCoreResourceControllerTests
{
    [Test]
    [Arguments("", "-1", "Used")]
    [Arguments("INVALID", "0", "999")]
    public async Task CreatePost_WhenProductIsInvalid_DoesNotPersist(
        string sku,
        string price,
        string condition
    )
    {
        // Arrange
        await using var app = await TestApplication.CreateAsync();
        var token = await app.GetTokenAsync("/admin/products/Create");

        // Act
        using var response = await app.PostAsync(
            "/admin/products/Create",
            token,
            ("Entity.Name", "Invalid"),
            ("Entity.Sku", sku),
            ("Entity.Price", price),
            ("Entity.Condition", condition)
        );

        // Assert
        await Assert.That(response.StatusCode).IsEqualTo(HttpStatusCode.OK);
        await Assert.That(await app.Db.Products.AnyAsync()).IsFalse();
    }

    [Test]
    public async Task CreatePost_WhenScalarValuesAndReferenceArePosted_PersistsEditableValues()
    {
        // Arrange
        await using var app = await TestApplication.CreateAsync();
        var category = new Category { Name = "Original" };
        app.Db.Categories.Add(category);
        await app.Db.SaveChangesAsync();
        var token = await app.GetTokenAsync("/admin/products/Create");

        // Act
        using var response = await app.PostAsync(
            "/admin/products/Create",
            token,
            ("Entity.Name", "Desk lamp"),
            ("Entity.Sku", "LAMP-001"),
            ("Entity.Description", "A reading lamp"),
            ("Entity.Condition", "Refurbished"),
            ("Entity.Price", "29.95"),
            ("Entity.CompareAtPrice", "39.95"),
            ("Entity.CostPrice", "12.50"),
            ("Entity.StockQuantity", "15"),
            ("Entity.TrackInventory", "true"),
            ("Entity.AllowBackorders", "false"),
            ("Entity.RequiresShipping", "true"),
            ("Entity.WeightKg", "1.25"),
            ("Entity.WidthCm", "12.5"),
            ("Entity.HeightCm", "40"),
            ("Entity.DepthCm", "15"),
            ("Entity.IsPublished", "true"),
            ("Entity.PublishedAt", "2026-09-08T12:00:00"),
            ("Entity.CreatedAt", "2000-01-01"),
            ("Entity.CategoryId", category.Id.ToString())
        );

        // Assert
        await Assert.That(response.StatusCode).IsEqualTo(HttpStatusCode.Redirect);
        var saved = await app.Db.Products.AsNoTracking().SingleAsync();
        await Assert.That(saved.Condition).IsEqualTo(ProductCondition.Refurbished);
        await Assert.That(saved.Price).IsEqualTo(29.95m);
        await Assert.That(saved.WeightKg).IsEqualTo(1.25m);
        await Assert.That(saved.StockQuantity).IsEqualTo(15);
        await Assert.That(saved.IsPublished).IsTrue();
        await Assert.That(saved.CategoryId).IsEqualTo(category.Id);
        await Assert.That(saved.PublishedAt).IsNotNull();
        await Assert.That(saved.CreatedAt.Year).IsNotEqualTo(2000);
    }

    [Test]
    public async Task Delete_WhenProductExists_RemovesProduct()
    {
        // Arrange
        await using var app = await TestApplication.CreateAsync();
        var product = new Product
        {
            Name = "Desk lamp",
            Sku = "LAMP-001",
            Condition = ProductCondition.Refurbished,
            CompareAtPrice = 39.95m,
            WeightKg = 1.25m,
            PublishedAt = new DateTime(2026, 9, 8),
            IsPublished = true,
        };
        app.Db.Products.Add(product);
        await app.Db.SaveChangesAsync();
        var token = await app.GetTokenAsync($"/admin/products/Edit/{product.Id}");

        // Act
        using var response = await app.PostAsync($"/admin/products/Delete/{product.Id}", token);

        // Assert
        await Assert.That(response.StatusCode).IsEqualTo(HttpStatusCode.Redirect);
        await Assert.That(await app.Db.Products.AnyAsync()).IsFalse();
    }

    [Test]
    public async Task EditPost_WhenOptionalFieldsAreEmpty_ClearsValuesAndUpdatesCondition()
    {
        // Arrange
        await using var app = await TestApplication.CreateAsync();
        var product = new Product
        {
            Name = "Desk lamp",
            Sku = "LAMP-001",
            Condition = ProductCondition.Refurbished,
            CompareAtPrice = 39.95m,
            WeightKg = 1.25m,
            PublishedAt = new DateTime(2026, 9, 8),
            IsPublished = true,
        };
        app.Db.Products.Add(product);
        await app.Db.SaveChangesAsync();
        var token = await app.GetTokenAsync($"/admin/products/Edit/{product.Id}");

        // Act
        using var response = await app.PostAsync(
            $"/admin/products/Edit/{product.Id}",
            token,
            ("Entity.Name", "Updated lamp"),
            ("Entity.Sku", "LAMP-001"),
            ("Entity.Condition", "Used"),
            ("Entity.CompareAtPrice", ""),
            ("Entity.WeightKg", ""),
            ("Entity.PublishedAt", "")
        );

        // Assert
        await Assert.That(response.StatusCode).IsEqualTo(HttpStatusCode.Redirect);
        var saved = await app.Db.Products.AsNoTracking().SingleAsync();
        await Assert.That(saved.Name).IsEqualTo("Updated lamp");
        await Assert.That(saved.Condition).IsEqualTo(ProductCondition.Used);
        await Assert.That(saved.CompareAtPrice).IsNull();
        await Assert.That(saved.WeightKg).IsNull();
        await Assert.That(saved.PublishedAt).IsNull();
    }

    [Test]
    public async Task Index_WhenSkuSearchAndPublishedScopeAreSpecified_RendersMatchingProduct()
    {
        // Arrange
        await using var app = await TestApplication.CreateAsync();
        var product = new Product
        {
            Name = "Desk lamp",
            Sku = "LAMP-001",
            Condition = ProductCondition.Refurbished,
            CompareAtPrice = 39.95m,
            WeightKg = 1.25m,
            PublishedAt = new DateTime(2026, 9, 8),
            IsPublished = true,
        };
        app.Db.Products.Add(product);
        await app.Db.SaveChangesAsync();

        // Act
        using var response = await app.Client.GetAsync(
            "/admin/products?Search=LAMP-001&Scope=published"
        );

        // Assert
        await Assert.That(response.StatusCode).IsEqualTo(HttpStatusCode.OK);
        await Assert.That(await response.Content.ReadAsStringAsync()).Contains("Desk lamp");
    }
}
