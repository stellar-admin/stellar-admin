using Microsoft.Extensions.DependencyInjection;

namespace StellarAdmin.Tests.Resources.Builders;

public class ResourceAddAnotherBuilderTests
{
    [Test]
    public async Task KeepForNext_WithComputedValue_ThrowsArgumentException()
    {
        // Arrange
        var services = new ServiceCollection();
        var resource = services.AddStellarAdmin().AddResource<Tour>();
        var sut = resource.AllowCreate().EnableAddAnother();

        // Act
        Action act = () => sut.KeepForNext(tour => tour.Name.ToUpperInvariant());

        // Assert
        await Assert.That(act).Throws<ArgumentException>();
    }

    public sealed class Tour
    {
        public string Name { get; set; } = "";
    }
}
