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
    public async Task EnableInlineRow_WhenFieldMatchesNoColumn_TurnsInlineRowOffAndKeepsForm(
        string operation
    )
    {
        // Arrange
        var services = new ServiceCollection();
        var sut = services.AddStellarAdmin().AddResource<Destination>();
        sut.UseDataSource<DestinationDataSource>();
        sut.UseKey(destination => destination.Id);
        ConfigureInlineRow(sut, operation);
        sut.Index(index =>
            index.AddTable(
                "table",
                table =>
                {
                    table.Columns(columns =>
                    {
                        columns.Add(destination => destination.Name);
                        columns.Add(destination => destination.Country!.Name);
                    });
                    EnableInlineRow(table, operation);
                }
            )
        );
        using var provider = services.BuildServiceProvider();

        // Act
        var options = provider.GetRequiredService<IOptions<ResourceOptions<Destination>>>().Value;

        // Assert
        var table = options.Index.Tables.Single();
        await Assert
            .That(options.Problems.Single().Message)
            .Contains("has fields with no column: `CountryId`");
        await Assert.That(table.InlineCreate).IsNull();
        await Assert.That(table.InlineEditing).IsNull();
        object? form = operation == "edit" ? options.Edit : options.Create;
        await Assert.That(form).IsNotNull();
    }

    [Test]
    [Arguments("edit")]
    [Arguments("create")]
    public async Task EnableInlineRow_WhenColumnEditsField_AcceptsConfiguration(string operation)
    {
        // Arrange
        var services = new ServiceCollection();
        var sut = services.AddStellarAdmin().AddResource<Destination>();
        sut.UseDataSource<DestinationDataSource>();
        sut.UseKey(destination => destination.Id);
        ConfigureInlineRow(sut, operation);
        sut.Index(index =>
            index.AddTable(
                "table",
                table =>
                {
                    table.Columns(columns =>
                    {
                        columns.Add(destination => destination.Name);
                        columns
                            .Add(destination => destination.Country!.Name)
                            .EditsField(destination => destination.CountryId);
                    });
                    EnableInlineRow(table, operation);
                }
            )
        );
        using var provider = services.BuildServiceProvider();

        // Act
        var options = provider.GetRequiredService<IOptions<ResourceOptions<Destination>>>().Value;

        // Assert
        var table = options.Index.Tables.Single();
        object? inlineRow = operation == "edit" ? table.InlineEditing : table.InlineCreate;
        await Assert.That(inlineRow).IsNotNull();
    }

    [Test]
    public async Task EnableInlineCreate_WhenRequiredCreatePropertyHasNoField_TurnsInlineRowOff()
    {
        // Arrange
        var services = new ServiceCollection();
        var sut = services.AddStellarAdmin().AddResource<Destination>();
        sut.UseDataSource<DestinationDataSource>();
        sut.UseKey(destination => destination.Id);
        sut.AllowCreate(create =>
        {
            create.Fields(fields => fields.Add(destination => destination.CountryId));
        });
        sut.Index(index =>
            index.AddTable(
                "table",
                table =>
                {
                    table.Columns(columns =>
                    {
                        columns.Add(destination => destination.Name);
                        columns.Add(destination => destination.CountryId);
                    });
                    table.EnableInlineCreate();
                }
            )
        );
        using var provider = services.BuildServiceProvider();

        // Act
        var options = provider.GetRequiredService<IOptions<ResourceOptions<Destination>>>().Value;

        // Assert
        await Assert.That(options.Problems.Single().Message).Contains("required properties `Name`");
        await Assert.That(options.Index.Tables.Single().InlineCreate).IsNull();
        await Assert.That(options.Create).IsNotNull();
    }

    [Test]
    [Arguments("edit")]
    [Arguments("create")]
    public async Task EnableInlineRow_WithoutKey_TurnsInlineRowOff(string operation)
    {
        // Arrange
        var services = new ServiceCollection();
        var sut = services.AddStellarAdmin().AddResource<Destination>();
        sut.UseDataSource<DestinationDataSource>();
        ConfigureInlineRow(sut, operation);
        sut.Index(index =>
            index.AddTable(
                "table",
                table =>
                {
                    table.Columns(columns =>
                    {
                        columns.Add(destination => destination.Name);
                        columns.Add(destination => destination.CountryId);
                    });
                    EnableInlineRow(table, operation);
                }
            )
        );
        using var provider = services.BuildServiceProvider();

        // Act
        var options = provider.GetRequiredService<IOptions<ResourceOptions<Destination>>>().Value;

        // Assert
        var table = options.Index.Tables.Single();
        await Assert.That(options.Problems.Single().Message).Contains("needs a resource key");
        await Assert.That(table.InlineCreate).IsNull();
        await Assert.That(table.InlineEditing).IsNull();
    }

    [Test]
    public async Task EnableInlineEditing_WithSeparateModel_MatchesFieldsByName()
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
        });
        sut.Index(index =>
            index.AddTable(
                "table",
                table =>
                {
                    table.Columns(columns =>
                    {
                        columns.Add(destination => destination.Name);
                        columns
                            .Add(destination => destination.Country!.Name)
                            .EditsField("CountryCode");
                    });
                    table.EnableInlineEditing(inline => inline.DimOtherRows = false);
                }
            )
        );
        using var provider = services.BuildServiceProvider();

        // Act
        var options = provider.GetRequiredService<IOptions<ResourceOptions<Destination>>>().Value;

        // Assert
        await Assert.That(options.Problems).IsEmpty();
        await Assert.That(options.Index.Tables.Single().InlineEditing?.DimOtherRows).IsFalse();
    }

    [Test]
    [Arguments("edit")]
    [Arguments("create")]
    public async Task EnableInlineRow_WithoutForm_TurnsInlineRowOff(string operation)
    {
        // Arrange
        var services = new ServiceCollection();
        var sut = services.AddStellarAdmin().AddResource<Destination>();
        sut.UseDataSource<DestinationDataSource>();
        sut.UseKey(destination => destination.Id);
        sut.Index(index =>
            index.AddTable(
                "table",
                table =>
                {
                    table.Columns(columns => columns.Add(destination => destination.Name));
                    EnableInlineRow(table, operation);
                }
            )
        );
        using var provider = services.BuildServiceProvider();

        // Act
        var options = provider.GetRequiredService<IOptions<ResourceOptions<Destination>>>().Value;

        // Assert
        var table = options.Index.Tables.Single();
        await Assert
            .That(options.Problems.Single().Message)
            .Contains($"needs the resource's {operation} form");
        await Assert.That(table.InlineCreate).IsNull();
        await Assert.That(table.InlineEditing).IsNull();
    }

    private static void EnableInlineRow(
        ResourceTableDataLayoutBuilder<Destination> table,
        string operation
    )
    {
        if (operation == "edit")
        {
            table.EnableInlineEditing();
        }
        else
        {
            table.EnableInlineCreate();
        }
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
