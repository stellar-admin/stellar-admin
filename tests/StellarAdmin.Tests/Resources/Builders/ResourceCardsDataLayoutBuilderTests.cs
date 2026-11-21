using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using StellarAdmin.Diagnostics;
using StellarAdmin.Resources;
using StellarAdmin.Resources.Builders;
using StellarAdmin.Resources.Displays;
using StellarAdmin.Resources.Options;

namespace StellarAdmin.Tests.Resources.Builders;

public class ResourceCardsDataLayoutBuilderTests
{
    [Test]
    public async Task Stats_WithMoreThanFour_KeepsTheFirstFourAndReportsIt()
    {
        // Arrange
        var services = new ServiceCollection();
        ConfigureCards(
            services,
            card =>
                card.Stats(stats =>
                {
                    stats.Add(product => product.Id);
                    stats.Add(product => product.Name);
                    stats.Add(product => product.Price);
                    stats.Add(product => product.Stock);
                    stats.Add(product => product.Rating);
                })
        );
        using var provider = services.BuildServiceProvider();

        // Act
        var options = provider.GetRequiredService<IOptions<ResourceOptions<Product>>>().Value;

        // Assert
        var cards = options.Index.Cards.Single();
        await Assert
            .That(cards.Card.Stats.Select(stat => stat.FieldName))
            .IsEquivalentTo(
                ["Id", "Name", "Price", "Stock"],
                TUnit.Assertions.Enums.CollectionOrdering.Matching
            );
        var problem = options.Problems.Single();
        await Assert.That(problem.Kind).IsEqualTo(ConfigurationProblemKind.TooManyCardStats);
        await Assert.That(problem.Message).Contains("`Rating`");
    }

    [Test]
    public async Task FieldsLayout_SetsTheCardsFieldLayout()
    {
        // Arrange
        var services = new ServiceCollection();
        ConfigureCards(
            services,
            card =>
                card.Fields(fields =>
                {
                    fields.Layout = ResourceCardFieldLayout.Grid;
                    fields.Add(product => product.Name);
                })
        );
        using var provider = services.BuildServiceProvider();

        // Act
        var options = provider.GetRequiredService<IOptions<ResourceOptions<Product>>>().Value;

        // Assert
        var card = options.Index.Cards.Single().Card;
        await Assert.That(card.FieldLayout).IsEqualTo(ResourceCardFieldLayout.Grid);
        await Assert.That(card.Fields.Single().FieldName).IsEqualTo("Name");
    }

    [Test]
    public async Task OverlayArrangement_WithoutImage_MakesTheCardsVerticalAndReportsIt()
    {
        // Arrange
        var services = new ServiceCollection();
        ConfigureCards(services, card => card.Arrangement = ResourceCardArrangement.Overlay);
        using var provider = services.BuildServiceProvider();

        // Act
        var options = provider.GetRequiredService<IOptions<ResourceOptions<Product>>>().Value;

        // Assert
        await Assert
            .That(options.Index.Cards.Single().Card.Arrangement)
            .IsEqualTo(ResourceCardArrangement.Vertical);
        var problem = options.Problems.Single();
        await Assert.That(problem.Kind).IsEqualTo(ConfigurationProblemKind.CardOverlayWithoutImage);
        await Assert.That(problem.Level).IsEqualTo(ConfigurationProblemLevel.Warning);
    }

    [Test]
    public async Task BadgeUseDisplay_WithHandler_KeepsTheBadgeDisplayAndReportsIt()
    {
        // Arrange
        var services = new ServiceCollection();
        ConfigureCards(
            services,
            card => card.Badge(product => product.Name).UseDisplay<PreparedDisplay>()
        );
        using var provider = services.BuildServiceProvider();

        // Act
        var options = provider.GetRequiredService<IOptions<ResourceOptions<Product>>>().Value;

        // Assert
        await Assert.That(options.Index.Cards.Single().Card.Badge!.Display).IsNull();
        await Assert
            .That(options.Problems.Single().Kind)
            .IsEqualTo(ConfigurationProblemKind.CardDisplayWithHandler);
    }

    [Test]
    public async Task UseDisplay_WithHandler_KeepsTheDefaultDisplayAndReportsIt()
    {
        // Arrange
        var services = new ServiceCollection();
        ConfigureCards(
            services,
            card =>
                card.Fields(fields =>
                    fields.Add(product => product.Name).UseDisplay<PreparedDisplay>()
                )
        );
        using var provider = services.BuildServiceProvider();

        // Act
        var options = provider.GetRequiredService<IOptions<ResourceOptions<Product>>>().Value;

        // Assert
        await Assert.That(options.Index.Cards.Single().Card.Fields.Single().Display).IsNull();
        await Assert
            .That(options.Problems.Single().Kind)
            .IsEqualTo(ConfigurationProblemKind.CardDisplayWithHandler);
    }

