using System.Net;
using IdentitySimplePlayground.Data;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.Extensions.DependencyInjection;
using StellarAdmin.Dashboard.Testing;

namespace StellarAdmin.Dashboard.EntityFrameworkCore.IntegrationTests;

public partial class PlaygroundCompatibilityTests
{
    public static IEnumerable<(Type EntityType, string PropertyName)> MetadataProperties() =>
        new[] { typeof(Category), typeof(Product) }.SelectMany(type =>
            type.GetProperties().Select(property => (type, property.Name))
        );

    [Test]
    [MethodDataSource(nameof(MetadataProperties))]
    public async Task GetMetadataForProperties_WhenResourceMetadataIsConfigured_HasLabelAndDescription(
        Type entityType,
        string propertyName
    )
    {
        // Arrange
        await using var app = new TestApplication();
        var sut = app.Services.GetRequiredService<IModelMetadataProvider>();

        // Act
        var property = sut.GetMetadataForProperty(entityType, propertyName);

        // Assert
        await Assert.That(property.DisplayName).IsNotNullOrWhiteSpace();
        await Assert.That(property.Description).IsNotNullOrWhiteSpace();
    }

    [Test]
    [Arguments("/admin/Users")]
    [Arguments("/admin/Roles")]
    public async Task Index_WhenIdentityResourceIsRequested_StillRenders(string path)
    {
        // Arrange
        await using var app = await TestApplication.CreateAsync();

        // Act
        using var response = await app.Client.GetAsync(path);

        // Assert
        await Assert.That(response.StatusCode).IsEqualTo(HttpStatusCode.OK);
    }

    [Test]
    public async Task Migrate_WhenIdentityDataAlreadyExists_PreservesRoleAndModel()
    {
        // Arrange
        await using var app = new TestApplication();
        await app.Db.GetService<IMigrator>().MigrateAsync("00000000000000_CreateIdentitySchema");
        var role = new ApplicationRole { Name = "Existing", NormalizedName = "EXISTING" };
        app.Db.Roles.Add(role);
        await app.Db.SaveChangesAsync();

        // Act
        await app.Db.Database.MigrateAsync();

        // Assert
        await Assert.That(await app.Db.Roles.AnyAsync(value => value.Id == role.Id)).IsTrue();
        await Assert.That(app.Db.Database.HasPendingModelChanges()).IsFalse();
    }
}
