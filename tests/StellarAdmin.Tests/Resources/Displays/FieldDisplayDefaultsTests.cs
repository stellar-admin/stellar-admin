using System.ComponentModel.DataAnnotations;
using System.Linq.Expressions;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.Extensions.DependencyInjection;
using StellarAdmin.Resources.Builders;
using StellarAdmin.Resources.Displays;
using StellarAdmin.Resources.Options;

namespace StellarAdmin.Tests.Resources.Displays;

public class FieldDisplayDefaultsTests
{
    [Test]
    [Arguments(nameof(Model.Email), typeof(EmailDisplay))]
    [Arguments(nameof(Model.Website), typeof(LinkDisplay))]
    [Arguments(nameof(Model.Phone), typeof(PhoneDisplay))]
    [Arguments(nameof(Model.Updated), typeof(DateDisplay))]
    [Arguments(nameof(Model.Published), typeof(DateDisplay))]
    [Arguments(nameof(Model.Released), typeof(DateDisplay))]
    [Arguments(nameof(Model.Opens), typeof(DateDisplay))]
    [Arguments(nameof(Model.Closed), typeof(DateDisplay))]
    [Arguments(nameof(Model.Active), typeof(BooleanDisplay))]
    [Arguments(nameof(Model.Archived), typeof(BooleanDisplay))]
    [Arguments(nameof(Model.Status), typeof(EnumDisplay))]
    [Arguments(nameof(Model.PreviousStatus), typeof(EnumDisplay))]
    [Arguments(nameof(Model.Quantity), typeof(NumberDisplay))]
    [Arguments(nameof(Model.Price), typeof(NumberDisplay))]
    [Arguments(nameof(Model.Rating), typeof(NumberDisplay))]
    [Arguments(nameof(Model.Name), typeof(TextDisplay))]
    [Arguments(nameof(Model.Notes), typeof(TextDisplay))]
    public async Task Resolve_WithKnownType_ReturnsDefaultDisplay(string property, Type expected)
    {
        // Arrange
        var metadata = GetMetadata(property);

        // Act
        var display = FieldDisplayDefaults.Resolve(metadata);

        // Assert
        await Assert.That(display?.GetType()).IsEqualTo(expected);
    }

    [Test]
    [Arguments(nameof(Model.Reference))]
    [Arguments(nameof(Model.Tags))]
    [Arguments(nameof(Model.Days))]
    [Arguments(nameof(Model.Address))]
    public async Task Resolve_WithOtherType_ReturnsNull(string property)
    {
        // Arrange
        var metadata = GetMetadata(property);

        // Act
        var display = FieldDisplayDefaults.Resolve(metadata);

        // Assert
        await Assert.That(display).IsNull();
    }

    [Test]
    public async Task Resolve_WithColumnWithoutDisplay_ReturnsDefaultForLeafProperty()
    {
        // Arrange
        var column = BuildColumn(model => model.Address!.City, _ => { });

        // Act
        var display = FieldDisplayDefaults.Resolve(column, CreateMetadataProvider());

        // Assert
        await Assert.That(display).IsTypeOf<TextDisplay>();
    }

    [Test]
    public async Task Resolve_WithColumnDisplay_ReturnsColumnDisplay()
    {
        // Arrange
        var column = BuildColumn(
            model => model.Price,
            builder => builder.UseDisplay<TextDisplay>(text => text.LineClamp = 2)
        );

        // Act
        var display = FieldDisplayDefaults.Resolve(column, CreateMetadataProvider());

        // Assert
        await Assert.That(display).IsSameReferenceAs(column.Display);
    }

    [Test]
    public async Task Resolve_WithColumnUsingBuiltInRendering_ReturnsNull()
    {
        // Arrange
        var column = BuildColumn(model => model.Price, builder => builder.UseBuiltInRendering());

        // Act
        var display = FieldDisplayDefaults.Resolve(column, CreateMetadataProvider());

        // Assert
        await Assert.That(display).IsNull();
    }

    [Test]
    public async Task Resolve_WithColumnPropertyHavingUIHint_ReturnsNull()
    {
        // Arrange
        var column = BuildColumn(model => model.Code, _ => { });

        // Act
        var display = FieldDisplayDefaults.Resolve(column, CreateMetadataProvider());

        // Assert
        await Assert.That(display).IsNull();
    }

    private static DataGridColumnOptions BuildColumn<TProperty>(
        Expression<Func<Model, TProperty>> field,
        Action<ResourceColumnBuilder<Model>> configure
    )
    {
        var builder = new ResourceColumnBuilder<Model>(field);
        configure(builder);

        return builder.Build();
    }

    private static IModelMetadataProvider CreateMetadataProvider()
    {
        var services = new ServiceCollection();
        services.AddMvcCore().AddDataAnnotations();

        return services.BuildServiceProvider().GetRequiredService<IModelMetadataProvider>();
    }

    private static ModelMetadata GetMetadata(string property) =>
        CreateMetadataProvider().GetMetadataForProperty(typeof(Model), property);

    public sealed class Model
    {
        public bool Active { get; set; }

        public Location? Address { get; set; }

        public bool? Archived { get; set; }

        [UIHint("Code")]
        public string? Code { get; set; }

        public DateTimeOffset? Closed { get; set; }

        public DayOfWeekFlags Days { get; set; }

        [EmailAddress]
        public string? Email { get; set; }

        public string? Name { get; set; }

        [DataType(DataType.MultilineText)]
        public string? Notes { get; set; }

        public TimeOnly Opens { get; set; }

        [Phone]
        public string? Phone { get; set; }

        public ProductStatus? PreviousStatus { get; set; }

        public decimal Price { get; set; }

        [DataType(DataType.Date)]
        public DateTime Published { get; set; }

        public int Quantity { get; set; }

        public double? Rating { get; set; }

        public Guid Reference { get; set; }

        public DateOnly Released { get; set; }

        public ProductStatus Status { get; set; }

        public string[] Tags { get; set; } = [];

        public DateTime Updated { get; set; }

        [Url]
        public string? Website { get; set; }
    }

    public sealed class Location
    {
        public string? City { get; set; }
    }

    [Flags]
    public enum DayOfWeekFlags
    {
        None = 0,
        Monday = 1,
        Tuesday = 2,
    }

    public enum ProductStatus
    {
        Draft,
        Published,
    }
}
