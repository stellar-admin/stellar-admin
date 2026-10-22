using System.ComponentModel.DataAnnotations;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using StellarAdmin.Diagnostics;
using StellarAdmin.Resources;
using StellarAdmin.Resources.Builders;
using StellarAdmin.Resources.Displays;
using StellarAdmin.Resources.Options;

namespace StellarAdmin.Tests.Resources.Options;

public class ValueTextCheckTests
{
    [Test]
    public async Task Run_WhenColumnShowsCollectionWithoutText_LeavesItOut()
    {
        // Arrange
        using var provider = CreateProvider(resource =>
            resource.Index(index =>
                index.Columns(columns => columns.Add(tour => tour.Destinations))
            )
        );

        // Act
        var options = GetOptions(provider);

        // Assert
        var problem = options.Problems.First(candidate =>
            candidate.Kind == ConfigurationProblemKind.ValueWithoutText
        );
        await Assert
            .That(problem.Message)
            .Contains(
                "The `Destinations` column isn't shown because its value is a collection of `Destination`, which has no text of its own."
            );
    }

    [Test]
    public async Task Run_WhenColumnShowsReferenceWithoutText_LeavesItOut()
    {
        // Arrange
        using var provider = CreateProvider(resource =>
            resource.Index(index => index.Columns(columns => columns.Add(tour => tour.Country)))
        );

        // Act
        var options = GetOptions(provider);

        // Assert
        var problem = options.Problems.First(candidate =>
            candidate.Kind == ConfigurationProblemKind.ValueWithoutText
        );
        await Assert
            .That(problem.Message)
            .Contains(
                "The `Country` column isn't shown because its value is a `Country`, which has no text of its own."
            );
        await Assert.That(problem.Fix).Contains("Show one of the `Country`'s properties instead");
    }

    [Test]
    public async Task Run_WhenColumnUsesBuiltInDisplayWithoutText_LeavesItOut()
    {
        // Arrange
        using var provider = CreateProvider(resource =>
            resource.Index(index =>
                index.Columns(columns =>
                    columns.Add(
                        tour => tour.Destinations,
                        column => column.UseDisplay<BadgeDisplay>()
                    )
                )
            )
        );

        // Act
        var options = GetOptions(provider);

        // Assert
        var problem = options.Problems.First(candidate =>
            candidate.Kind == ConfigurationProblemKind.ValueWithoutText
        );
    }

    [Test]
    [Arguments("view")]
    [Arguments("create")]
    [Arguments("edit")]
    public async Task Run_WhenDisplayFieldShowsCollectionWithoutText_LeavesItOut(string page)
    {
        // Arrange
        using var provider = CreateProvider(resource =>
        {
            switch (page)
            {
                case "view":
                    resource.AllowView(view =>
                        view.Fields(fields => fields.Add(tour => tour.Destinations))
                    );
                    break;
                case "create":
                    resource.AllowCreate(create =>
                        create.Fields(fields =>
                        {
                            fields.Add(tour => tour.Name);
                            fields.AddDetails("Route").Add(tour => tour.Destinations);
                        })
                    );
                    break;
                default:
                    resource.AllowEdit(edit =>
                        edit.Fields(fields =>
                        {
                            fields.Add(tour => tour.Name);
                            fields.AddDetails("Route").Add(tour => tour.Destinations);
                        })
                    );
                    break;
            }
        });

        // Act
        var options = GetOptions(provider);

        // Assert
        var problem = options.Problems.First(candidate =>
            candidate.Kind == ConfigurationProblemKind.ValueWithoutText
        );
        await Assert.That(problem.Message).Contains("its value is a collection of `Destination`");
    }

    [Test]
    public async Task Run_WhenValuesHaveTextOrADisplayThatShowsThem_KeepsIt()
    {
        // Arrange
        using var provider = CreateProvider(resource =>
            resource.Index(index =>
                index.Columns(columns =>
                {
                    columns.Add(
                        tour => tour.Destinations,
                        column =>
                            column.UseDisplay<BadgeDisplay>(badge =>
                                badge.UseText<Destination>(destination => destination.Name)
                            )
                    );
                    columns.Add(
                        tour => tour.Country,
                        column =>
                            column.UseDisplay<BadgeDisplay>(badge =>
                                badge.UseText<Country>(country => country.Name)
                            )
                    );
                    columns.Add(tour => tour.Guides, column => column.UseDisplay<GuideDisplay>());
                    columns.Add(tour => tour.Stops);
                    columns.Add(tour => tour.Highlights);
                    columns.Add(tour => tour.Days);
                    columns.Add(tour => tour.Leader);
                })
            )
        );

        // Act
        var options = GetOptions(provider);

        // Assert
        await Assert.That(options.Index.Columns.Count).IsEqualTo(7);
    }

