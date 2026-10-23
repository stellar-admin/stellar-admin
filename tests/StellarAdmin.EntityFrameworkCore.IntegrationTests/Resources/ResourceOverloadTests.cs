using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using StellarAdmin.EntityFrameworkCore.IntegrationTests.Fixtures;
using StellarAdmin.Resources.Editors;
using TUnit.Assertions;
using TUnit.Core;

namespace StellarAdmin.EntityFrameworkCore.IntegrationTests.Resources;

public class ResourceOverloadTests
{
    [Test]
    public async Task ChoiceUseItems_WithNullConfigure_ThrowsArgumentNullException()
    {
        // Arrange
        var sut = new SelectEditor();

        // Act
        Action act = () =>
            sut.UseItems<Category, int>(category => category.Id, category => category.Name, null!);

        // Assert
        await Assert.That(act).Throws<ArgumentNullException>();
    }

    [Test]
    public async Task LookupUseItems_WithNullConfigure_ThrowsArgumentNullException()
    {
        // Arrange
        var sut = new LookupSheetEditor();

        // Act
        Action act = () =>
            sut.UseItems<Category, int>(category => category.Id, category => category.Name, null!);

        // Assert
        await Assert.That(act).Throws<ArgumentNullException>();
    }

    [Test]
    public async Task MultiLookupUseItems_WithNullConfigure_ThrowsArgumentNullException()
    {
        // Arrange
        var sut = new MultiLookupSheetEditor();

        // Act
        Action act = () =>
            sut.UseItems<Category, int>(category => category.Id, category => category.Name, null!);

        // Assert
        await Assert.That(act).Throws<ArgumentNullException>();
    }

    [Test]
    public async Task SearchOn_WithNullInMoreSelectors_ThrowsArgumentException()
    {
        // Arrange
        var sut = new LookupSheetEditor();

        // Act
        Action act = () =>
            sut.UseItems<Category, int>(
                category => category.Id,
                category => category.Name,
                items => items.SearchOn(category => category.Name, null!, null!)
            );

        // Assert
        await Assert.That(act).Throws<ArgumentException>();
    }

    [Test]
    public async Task SearchOn_WithNullSelector_ThrowsArgumentNullException()
    {
        // Arrange
        var sut = new LookupSheetEditor();

        // Act
        Action act = () =>
            sut.UseItems<Category, int>(
                category => category.Id,
                category => category.Name,
                items => items.SearchOn(null!)
            );

        // Assert
        await Assert.That(act).Throws<ArgumentNullException>();
    }

    [Test]
    public async Task ScopesAdd_WithNullPredicate_ThrowsArgumentNullException()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddDbContext<CatalogDbContext>(options =>
            options.UseSqlite("Data Source=:memory:")
        );

        // Act
        Action act = () =>
            services.AddStellarAdmin(admin =>
                admin.AddEfCoreResource<Product>(resource =>
                    resource.Index(index =>
                        index.EnableScopes(scopes => scopes.Add("all", "All products", null!))
                    )
                )
            );

        // Assert
        await Assert.That(act).Throws<ArgumentNullException>();
    }
}
