using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using StellarAdmin.Diagnostics;
using StellarAdmin.Resources;
using StellarAdmin.Resources.Displays;
using StellarAdmin.Resources.Options;
using StellarAdmin.UI;

namespace StellarAdmin.Tests.Resources.Builders;

public class ResourceFieldsBuilderTests
{
    [Test]
    public async Task AddDetails_WithFields_BuildsReadOnlySection()
    {
        // Arrange
        var services = new ServiceCollection();
        var sut = services.AddStellarAdmin().AddResource<Product>();
        sut.UseDataSource<ProductDataSource>();
        sut.AllowCreate(create =>
            create.Fields(fields =>
                fields.AddDetails(
                    "Audit",
                    details =>
                    {
                        details.Description = "Read-only values.";
                        details.Orientation = DetailsOrientation.Horizontal;
                        details.Variant = DetailsVariant.Divided;
                        details.Add(product => product.ReadOnly, field => field.Title = "Copy");
                        details.Add(product => product.Name).EmptyText = "None";
                    }
                )
            )
        );
        using var provider = services.BuildServiceProvider();

        // Act
        var options = provider.GetRequiredService<IOptions<ResourceOptions<Product>>>().Value;

        // Assert
        var details = (FormDetailsOptions)options.Create!.Items[0];
        await Assert.That(details.Title).IsEqualTo("Audit");
        await Assert.That(details.Description).IsEqualTo("Read-only values.");
        await Assert.That(details.Orientation).IsEqualTo(DetailsOrientation.Horizontal);
        await Assert.That(details.Variant).IsEqualTo(DetailsVariant.Divided);
        await Assert
            .That(details.Fields.Select(field => field.FieldName))
            .IsEquivalentTo(["ReadOnly", "Name"]);
        await Assert.That(details.Fields[0].Title).IsEqualTo("Copy");
        await Assert.That(details.Fields[1].EmptyText).IsEqualTo("None");
        await Assert.That(options.Create.Fields).IsEmpty();
    }

    [Test]
    [Arguments("")]
    [Arguments(" ")]
    public async Task AddDetails_WithBlankTitle_ReportsProblem(string title)
    {
        // Arrange
        var services = new ServiceCollection();
        var sut = services.AddStellarAdmin().AddResource<Product>();
        sut.UseDataSource<ProductDataSource>();
        sut.AllowCreate(create => create.Fields(fields => fields.AddDetails(title)));
        using var provider = services.BuildServiceProvider();

        // Act
        var options = provider.GetRequiredService<IOptions<ResourceOptions<Product>>>().Value;

        // Assert
        await Assert.That(options.Create!.Items).HasCount(1);
    }

    [Test]
    public async Task AddDisplayField_WithGetterOnlyProperty_AddsUnboundField()
    {
        // Arrange
        var services = new ServiceCollection();
        var sut = services.AddStellarAdmin().AddResource<Product>();
        sut.UseDataSource<ProductDataSource>();
        sut.AllowCreate(create =>
            create.Fields(fields =>
            {
                fields.Add(product => product.Name);
                fields.AddDisplayField(
                    product => product.ReadOnly,
                    field =>
                    {
                        field.Title = "Copy";
                        field.EmptyText = "None";
                        field.UseDisplay<ReferenceDisplay>(display => display.Prefix = "#");
                    }
                );
            })
        );
        using var provider = services.BuildServiceProvider();

        // Act
        var options = provider.GetRequiredService<IOptions<ResourceOptions<Product>>>().Value;

        // Assert
        var field = (FormDisplayFieldOptions)options.Create!.Items[1];
        await Assert.That(field.FieldName).IsEqualTo("ReadOnly");
        await Assert.That(field.Title).IsEqualTo("Copy");
        await Assert.That(field.EmptyText).IsEqualTo("None");
        await Assert.That(((ReferenceDisplay)field.Display!).Prefix).IsEqualTo("#");
        await Assert.That(options.Create.Fields.Select(f => f.FieldName)).IsEquivalentTo(["Name"]);
    }

    [Test]
    [Arguments("display")]
    [Arguments("details")]
    public async Task AddDisplayField_WithMethodExpression_AddsComputedField(string scope)
    {
        // Arrange
        var services = new ServiceCollection();
        var sut = services.AddStellarAdmin().AddResource<Product>();
        sut.UseDataSource<ProductDataSource>();
        sut.AllowCreate(create =>
            create.Fields(fields =>
            {
                if (scope == "display")
                {
                    fields.AddDisplayField(
                        product => product.Name.ToUpper(),
                        field => field.Title = "Shouted"
                    );
                }
                else
                {
                    fields
                        .AddDetails("Audit")
                        .Add(product => product.Name.ToUpper(), field => field.Title = "Shouted");
                }
            })
        );
        using var provider = services.BuildServiceProvider();

        // Act
        var options = provider.GetRequiredService<IOptions<ResourceOptions<Product>>>().Value;

        // Assert
        var field = options.Create!.DisplayFields.Single();
        await Assert.That(field.FieldName).IsNull();
        await Assert.That(field.PropertyPath).IsEmpty();
        await Assert.That(field.GetValue(new Product { Name = "tent" })).IsEqualTo("TENT");
    }

