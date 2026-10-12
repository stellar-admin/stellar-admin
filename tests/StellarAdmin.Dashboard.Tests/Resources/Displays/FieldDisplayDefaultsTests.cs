using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.Extensions.DependencyInjection;
using StellarAdmin.Dashboard.Resources.Displays;

namespace StellarAdmin.Dashboard.Tests.Resources.Displays;

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

    private static ModelMetadata GetMetadata(string property)
    {
        var services = new ServiceCollection();
        services.AddMvcCore().AddDataAnnotations();
        using var provider = services.BuildServiceProvider();

        return provider
            .GetRequiredService<IModelMetadataProvider>()
            .GetMetadataForProperty(typeof(Model), property);
    }

    public sealed class Model
    {
        public bool Active { get; set; }

        public Location? Address { get; set; }

        public bool? Archived { get; set; }

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

    public sealed class Location;

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
