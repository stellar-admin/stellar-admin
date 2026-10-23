using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using StellarAdmin.Diagnostics;
using StellarAdmin.Resources;
using StellarAdmin.Resources.Builders;
using StellarAdmin.Resources.Editors;
using StellarAdmin.Resources.Options;

namespace StellarAdmin.Tests.Resources.Options;

public class EditorCheckTests
{
    [Test]
    public async Task Run_WithBooleanEditorOnNullableBoolean_UsesDefaultEditor()
    {
        // Arrange
        using var provider = CreateProvider(fields =>
            fields.Add(tour => tour.Guided).UseToggleEditor()
        );

        // Act
        var options = GetOptions(provider);

        // Assert
        var problem = options.Problems.Single();
        await Assert.That(options.Create!.Fields.Single().Editor).IsNull();
        await Assert.That(problem.Kind).IsEqualTo(ConfigurationProblemKind.UnsuitableEditor);
    }

    [Test]
    public async Task Run_WithItemsOfAnotherValueType_UsesDefaultEditor()
    {
        // Arrange
        using var provider = CreateProvider(fields =>
            fields
                .Add(tour => tour.GuideCode)
                .UseSelectEditor(select => select.UseItems(new NumberItems()))
        );

        // Act
        var options = GetOptions(provider);

        // Assert
        var problem = options.Problems.Single();
        await Assert.That(options.Create!.Fields.Single().Editor).IsNull();
        await Assert.That(problem.Kind).IsEqualTo(ConfigurationProblemKind.UnsuitableEditor);
        await Assert.That(problem.Message).Contains("its items' values are `Int32`");
    }

    [Test]
    public async Task Run_WithItemsOfPropertyValueType_ReportsNothing()
    {
        // Arrange
        using var provider = CreateProvider(fields =>
        {
            fields
                .Add(tour => tour.GuideId)
                .UseSelectEditor(select => select.UseItems(new NumberItems()));
            fields
                .Add(tour => tour.StopIds)
                .UseCheckboxGroupEditor(group => group.UseItems(new NumberItems()));
        });

        // Act
        var options = GetOptions(provider);

        // Assert
        await Assert.That(options.Problems).IsEmpty();
    }

    [Test]
    public async Task Run_WithRadioClassNamesForMissingElements_ClearsThem()
    {
        // Arrange
        using var provider = CreateProvider(fields =>
            fields
                .Add(tour => tour.Level)
                .UseRadioGroupEditor(radio =>
                {
                    radio.ClassNames.Content = "gap-4";
                    radio.ClassNames.Option.Error = "text-red-600";
                })
        );

        // Act
        var options = GetOptions(provider);

        // Assert
        var radio = (RadioGroupEditor)options.Create!.Fields.Single().Editor!;
        await Assert.That(radio.ClassNames.Content).IsNull();
        await Assert.That(radio.ClassNames.Option.Error).IsNull();
    }

    [Test]
    public async Task Run_WithSourceArgumentOfAnotherType_UsesDefaultEditor()
    {
        // Arrange
        using var provider = CreateProvider(fields =>
            fields
                .Add(tour => tour.Airport)
                .UseLookupSheetEditor(lookup =>
                    lookup.UseSource<CountryAirportSource>(tour => tour.GuideCode)
                )
        );

        // Act
        var options = GetOptions(provider);

        // Assert
        var problem = options.Problems.Single();
        await Assert.That(options.Create!.Fields.Single().Editor).IsNull();
        await Assert
            .That(problem.Message)
            .Contains("searches with a `Int32` from the form, and the field passes it a `String`");
    }

    [Test]
    public async Task Run_WithSourceArgumentMissing_UsesDefaultEditor()
    {
        // Arrange
        using var provider = CreateProvider(fields =>
            fields
                .Add(tour => tour.Airport)
                .UseLookupSheetEditor(lookup => lookup.UseSource<CountryAirportSource>())
        );

        // Act
        var options = GetOptions(provider);

        // Assert
        var problem = options.Problems.Single();
        await Assert.That(options.Create!.Fields.Single().Editor).IsNull();
        await Assert.That(problem.Message).Contains("the field passes it none");
    }

    [Test]
    public async Task Run_WithSourceArgumentNotTaken_UsesDefaultEditor()
    {
        // Arrange
        using var provider = CreateProvider(fields =>
            fields
                .Add(tour => tour.Airport)
                .UseLookupSheetEditor(lookup =>
                    lookup.UseSource<AirportSource>(tour => tour.GuideId)
                )
        );

        // Act
        var options = GetOptions(provider);

        // Assert
        var problem = options.Problems.Single();
        await Assert.That(options.Create!.Fields.Single().Editor).IsNull();
        await Assert
            .That(problem.Message)
            .Contains("doesn't search with a value from the form");
    }