    [Test]
    [Arguments(true)]
    [Arguments(false)]
    public async Task UseBuiltInRendering_AndUseDisplay_LastCallWins(bool builtInLast)
    {
        // Arrange
        var services = new ServiceCollection();
        var sut = services.AddStellarAdmin().AddResource<Product>();
        sut.UseDataSource<ProductDataSource>();
        sut.AllowCreate(create =>
            create.Fields(fields =>
            {
                var field = fields.AddDisplayField(product => product.Name);
                if (builtInLast)
                {
                    field.UseDisplay<ReferenceDisplay>().UseBuiltInRendering();
                }
                else
                {
                    field.UseBuiltInRendering().UseDisplay<ReferenceDisplay>();
                }
            })
        );
        using var provider = services.BuildServiceProvider();

        // Act
        var options = provider.GetRequiredService<IOptions<ResourceOptions<Product>>>().Value;

        // Assert
        var field = (FormDisplayFieldOptions)options.Create!.Items[0];
        await Assert.That(field.UsesBuiltInRendering).IsEqualTo(builtInLast);
        await Assert.That(field.Display is ReferenceDisplay).IsEqualTo(!builtInLast);
    }

    [Test]
    public async Task UseDisplay_WithHandlerForAnotherDisplay_KeepsDefaultDisplay()
    {
        // Arrange
        var services = new ServiceCollection();
        var sut = services.AddStellarAdmin().AddResource<Product>();
        sut.UseDataSource<ProductDataSource>();
        sut.AllowCreate(create =>
            create.Fields(fields =>
                fields.AddDisplayField(product => product.Name).UseDisplay<MismatchedDisplay>()
            )
        );
        using var provider = services.BuildServiceProvider();

        // Act
        var options = provider.GetRequiredService<IOptions<ResourceOptions<Product>>>().Value;

        // Assert
        await Assert.That(options.Create!.DisplayFields.Single().Display).IsNull();
        await Assert
            .That(options.Problems.Single().Kind)
            .IsEqualTo(ConfigurationProblemKind.MismatchedHandler);
    }

    [Test]
    public async Task Add_WithMethodCall_LeavesOutField()
    {
        // Arrange
        var services = new ServiceCollection();
        var sut = services.AddStellarAdmin().AddResource<Product>();
        sut.UseDataSource<ProductDataSource>();
        sut.AllowCreate(create =>
            create.Fields(fields => fields.Add(product => product.Name.ToUpper()))
        );
        using var provider = services.BuildServiceProvider();

        // Act
        var options = provider.GetRequiredService<IOptions<ResourceOptions<Product>>>().Value;

        // Assert
        await Assert.That(options.Create!.Fields).IsEmpty();
        await Assert
            .That(options.Problems.Single().Kind)
            .IsEqualTo(ConfigurationProblemKind.InvalidSelector);
    }

    [Test]
    [Arguments("nested")]
    [Arguments("readOnly")]
    public async Task Add_WithoutSetterOrBinder_LeavesOutField(string expression)
    {
        // Arrange
        var services = new ServiceCollection();
        var sut = services.AddStellarAdmin().AddResource<Product>();
        sut.UseDataSource<ProductDataSource>();
        sut.AllowCreate(create =>
            create.Fields(fields =>
            {
                if (expression == "nested")
                {
                    fields.Add(product => product.Name.Length);
                }
                else
                {
                    fields.Add(product => product.ReadOnly);
                }
            })
        );
        using var provider = services.BuildServiceProvider();

        // Act
        var options = provider.GetRequiredService<IOptions<ResourceOptions<Product>>>().Value;

        // Assert
        await Assert.That(options.Create!.Fields).IsEmpty();
        await Assert.That(options.Problems.Single().Message).Contains("has no public setter");
    }

    [Test]
    public async Task Add_WithoutSetterAndWithBinder_PassesOptionsValidation()
    {
        // Arrange
        var binder = new ReadOnlyBinder();
        var services = new ServiceCollection();
        var sut = services.AddStellarAdmin().AddResource<Product>();
        sut.UseDataSource<ProductDataSource>();
        sut.AllowCreate(create => create.Fields(fields => fields.Add(product => product.ReadOnly)));
        services
            .AddOptions<ResourceOptions<Product>>()
            .PostConfigure(options => options.Create!.Fields[0].UseBinder(binder));
        using var provider = services.BuildServiceProvider();

        // Act
        var options = provider.GetRequiredService<IOptions<ResourceOptions<Product>>>().Value;

        // Assert
        await Assert.That(options.Create!.Fields[0].Binder).IsSameReferenceAs(binder);
    }

