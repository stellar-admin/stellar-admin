using AngleSharp.Html.Parser;
using IdentitySimplePlayground.Data;
using StellarAdmin.Dashboard.Testing;
using StellarAdmin.TagHelpers;

namespace StellarAdmin.Dashboard.IntegrationTests.Areas.StellarAdmin.Views.Shared;

public partial class FormPageTests
{
    [Test]
    [Arguments("products")]
    [Arguments("users")]
    [Arguments("roles")]
    public async Task ExecuteAsync_WhenCreateFormRenders_OmitsDeleteTrigger(string route)
    {
        // Arrange
        await using var app = await TestApplication.CreateAsync();
        var product = new Product { Name = "Lamp", Sku = "LAMP" };
        var role = new ApplicationRole { Name = "Existing", NormalizedName = "EXISTING" };
        var user = new ApplicationUser
        {
            UserName = "layout@example.com",
            Email = "layout@example.com",
        };
        app.Db.AddRange(product, role, user);
        await app.Db.SaveChangesAsync();
        var id =
            route == "products" ? product.Id.ToString()
            : route == "roles" ? role.Id
            : user.Id;
        var path = $"/admin/{route}/Create";

        // Act
        using var html = await app.GetHtmlAsync(path);

        // Assert
        await Assert
            .That(html.QuerySelectorAll("button[commandfor=delete-confirm-dialog]").Length)
            .IsEqualTo(0);
    }

    [Test]
    [Arguments("products")]
    [Arguments("users")]
    [Arguments("roles")]
    public async Task ExecuteAsync_WhenEditFormRenders_ShowsDeleteBeforeCancelWithConfirmation(
        string route
    )
    {
        // Arrange
        await using var app = await TestApplication.CreateAsync();
        var product = new Product { Name = "Lamp", Sku = "LAMP" };
        var role = new ApplicationRole { Name = "Existing", NormalizedName = "EXISTING" };
        var user = new ApplicationUser
        {
            UserName = "layout@example.com",
            Email = "layout@example.com",
        };
        app.Db.AddRange(product, role, user);
        await app.Db.SaveChangesAsync();
        var id =
            route == "products" ? product.Id.ToString()
            : route == "roles" ? role.Id
            : user.Id;
        var path = $"/admin/{route}/Edit/{id}";

        // Act
        using var html = await app.GetHtmlAsync(path);

        // Assert
        await Assert
            .That(html.QuerySelectorAll("button[commandfor=delete-confirm-dialog]").Length)
            .IsEqualTo(1);
        var trigger = html.QuerySelector("button[commandfor=delete-confirm-dialog]")!;
        await Assert.That(trigger.ClassList).Contains("sa-button-variant-ghost");
        await Assert.That(trigger.GetAttribute("type")).IsEqualTo("button");
        var markup = html.DocumentElement.OuterHtml;
        await Assert
            .That(markup.IndexOf("sa-resource-form-delete", StringComparison.Ordinal))
            .IsGreaterThanOrEqualTo(0);
        await Assert
            .That(markup.IndexOf("sa-resource-form-delete", StringComparison.Ordinal))
            .IsLessThan(markup.IndexOf("data-slot=\"form-cancel\"", StringComparison.Ordinal));
        await Assert.That(html.QuerySelector(".sa-button-variant-destructive")).IsNotNull();
    }

    [Test]
    [Arguments("products", false)]
    [Arguments("products", true)]
    [Arguments("users", false)]
    [Arguments("users", true)]
    [Arguments("roles", false)]
    [Arguments("roles", true)]
    public async Task ExecuteAsync_WhenResourceFormRenders_CancelReturnsToIndex(
        string route,
        bool edit
    )
    {
        // Arrange
        await using var app = await TestApplication.CreateAsync();
        var product = new Product { Name = "Lamp", Sku = "LAMP" };
        var role = new ApplicationRole { Name = "Existing", NormalizedName = "EXISTING" };
        var user = new ApplicationUser
        {
            UserName = "layout@example.com",
            Email = "layout@example.com",
        };
        app.Db.AddRange(product, role, user);
        await app.Db.SaveChangesAsync();
        var id =
            route == "products" ? product.Id.ToString()
            : route == "roles" ? role.Id
            : user.Id;
        var path = $"/admin/{route}/" + (edit ? $"Edit/{id}" : "Create");

        // Act
        using var html = await app.GetHtmlAsync(path);

        // Assert
        var cancel = html.QuerySelector("a[data-slot=form-cancel]");
        await Assert.That(cancel).IsNotNull();
        await Assert
            .That(cancel!.GetAttribute("href")!.ToLowerInvariant())
            .IsEqualTo($"/admin/{route}");
        await Assert.That(cancel.ClassList).Contains("sa-button-variant-outline");
    }
}
