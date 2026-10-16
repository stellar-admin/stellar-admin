using System.ComponentModel.DataAnnotations;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using StellarAdmin.Resources;
using StellarAdmin.Resources.Builders;
using StellarAdmin.Resources.Options;

namespace StellarAdmin.Tests.Resources.Builders;

public partial class ResourceBuilderTests
{
    [Test]
    [Arguments("edit")]
    [Arguments("create")]
    public async Task UseInlineRow_WhenFieldMatchesNoColumn_RejectsConfiguration(string operation)
    {
        // Arrange
        var services = new ServiceCollection();
        var sut = services.AddStellarAdmin().AddResource<Destination>();
        sut.UseDataSource<DestinationDataSource>();
        sut.UseKey(destination => destination.Id);
        ConfigureInlineRow(sut, operation);
        sut.Index(index =>
            index.Columns(columns =>
            {
                columns.Add(destination => destination.Name);
                columns.Add(destination => destination.Country!.Name);
            })
        );
        using var provider = services.BuildServiceProvider();

        // Act
        Action act = () =>
            _ = provider.GetRequiredService<IOptions<ResourceOptions<Destination>>>().Value;

        // Assert
        var exception = await Assert.That(act).Throws<OptionsValidationException>();
        await Assert.That(exception!.Message).Contains("'CountryId' matches no index column");
    }

    [Test]
    [Arguments("edit")]
    [Arguments("create")]
    public async Task UseInlineRow_WhenColumnEditsField_AcceptsConfiguration(string operation)
    {
        // Arrange
        var services = new ServiceCollection();
        var sut = services.AddStellarAdmin().AddResource<Destination>();
        sut.UseDataSource<DestinationDataSource>();
        sut.UseKey(destination => destination.Id);
        ConfigureInlineRow(sut, operation);
        sut.Index(index =>
            index.Columns(columns =>
            {
                columns.Add(destination => destination.Name);
                columns
                    .Add(destination => destination.Country!.Name)
                    .EditsField(destination => destination.CountryId);
            })
        );
        using var provider = services.BuildServiceProvider();

        // Act
        var options = provider.GetRequiredService<IOptions<ResourceOptions<Destination>>>().Value;

        // Assert
        object? inlineRow =
            operation == "edit" ? options.Edit?.InlineRow : options.Create?.InlineRow;
        await Assert.That(inlineRow).IsNotNull();
    }

    [Test]
    public async Task UseInlineRow_WhenRequiredCreatePropertyHasNoField_RejectsConfiguration()
    {
        // Arrange
        var services = new ServiceCollection();
        var sut = services.AddStellarAdmin().AddResource<Destination>();
        sut.UseDataSource<DestinationDataSource>();
        sut.UseKey(destination => destination.Id);
        sut.AllowCreate(create =>
        {
            create.Fields(fields => fields.Add(destination => destination.CountryId));
            create.UseInlineRow();
        });
        sut.Index(index =>
            index.Columns(columns =>
            {
                columns.Add(destination => destination.Name);
                columns.Add(destination => destination.CountryId);
            })
        );
        using var provider = services.BuildServiceProvider();

        // Act
        Action act = () =>
            _ = provider.GetRequiredService<IOptions<ResourceOptions<Destination>>>().Value;

        // Assert
        var exception = await Assert.That(act).Throws<OptionsValidationException>();
        await Assert.That(exception!.Message).Contains("required property 'Name'");
    }

    [Test]
    [Arguments("edit")]
    [Arguments("create")]
    public async Task UseInlineRow_WithoutKey_RejectsConfiguration(string operation)
    {
        // Arrange
        var services = new ServiceCollection();
        var sut = services.AddStellarAdmin().AddResource<Destination>();
        sut.UseDataSource<DestinationDataSource>();
        ConfigureInlineRow(sut, operation);
        sut.Index(index =>
            index.Columns(columns =>
            {
                columns.Add(destination => destination.Name);
                columns.Add(destination => destination.CountryId);
            })
        );
        using var provider = services.BuildServiceProvider();

        // Act
        Action act = () =>
            _ = provider.GetRequiredService<IOptions<ResourceOptions<Destination>>>().Value;

        // Assert
        var exception = await Assert.That(act).Throws<OptionsValidationException>();
        await Assert.That(exception!.Message).Contains("Inline rows require a resource key");
    }

