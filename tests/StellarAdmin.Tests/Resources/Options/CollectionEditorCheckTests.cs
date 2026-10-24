using System.ComponentModel.DataAnnotations;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using StellarAdmin.Resources;
using StellarAdmin.Resources.Editors;
using StellarAdmin.Resources.Options;

namespace StellarAdmin.Tests.Resources.Options;

public class CollectionEditorCheckTests
{
    [Test]
    [Arguments(nameof(Excursion.Highlights))]
    [Arguments(nameof(Excursion.DepartureDays))]
    [Arguments(nameof(Excursion.Prices))]
    public async Task Run_WithListOfTextOrNumbers_UsesListEditor(string property)
    {
        // Arrange
        using var provider = CreateProvider();

        // Act
        var field = GetField(provider, property);

        // Assert
        await Assert.That(field.Editor).IsTypeOf<ListEditor>();
        await Assert.That(field.HandlerType).IsNull();
    }

    [Test]
    [Arguments(nameof(Excursion.LocalNames))]
    [Arguments(nameof(Excursion.SeasonalPrices))]
    public async Task Run_WithDictionaryOfSimpleValues_UsesDictionaryEditor(string property)
    {
        // Arrange
        using var provider = CreateProvider();

        // Act
        var field = GetField(provider, property);

        // Assert
        await Assert.That(field.Editor).IsTypeOf<DictionaryEditor>();
        await Assert.That(field.HandlerType).IsEqualTo(typeof(DictionaryEditorHandler));
    }

    [Test]
    [Arguments(nameof(Excursion.Seasons))]
    [Arguments(nameof(Excursion.GuideIds))]
    [Arguments(nameof(Excursion.Photo))]
    [Arguments(nameof(Excursion.Stops))]
    [Arguments(nameof(Excursion.Codes))]
    public async Task Run_WithOtherProperty_KeepsDefaultEditor(string property)
    {
        // Arrange
        using var provider = CreateProvider();

        // Act
        var field = GetField(provider, property);

        // Assert
        await Assert.That(field.Editor).IsNull();
    }

    // Every property is a field, so that each test reads its own
    private static ServiceProvider CreateProvider()
    {
        var services = new ServiceCollection();
        var resource = services.AddStellarAdmin().AddResource<Excursion>();
        resource.UseDataSource<ExcursionDataSource>();
        resource.AllowCreate(create =>
            create.Fields(fields =>
            {
                fields.Add(excursion => excursion.Codes);
                fields.Add(excursion => excursion.DepartureDays);
                fields.Add(excursion => excursion.GuideIds);
                fields.Add(excursion => excursion.Highlights);
                fields.Add(excursion => excursion.LocalNames);
                fields.Add(excursion => excursion.Photo);
                fields.Add(excursion => excursion.Prices);
                fields.Add(excursion => excursion.SeasonalPrices);
                fields.Add(excursion => excursion.Seasons);
                fields.Add(excursion => excursion.Stops);
            })
        );

        return services.BuildServiceProvider();
    }

    private static FormFieldOptions GetField(ServiceProvider provider, string property) =>
        provider
            .GetRequiredService<IOptions<ResourceOptions<Excursion>>>()
            .Value.Create!.Fields.Single(field => field.FieldName == property);

    public enum Season
    {
        Summer,
        Winter,
    }

    public sealed class Excursion
    {
        // A [UIHint] names the template the app wants
        [UIHint("Codes")]
        public List<string> Codes { get; set; } = [];

        public int[] DepartureDays { get; set; } = [];

        public List<Guid> GuideIds { get; set; } = [];

        public IReadOnlyList<string> Highlights { get; set; } = [];

        public IDictionary<string, string> LocalNames { get; set; } =
            new Dictionary<string, string>();

        // Binary data, not a list of numbers
        public byte[] Photo { get; set; } = [];

        public List<decimal> Prices { get; set; } = [];

        public Dictionary<Season, decimal?> SeasonalPrices { get; set; } = [];

        public List<Season> Seasons { get; set; } = [];

        public Dictionary<string, List<string>> Stops { get; set; } = [];
    }

    public sealed class ExcursionDataSource
        : IResourceDataSource<Excursion>,
            IResourceCreateHandler<Excursion>
    {
        public Task<ResourceOperationResult> CreateAsync(
            Excursion model,
            CancellationToken cancellationToken
        ) => Task.FromResult(ResourceOperationResult.Success());

        public Task<ResourceListResult<Excursion>> ListAsync(
            ResourceListRequest request,
            CancellationToken cancellationToken
        ) => Task.FromResult(new ResourceListResult<Excursion>([], 0));
    }
}