    [Test]
    [Arguments("columns")]
    [Arguments("columnTier")]
    [Arguments("fieldSpan")]
    [Arguments("spanTier")]
    [Arguments("groupSpan")]
    [Arguments("sectionSpan")]
    [Arguments("fieldStart")]
    [Arguments("startTier")]
    [Arguments("groupStart")]
    [Arguments("sectionStart")]
    [Arguments("displayFieldSpan")]
    [Arguments("displayFieldStart")]
    [Arguments("detailsColumns")]
    [Arguments("detailsSpan")]
    [Arguments("detailsStart")]
    public async Task Fields_WithGridValueOutOfRange_ClampsValue(string setting)
    {
        // Arrange
        var services = new ServiceCollection();
        var sut = services.AddStellarAdmin().AddResource<Product>();
        sut.UseDataSource<ProductDataSource>();
        sut.AllowCreate(create =>
            create.Fields(fields =>
            {
                switch (setting)
                {
                    case "columns":
                        fields.Columns(0);
                        break;
                    case "columnTier":
                        fields.Columns(columns => columns.Large(13));
                        break;
                    case "fieldSpan":
                        fields.Add(product => product.Name).ColumnSpan(0);
                        break;
                    case "spanTier":
                        fields.Add(product => product.Name).ColumnSpan(span => span.Small(13));
                        break;
                    case "groupSpan":
                        fields.AddGroup().ColumnSpan(0);
                        break;
                    case "sectionSpan":
                        fields.AddSection("Details").ColumnSpan(13);
                        break;
                    case "fieldStart":
                        fields.Add(product => product.Name).ColumnStart(0);
                        break;
                    case "startTier":
                        fields.Add(product => product.Name).ColumnStart(start => start.Medium(13));
                        break;
                    case "groupStart":
                        fields.AddGroup().ColumnStart(13);
                        break;
                    case "sectionStart":
                        fields.AddSection("Details").ColumnStart(0);
                        break;
                    case "displayFieldSpan":
                        fields.AddDisplayField(product => product.Name).ColumnSpan(0);
                        break;
                    case "displayFieldStart":
                        fields.AddDisplayField(product => product.Name).ColumnStart(13);
                        break;
                    case "detailsColumns":
                        fields.AddDetails("Audit").Columns(13);
                        break;
                    case "detailsSpan":
                        fields.AddDetails("Audit").ColumnSpan(0);
                        break;
                    case "detailsStart":
                        fields.AddDetails("Audit").ColumnStart(13);
                        break;
                }
            })
        );
        using var provider = services.BuildServiceProvider();

        // Act
        var options = provider.GetRequiredService<IOptions<ResourceOptions<Product>>>().Value;

        // Assert
        var problem = options.Problems.Single();
        await Assert.That(problem.Kind).IsEqualTo(ConfigurationProblemKind.InvalidValue);
    }

    public sealed class Product
    {
        public string Name { get; set; } = "";
        public string ReadOnly => Name;
    }

    public sealed class ReadOnlyBinder : FieldBinder
    {
        public override Task BindAsync(
            FieldBindingContext context,
            CancellationToken cancellationToken
        ) => Task.CompletedTask;

        public override object? GetValue(FieldBindingContext context) => null;
    }

    public sealed class ReferenceDisplay : FieldDisplay
    {
        public string Prefix { get; set; } = "";

        public override string TemplateName => "Reference";
    }

    public sealed class MismatchedDisplay : FieldDisplay, IFieldDisplay<OtherDisplayHandler>
    {
        public override string TemplateName => "Mismatched";
    }

    public sealed class OtherDisplay : FieldDisplay
    {
        public override string TemplateName => "Other";
    }

    public sealed class OtherDisplayHandler(OtherDisplay display)
        : FieldDisplayHandler<OtherDisplay>(display)
    {
        public override Task<object?> PrepareAsync(
            FieldDisplayContext context,
            CancellationToken cancellationToken
        ) => Task.FromResult<object?>(null);
    }

    public sealed class ProductDataSource
        : IResourceDataSource<Product>,
            IResourceCreateHandler<Product>
    {
        public Task<ResourceOperationResult> CreateAsync(
            Product model,
            CancellationToken cancellationToken
        ) => Task.FromResult(ResourceOperationResult.Success());

        public Task<ResourceListResult<Product>> ListAsync(
            ResourceListRequest request,
            CancellationToken cancellationToken
        ) => Task.FromResult(new ResourceListResult<Product>([], 0));
    }
}
