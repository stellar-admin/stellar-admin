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
            fields.Add(tour => tour.Guided).UseEditor<ToggleEditor>()
        );

        // Act
        var options = GetOptions(provider);

        // Assert
        var problem = options.Problems.Single();
        await Assert.That(options.Create!.Fields.Single().Editor).IsNull();
        await Assert.That(problem.Kind).IsEqualTo(ConfigurationProblemKind.UnsuitableEditor);
    }

    [Test]
    public async Task Run_WithRadioClassNamesForMissingElements_ClearsThem()
    {
        // Arrange
        using var provider = CreateProvider(fields =>
            fields
                .Add(tour => tour.Level)
                .UseEditor<RadioGroupEditor>(radio =>
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
    public async Task Run_WithSuitableEditors_ReportsNothing()
    {
        // Arrange
        using var provider = CreateProvider(fields =>
        {
            fields.Add(tour => tour.Featured).UseEditor<ToggleEditor>();
            fields.Add(tour => tour.Level).UseEditor<SelectEditor>();
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

    public sealed class Tour
    {
        public bool Featured { get; set; }

        public bool? Guided { get; set; }

        public TourLevel Level { get; set; }
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