    [Test]
    public async Task Run_WhenColumnShowsResourceRecord_KeepsIt()
    {
        // Arrange
        using var provider = CreateProvider(
            resource =>
                resource.Index(index =>
                    index.Columns(columns => columns.Add(tour => tour.Country))
                ),
            admin => admin.AddResource<Country>()
        );

        // Act
        var options = GetOptions(provider);

        // Assert
        await Assert
            .That(
                options.Problems.Any(problem =>
                    problem.Kind == ConfigurationProblemKind.ValueWithoutText
                )
            )
            .IsFalse();
        await Assert.That(options.Index.Columns.Count).IsEqualTo(1);
    }

    [Test]
    public async Task Run_WhenColumnShowsResourceRecordsInBadges_KeepsIt()
    {
        // Arrange
        using var provider = CreateProvider(
            resource =>
                resource.Index(index =>
                    index.Columns(columns =>
                        columns.Add(
                            tour => tour.Destinations,
                            column => column.UseDisplay<BadgeDisplay>()
                        )
                    )
                ),
            admin => admin.AddResource<Destination>()
        );

        // Act
        var options = GetOptions(provider);

        // Assert
        await Assert.That(options.Index.Columns.Count).IsEqualTo(1);
    }

    [Test]
    public async Task Run_WhenColumnShowsResourceRecordsWithoutBadges_LeavesItOut()
    {
        // Arrange
        using var provider = CreateProvider(
            resource =>
                resource.Index(index =>
                    index.Columns(columns => columns.Add(tour => tour.Destinations))
                ),
            admin => admin.AddResource<Destination>()
        );

        // Act
        var options = GetOptions(provider);

        // Assert
        var problem = options.Problems.First(candidate =>
            candidate.Kind == ConfigurationProblemKind.ValueWithoutText
        );
        await Assert
            .That(problem.Message)
            .Contains(
                "The `Destinations` column isn't shown because its value is a collection of `Destination`, whose titles show only in badges."
            );
        await Assert.That(problem.Fix).IsEqualTo("Call `UseDisplay<BadgeDisplay>()`.");
    }

    private static ServiceProvider CreateProvider(Action<ResourceBuilder<Tour>> configure) =>
        CreateProvider(configure, _ => { });

    private static ServiceProvider CreateProvider(
        Action<ResourceBuilder<Tour>> configure,
        Action<StellarAdminBuilder> configureAdmin
    )
    {
        var services = new ServiceCollection();
        var admin = services.AddStellarAdmin();
        var resource = admin.AddResource<Tour>();
        resource.UseDataSource<TourDataSource>();
        resource.UseKey(tour => tour.Id);
        configure(resource);
        configureAdmin(admin);

        return services.BuildServiceProvider();
    }

    private static ResourceOptions<Tour> GetOptions(ServiceProvider provider) =>
        provider.GetRequiredService<IOptions<ResourceOptions<Tour>>>().Value;

    public sealed class Country
    {
        public string Name { get; set; } = "";
    }

    public sealed class Destination
    {
        public string Name { get; set; } = "";
    }

    public sealed class Guide
    {
        public string Name { get; set; } = "";
    }

    public sealed class GuideDisplay : FieldDisplay
    {
        public override string TemplateName => "Guides";
    }

    public sealed class Leader
    {
        public string Name { get; set; } = "";
    }

    public sealed class Stop
    {
        public string Name { get; set; } = "";

        public override string ToString() => Name;
    }

    public sealed class Tour
    {
        public Country? Country { get; set; }

        public List<int> Days { get; set; } = [];

        public List<Destination> Destinations { get; set; } = [];

        public List<Guide> Guides { get; set; } = [];

        public List<string> Highlights { get; set; } = [];

        public int Id { get; set; }

        [UIHint("Leader")]
        public Leader? Leader { get; set; }

        [Required]
        public string Name { get; set; } = "";

        public List<Stop> Stops { get; set; } = [];
    }

    public sealed class TourDataSource : IResourceCrudDataSource<Tour>
    {
        public Task<ResourceOperationResult> CreateAsync(
            Tour resource,
            CancellationToken cancellationToken
        ) => Task.FromResult(ResourceOperationResult.Success());

        public Task<ResourceOperationResult> DeleteAsync(
            string id,
            CancellationToken cancellationToken
        ) => Task.FromResult(ResourceOperationResult.NotFound());

        public Task<Tour?> FindAsync(string id, CancellationToken cancellationToken) =>
            Task.FromResult<Tour?>(null);

        public Task<ResourceListResult<Tour>> ListAsync(
            ResourceListRequest request,
            CancellationToken cancellationToken
        ) => Task.FromResult(new ResourceListResult<Tour>([], 0));

        public Task<ResourceOperationResult> UpdateAsync(
            string id,
            Tour resource,
            CancellationToken cancellationToken
        ) => Task.FromResult(ResourceOperationResult.NotFound());
    }
}
