using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using StellarAdmin.Diagnostics;
using StellarAdmin.Resources;
using StellarAdmin.Resources.Options;

namespace StellarAdmin.Tests.Resources.Builders;

public class ResourceIndexBuilderBaseTests
{
    [Test]
    [Arguments("")]
    [Arguments(" ")]
    public async Task EnableRowDetails_WithBlankPartialName_DoesNotExpandRows(string partialName)
    {
        // Arrange
        var services = new ServiceCollection();
        var resource = services.AddStellarAdmin().AddResource<Product>();
        resource.Index(index => index.EnableRowDetails(partialName));
        using var provider = services.BuildServiceProvider();

        // Act
        var options = provider.GetRequiredService<IOptions<ResourceOptions<Product>>>().Value;

        // Assert
        await Assert.That(options.Index.RowDetails).IsNull();
        await Assert
            .That(options.Problems.Single().Kind)
            .IsEqualTo(ConfigurationProblemKind.RowDetailsWithoutPanels);
    }

    [Test]
    public async Task EnableRowDetails_WithRegisteredHandler_KeepsApplicationLifetime()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddSingleton<ProductNotesHandler>();
        var resource = services.AddStellarAdmin().AddResource<Product>();

        // Act
        resource.Index(index =>
            index.EnableRowDetails<ProductNotes, ProductNotesHandler>("_ProductNotes")
        );

        // Assert
        var registration = services.Single(service =>
            service.ServiceType == typeof(ProductNotesHandler)
        );
        await Assert.That(registration.Lifetime).IsEqualTo(ServiceLifetime.Singleton);
    }

    [Test]
    public async Task EnableRowDetails_WithPanels_RegistersPanelsInOrder()
    {
        // Arrange
        var services = new ServiceCollection();
        var resource = services.AddStellarAdmin().AddResource<Product>();
        resource.UseKey(product => product.Id);

        // Act
        resource.Index(index =>
            index.EnableRowDetails(panels =>
            {
                panels.Add("Summary", "_ProductSummary");
                panels.Add<ProductNotes, ProductNotesHandler>("Notes", "_ProductNotes");
            })
        );

        // Assert
        using var provider = services.BuildServiceProvider();
        var rowDetails = provider
            .GetRequiredService<IOptions<ResourceOptions<Product>>>()
            .Value.Index.RowDetails!;
        await Assert
            .That(rowDetails.Panels.Select(panel => (panel.Name, panel.Title, panel.LoadsOnDemand)))
            .IsEquivalentTo(
                new (string?, string?, bool)[]
                {
                    ("summary", "Summary", false),
                    ("notes", "Notes", true),
                }
            );
    }

    [Test]
    public async Task EnableRowDetails_WithoutPanels_DoesNotExpandRows()
    {
        // Arrange
        var services = new ServiceCollection();
        var resource = services.AddStellarAdmin().AddResource<Product>();
        resource.Index(index => index.EnableRowDetails(panels => { }));
        using var provider = services.BuildServiceProvider();

        // Act
        var options = provider.GetRequiredService<IOptions<ResourceOptions<Product>>>().Value;

        // Assert
        await Assert.That(options.Index.RowDetails).IsNull();
        await Assert
            .That(options.Problems.Single().Kind)
            .IsEqualTo(ConfigurationProblemKind.RowDetailsWithoutPanels);
    }

    [Test]
    public async Task EnableRowDetails_WithDuplicatePanelNames_LeavesOutLaterPanel()
    {
        // Arrange
        var services = new ServiceCollection();
        var resource = services.AddStellarAdmin().AddResource<Product>();
        resource.Index(index =>
            index.EnableRowDetails(panels =>
            {
                panels.Add("Order history", "_OrderHistory");
                panels.Add("Orders", "_Orders").Name = "order-history";
            })
        );
        using var provider = services.BuildServiceProvider();

        // Act
        var options = provider.GetRequiredService<IOptions<ResourceOptions<Product>>>().Value;

        // Assert
        await Assert
            .That(options.Index.RowDetails!.Panels.Select(panel => panel.Title))
            .IsEquivalentTo(["Order history"]);
        await Assert.That(options.Problems.Single().Message).Contains("order-history");
    }

    [Test]
    [Arguments("?!", "_Panel", "")]
    [Arguments("Panel", " ", "")]
    [Arguments(" ", "_Panel", "")]
    [Arguments("Panel", "_Panel", " ")]
    public async Task EnableRowDetails_WithBlankPanelValue_LeavesOutPanel(
        string title,
        string partialName,
        string name
    )
    {
        // Arrange
        var services = new ServiceCollection();
        var resource = services.AddStellarAdmin().AddResource<Product>();
        resource.Index(index =>
            index.EnableRowDetails(panels =>
            {
                panels.Add("Notes", "_Notes");
                var panel = panels.Add(title, partialName);
                if (name.Length > 0)
                {
                    panel.Name = name;
                }
            })
        );
        using var provider = services.BuildServiceProvider();

        // Act
        var options = provider.GetRequiredService<IOptions<ResourceOptions<Product>>>().Value;

        // Assert
        await Assert
            .That(options.Index.RowDetails!.Panels.Select(panel => panel.Title))
            .IsEquivalentTo(["Notes"]);
        await Assert
            .That(options.Problems.Single().Kind)
            .IsEqualTo(ConfigurationProblemKind.RowDetailPanelWithoutName);
    }

    public sealed class Product
    {
        public int Id { get; set; }
    }

    public sealed record ProductNotes(string Notes);

    public sealed class ProductNotesHandler : IResourceRowDetailHandler<ProductNotes>
    {
        public Task<ProductNotes?> FindAsync(string id, CancellationToken cancellationToken) =>
            Task.FromResult<ProductNotes?>(new("Notes"));
    }
}