    [Test]
    public async Task UseInlineRow_WithSeparateModel_MatchesFieldsByName()
    {
        // Arrange
        var services = new ServiceCollection();
        var sut = services.AddStellarAdmin().AddResource<Destination>();
        sut.UseDataSource<DestinationDataSource>();
        sut.UseKey(destination => destination.Id);
        sut.AllowEdit<DestinationEditModel, DestinationEditHandler>(edit =>
        {
            edit.Fields(fields =>
            {
                fields.Add(model => model.Name);
                fields.Add(model => model.CountryCode);
            });
            edit.UseInlineRow(inline => inline.DimOtherRows = false);
        });
        sut.Index(index =>
            index.Columns(columns =>
            {
                columns.Add(destination => destination.Name);
                columns.Add(destination => destination.Country!.Name).EditsField("CountryCode");
            })
        );
        using var provider = services.BuildServiceProvider();

        // Act
        var options = provider.GetRequiredService<IOptions<ResourceOptions<Destination>>>().Value;

        // Assert
        await Assert.That(options.Edit?.InlineRow?.DimOtherRows).IsFalse();
    }

    private static void ConfigureInlineRow(ResourceBuilder<Destination> sut, string operation)
    {
        if (operation == "edit")
        {
            sut.AllowEdit(edit =>
            {
                edit.Fields(fields =>
                {
                    fields.Add(destination => destination.Name);
                    fields.Add(destination => destination.CountryId);
                });
                edit.UseInlineRow();
            });
        }
        else
        {
            sut.AllowCreate(create =>
            {
                create.Fields(fields =>
                {
                    fields.Add(destination => destination.Name);
                    fields.Add(destination => destination.CountryId);
                });
                create.UseInlineRow();
            });
        }
    }

    public sealed class Country
    {
        public string Name { get; set; } = "";
    }

    public sealed class Destination
    {
        public Country? Country { get; set; }

        public int CountryId { get; set; }

        public int Id { get; set; }

        [Required]
        public string Name { get; set; } = "";
    }

    public sealed class DestinationDataSource : IResourceCrudDataSource<Destination>
    {
        public Task<ResourceOperationResult> CreateAsync(
            Destination resource,
            CancellationToken cancellationToken
        ) => Task.FromResult(ResourceOperationResult.Success());

        public Task<ResourceOperationResult> DeleteAsync(
            string id,
            CancellationToken cancellationToken
        ) => Task.FromResult(ResourceOperationResult.NotFound());

        public Task<Destination?> FindAsync(string id, CancellationToken cancellationToken) =>
            Task.FromResult<Destination?>(null);

        public Task<ResourceListResult<Destination>> ListAsync(
            ResourceListRequest request,
            CancellationToken cancellationToken
        ) => Task.FromResult(new ResourceListResult<Destination>([], 0));

        public Task<ResourceOperationResult> UpdateAsync(
            string id,
            Destination resource,
            CancellationToken cancellationToken
        ) => Task.FromResult(ResourceOperationResult.NotFound());
    }

    public sealed class DestinationEditHandler : IResourceEditHandler<DestinationEditModel>
    {
        public Task<DestinationEditModel?> FindAsync(
            string id,
            CancellationToken cancellationToken
        ) => Task.FromResult<DestinationEditModel?>(null);

        public Task<ResourceOperationResult> UpdateAsync(
            string id,
            DestinationEditModel model,
            CancellationToken cancellationToken
        ) => Task.FromResult(ResourceOperationResult.NotFound());
    }

    public sealed class DestinationEditModel
    {
        public string CountryCode { get; set; } = "";

        public string Name { get; set; } = "";
    }
}
