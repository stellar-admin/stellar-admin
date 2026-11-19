using System.Linq.Expressions;
using Microsoft.Extensions.DependencyInjection;
using StellarAdmin.EntityFrameworkCore.IntegrationTests.Fixtures;
using StellarAdmin.EntityFrameworkCore.IntegrationTests.Infrastructure;
using StellarAdmin.Resources;
using TUnit.Assertions.Enums;

namespace StellarAdmin.EntityFrameworkCore.IntegrationTests.Resources;

public class ResourceDataSourceTests
{
    [Test]
    public async Task List_WithFieldOfAnotherType_Throws()
    {
        // Arrange
        await using var sut = await EfCoreTestHost.CreateAsync();
        using var scope = sut.Services.CreateScope();
        var dataSource = scope.ServiceProvider.GetRequiredService<IResourceDataSource<Product>>();
        Expression<Func<Category, string>> name = category => category.Name;

        // Act
        var act = () => dataSource.ListAsync(new() { Fields = [name] }, CancellationToken.None);

        // Assert
        await Assert.That(act).Throws<ArgumentException>();
    }

    [Test]
    public async Task List_WithFields_IncludesTheNavigationsTheyRead()
    {
        // Arrange
        await using var sut = await EfCoreTestHost.CreateAsync();
        using var scope = sut.Services.CreateScope();
        var dataSource = scope.ServiceProvider.GetRequiredService<IResourceDataSource<Product>>();
        Expression<Func<Product, string?>> category = product => product.Category!.Name;

        // Act
        var result = await dataSource.ListAsync(
            new() { Fields = [category] },
            CancellationToken.None
        );

        // Assert
        await Assert
            .That(result.Items.Select(product => product.Category?.Name))
            .IsEquivalentTo(["Office", "Technology", "Technology", "Beverage"]);
    }

    [Test]
    public async Task List_WithoutFields_IncludesNothing()
    {
        // Arrange
        await using var sut = await EfCoreTestHost.CreateAsync(resource =>
            resource.Index(index =>
                index.Columns(columns => columns.Add(product => product.Category))
            )
        );
        using var scope = sut.Services.CreateScope();
        var dataSource = scope.ServiceProvider.GetRequiredService<IResourceDataSource<Product>>();

        // Act
        var result = await dataSource.ListAsync(new(), CancellationToken.None);

        // Assert
        await Assert.That(result.Items.Count).IsEqualTo(4);
        await Assert.That(result.Items.All(product => product.Category is null)).IsTrue();
    }

    [Test]
    public async Task List_WithSortSelector_OrdersByItThenByKey()
    {
        // Arrange
        await using var sut = await EfCoreTestHost.CreateAsync();
        using var scope = sut.Services.CreateScope();
        var dataSource = scope.ServiceProvider.GetRequiredService<IResourceDataSource<Product>>();
        Expression<Func<Product, string>> name = product => product.Name;

        // Act
        var result = await dataSource.ListAsync(
            new()
            {
                Sort = new("NotAColumn", ResourceSortDirection.Descending) { Selector = name },
            },
            CancellationToken.None
        );

        // Assert
        await Assert
            .That(result.Items.Select(product => product.Number))
            .IsEquivalentTo([1, 4, 2, 3], CollectionOrdering.Matching);
    }

    [Test]
    public async Task List_WithSortWithoutSelector_Throws()
    {
        // Arrange
        await using var sut = await EfCoreTestHost.CreateAsync();
        using var scope = sut.Services.CreateScope();
        var dataSource = scope.ServiceProvider.GetRequiredService<IResourceDataSource<Product>>();

        // Act
        var act = () =>
            dataSource.ListAsync(
                new() { Sort = new(nameof(Product.Name), ResourceSortDirection.Ascending) },
                CancellationToken.None
            );

        // Assert
        await Assert.That(act).Throws<InvalidOperationException>();
    }
}
