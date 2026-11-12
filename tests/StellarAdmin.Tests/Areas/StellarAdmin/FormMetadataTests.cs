using System.Collections;
using System.ComponentModel.DataAnnotations;
using System.Reflection;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.ModelBinding.Metadata;
using Microsoft.Extensions.DependencyInjection;
using StellarAdmin.Areas.StellarAdmin;

namespace StellarAdmin.Tests.Areas.StellarAdmin;

public class FormMetadataTests
{
    [Test]
    public async Task Create_PropertyMetadata_AnswersAsFrameworkMetadataExceptDisplayName()
    {
        // Arrange
        var (provider, detailsProvider) = CreateProviders();
        var framework = provider.GetMetadataForProperty(typeof(Trip), nameof(Trip.Passengers));
        var members = typeof(DefaultModelMetadata)
            .GetProperties(BindingFlags.Public | BindingFlags.Instance)
            .Where(property =>
                property.GetIndexParameters().Length == 0
                && property.Name
                    is not nameof(ModelMetadata.DisplayName)
                        and not nameof(ModelMetadata.Properties)
            )
            .ToArray();
        var labels = new Dictionary<string, string> { ["Passengers"] = "Travellers" };

        // Act
        var sut = FormMetadata.Create(provider, detailsProvider, typeof(Trip), labels).Properties[
            nameof(Trip.Passengers)
        ]!;

        // Assert
        var differences = members
            .Where(member => !AreEqual(member.GetValue(sut), member.GetValue(framework)))
            .Select(member => member.Name)
            .ToArray();
        await Assert.That(sut).IsTypeOf<FormMetadata>();
        await Assert.That(differences).IsEmpty();
    }

    [Test]
    public async Task Create_SamePropertyUnderTwoPaths_NamesEachByItsLabel()
    {
        // Arrange
        var (provider, detailsProvider) = CreateProviders();
        var labels = new Dictionary<string, string>
        {
            ["Billing.Street"] = "Billing street",
            ["Shipping.Street"] = "Shipping street",
        };

        // Act
        var sut = FormMetadata.Create(provider, detailsProvider, typeof(Trip), labels);

        // Assert
        await Assert
            .That(sut.Properties["Billing"]!.Properties["Street"]!.GetDisplayName())
            .IsEqualTo("Billing street");
        await Assert
            .That(sut.Properties["Shipping"]!.Properties["Street"]!.GetDisplayName())
            .IsEqualTo("Shipping street");
    }

    [Test]
    public async Task Create_PropertyOffFieldPaths_IsFrameworkMetadata()
    {
        // Arrange
        var (provider, detailsProvider) = CreateProviders();
        var labels = new Dictionary<string, string> { ["Billing.Street"] = "Billing street" };

        // Act
        var sut = FormMetadata.Create(provider, detailsProvider, typeof(Trip), labels);

        // Assert
        await Assert.That(sut.Properties["Billing"]).IsTypeOf<FormMetadata>();
        await Assert
            .That(sut.Properties["Shipping"])
            .IsSameReferenceAs(provider.GetMetadataForProperty(typeof(Trip), "Shipping"));
    }

    [Test]
    public async Task Create_LeavesFrameworkMetadataUnnamed()
    {
        // Arrange
        var (provider, detailsProvider) = CreateProviders();
        var labels = new Dictionary<string, string> { ["DepartureDate"] = "Leaves on" };

        // Act
        var sut = FormMetadata.Create(provider, detailsProvider, typeof(Trip), labels);

        // Assert
        await Assert
            .That(sut.Properties[nameof(Trip.DepartureDate)]!.GetDisplayName())
            .IsEqualTo("Leaves on");
        await Assert
            .That(
                provider
                    .GetMetadataForProperty(typeof(Trip), nameof(Trip.DepartureDate))
                    .DisplayName
            )
            .IsNull();
    }

    // A collection, such as the validator metadata, is a new wrapper of the same items
    private static bool AreEqual(object? value, object? other) =>
        value is IEnumerable values and not string && other is IEnumerable others
            ? values.Cast<object?>().SequenceEqual(others.Cast<object?>())
            : Equals(value, other);

    private static (IModelMetadataProvider, ICompositeMetadataDetailsProvider) CreateProviders()
    {
        var services = new ServiceCollection()
            .AddLogging()
            .AddMvcCore()
            .AddDataAnnotations()
            .Services.BuildServiceProvider();

        return (
            services.GetRequiredService<IModelMetadataProvider>(),
            services.GetRequiredService<ICompositeMetadataDetailsProvider>()
        );
    }

    private sealed class Address
    {
        [Required]
        public string? Street { get; set; }
    }

    private sealed class Trip
    {
        public Address Billing { get; set; } = new();

        public DateTime DepartureDate { get; set; }

        [Display(Name = "Number of passengers"), Range(1, 9)]
        public int Passengers { get; set; }

        public Address Shipping { get; set; } = new();
    }
}