    [Test]
    public async Task Run_WithSourceOfAnotherValueType_UsesDefaultEditor()
    {
        // Arrange
        using var provider = CreateProvider(fields =>
            fields
                .Add(tour => tour.GuideId)
                .UseLookupSheetEditor(lookup => lookup.UseSource<AirportSource>())
        );

        // Act
        var options = GetOptions(provider);

        // Assert
        var problem = options.Problems.Single();
        await Assert.That(options.Create!.Fields.Single().Editor).IsNull();
        await Assert.That(problem.Message).Contains("its items' values are `String`");
    }

    [Test]
    public async Task Run_WithSourceOverridingTwoMediaMethods_UsesDefaultEditor()
    {
        // Arrange
        using var provider = CreateProvider(fields =>
            fields
                .Add(tour => tour.Airport)
                .UseLookupSheetEditor(lookup => lookup.UseSource<IconAndCodeAirportSource>())
        );

        // Act
        var options = GetOptions(provider);

        // Assert
        var problem = options.Problems.Single();
        await Assert.That(options.Create!.Fields.Single().Editor).IsNull();
        await Assert.That(problem.Message).Contains("overrides more than one media method");
    }

    [Test]
    public async Task Run_WithSuitableEditors_ReportsNothing()
    {
        // Arrange
        using var provider = CreateProvider(fields =>
        {
            fields.Add(tour => tour.Featured).UseToggleEditor();
            fields.Add(tour => tour.Level).UseSelectEditor();
            fields
                .Add(tour => tour.Airport)
                .UseLookupSheetEditor(lookup =>
                    lookup.UseSource<CountryAirportSource>(tour => tour.GuideId)
                );
        });

        // Act
        var options = GetOptions(provider);

        // Assert
        await Assert.That(options.Problems).IsEmpty();
    }

    private static ServiceProvider CreateProvider(Action<ResourceFieldsBuilder<Tour>> fields)
    {
        var services = new ServiceCollection();
        var resource = services.AddStellarAdmin().AddResource<Tour>();
        resource.UseDataSource<TourDataSource>();
        resource.AllowCreate(create => create.Fields(fields));

        return services.BuildServiceProvider();
    }

    private static ResourceOptions<Tour> GetOptions(ServiceProvider provider) =>
        provider.GetRequiredService<IOptions<ResourceOptions<Tour>>>().Value;

    public enum TourLevel
    {
        Easy,
        Hard,
    }

    public sealed record Airport(string Code, string Name);

    public class AirportSource : LookupSource<Airport, string>
    {
        public override Task<IReadOnlyCollection<Airport>> FindAsync(
            IReadOnlyCollection<string> values,
            CancellationToken cancellationToken
        ) => Task.FromResult<IReadOnlyCollection<Airport>>([]);

        public override string GetTitle(Airport item) => item.Name;

        public override string GetValue(Airport item) => item.Code;

        public override Task<LookupPage<Airport>> SearchAsync(
            LookupQuery query,
            CancellationToken cancellationToken
        ) => Task.FromResult(new LookupPage<Airport>([], false));
    }

    public sealed class CountryAirportSource : LookupSource<Airport, string, int>
    {
        public override Task<IReadOnlyCollection<Airport>> FindAsync(
            IReadOnlyCollection<string> values,
            CancellationToken cancellationToken
        ) => Task.FromResult<IReadOnlyCollection<Airport>>([]);

        public override string GetTitle(Airport item) => item.Name;

        public override string GetValue(Airport item) => item.Code;

        public override Task<LookupPage<Airport>> SearchAsync(
            int argument,
            LookupQuery query,
            CancellationToken cancellationToken
        ) => Task.FromResult(new LookupPage<Airport>([], false));
    }

    public sealed class IconAndCodeAirportSource : AirportSource
    {
        public override string? GetCode(Airport item) => item.Code;

        public override string? GetIcon(Airport item) => "plane";
    }

    // Integration items whose values are numbers
    public sealed class NumberItems : ChoiceItems
    {
        public override Type ValueType => typeof(int);

        public override Task<IReadOnlyList<ChoiceItem>> LoadAsync(
            object model,
            IServiceProvider services,
            CancellationToken cancellationToken
        ) => Task.FromResult<IReadOnlyList<ChoiceItem>>([new("1", "One")]);
    }

    public sealed class Tour
    {
        public string? Airport { get; set; }

        public bool Featured { get; set; }

        public string? GuideCode { get; set; }

        public int? GuideId { get; set; }

        public bool? Guided { get; set; }

        public TourLevel Level { get; set; }

        public List<int> StopIds { get; set; } = [];
    }

    public sealed class TourDataSource : IResourceDataSource<Tour>, IResourceCreateHandler<Tour>
    {
        public Task<ResourceOperationResult> CreateAsync(
            Tour model,
            CancellationToken cancellationToken
        ) => Task.FromResult(ResourceOperationResult.Success());

        public Task<ResourceListResult<Tour>> ListAsync(
            ResourceListRequest request,
            CancellationToken cancellationToken
        ) => Task.FromResult(new ResourceListResult<Tour>([], 0));
    }
}