    [Test]
    public async Task HandlerCards_WithoutTitleOrKey_ReportIt()
    {
        // Arrange
        var services = new ServiceCollection();
        services
            .AddStellarAdmin()
            .AddResource<Product>()
            .UseTitle(product => product.Name)
            .Index(index =>
            {
                index.AddCards("cards");
                index.AddCards<ProductTotal, ProductTotalsHandler>("totals");
            });
        using var provider = services.BuildServiceProvider();

        // Act
        var options = provider.GetRequiredService<IOptions<ResourceOptions<Product>>>().Value;

        // Assert
        var cards = options.Index.Cards.ToArray();
        await Assert
            .That(cards[0].Card.TitleField?.ToString())
            .IsEqualTo("product => product.Name");
        await Assert.That(cards[1].Card.TitleField).IsNull();
        var problem = options.Problems.Single();
        await Assert.That(problem.Kind).IsEqualTo(ConfigurationProblemKind.UntitledHandlerCards);
        await Assert.That(problem.Level).IsEqualTo(ConfigurationProblemLevel.Warning);
        await Assert.That(problem.Message).Contains("`totals`");
    }

    [Test]
    public async Task HandlerCards_WithRecordKey_DoNotReportAMissingTitle()
    {
        // Arrange
        var services = new ServiceCollection();
        services
            .AddStellarAdmin()
            .AddResource<Product>()
            .Index(index =>
                index.AddCards<ProductTotal, ProductTotalsHandler>(
                    "totals",
                    totals => totals.HasRecordKey(total => total.Name)
                )
            );
        using var provider = services.BuildServiceProvider();

        // Act
        var options = provider.GetRequiredService<IOptions<ResourceOptions<Product>>>().Value;

        // Assert
        await Assert.That(options.Problems).IsEmpty();
    }

    [Test]
    public async Task DefaultSortBy_WithComputedValue_LeavesTheSortUnsetAndReportsIt()
    {
        // Arrange
        var services = new ServiceCollection();
        services
            .AddStellarAdmin()
            .AddResource<Product>()
            .Index(index =>
                index.AddCards(
                    "cards",
                    cards => cards.DefaultSortBy(product => product.Name.ToUpperInvariant())
                )
            );
        using var provider = services.BuildServiceProvider();

        // Act
        var options = provider.GetRequiredService<IOptions<ResourceOptions<Product>>>().Value;

        // Assert
        await Assert.That(options.Index.Cards.Single().DefaultSort).IsNull();
        await Assert
            .That(options.Problems.Single().Kind)
            .IsEqualTo(ConfigurationProblemKind.InvalidSelector);
    }

    [Test]
    [Arguments(12, 12, false)]
    [Arguments(0, 25, true)]
    public async Task EnablePaging_WithPageSize_OffersOnlyThatPageSize(
        int pageSize,
        int expectedPageSize,
        bool isReported
    )
    {
        // Arrange
        ConfigurationProblemKind[] expectedProblems = isReported
            ? [ConfigurationProblemKind.InvalidDefaultPageSize]
            : [];
        var services = new ServiceCollection();
        services
            .AddStellarAdmin()
            .AddResource<Product>()
            .Index(index =>
                index.AddCards(
                    "cards",
                    cards => cards.EnablePaging(paging => paging.PageSize = pageSize)
                )
            );
        using var provider = services.BuildServiceProvider();

        // Act
        var options = provider.GetRequiredService<IOptions<ResourceOptions<Product>>>().Value;

        // Assert
        var paging = options.Index.Cards.Single().Paging!;
        await Assert.That(paging.PageSize).IsEqualTo(expectedPageSize);
        await Assert.That(paging.PageSizes).IsEquivalentTo([expectedPageSize]);
        await Assert
            .That(options.Problems.Select(problem => problem.Kind))
            .IsEquivalentTo(expectedProblems);
    }

    private static void ConfigureCards(
        IServiceCollection services,
        Action<ResourceCardBuilder<Product>> configure
    ) =>
        services
            .AddStellarAdmin()
            .AddResource<Product>()
            .Index(index => index.AddCards("cards", cards => cards.Card(configure)));

    public sealed class Product
    {
        public int Id { get; set; }

        public string Name { get; set; } = "";

        public decimal Price { get; set; }

        public double Rating { get; set; }

        public int Stock { get; set; }
    }

    public sealed record ProductTotal(string Name, int Count);

    public sealed class ProductTotalsHandler : IResourceDataLayoutHandler<ProductTotal>
    {
        public Task<ResourceListResult<ProductTotal>> ListAsync(
            ResourceListRequest request,
            CancellationToken cancellationToken
        ) => Task.FromResult(new ResourceListResult<ProductTotal>([], 0));
    }

    public sealed class PreparedDisplay : FieldDisplay, IFieldDisplay<PreparedDisplayHandler>
    {
        public override string TemplateName => "Prepared";
    }

    public sealed class PreparedDisplayHandler(PreparedDisplay display)
        : FieldDisplayHandler<PreparedDisplay>(display)
    {
        public override Task<object?> PrepareAsync(
            FieldDisplayContext context,
            CancellationToken cancellationToken
        ) => Task.FromResult<object?>(null);
    }
}
