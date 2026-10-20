using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using StellarAdmin.Resources.Options;

namespace StellarAdmin.Tests.Resources.Builders;

public partial class ResourceBuilderTests
{
    [Test]
    public async Task EnableAddAnother_WhenKeptPropertyIsNoCreateField_RejectsConfiguration()
    {
        // Arrange
        var services = new ServiceCollection();
        var sut = services.AddStellarAdmin().AddResource<Destination>();
        sut.UseDataSource<DestinationDataSource>();
        sut.AllowCreate(create =>
        {
            create.Fields(fields => fields.Add(destination => destination.Name));
            create.EnableAddAnother(another =>
                another.KeepForNext(destination => destination.CountryId)
            );
        });
        using var provider = services.BuildServiceProvider();

        // Act
        Action act = () =>
            _ = provider.GetRequiredService<IOptions<ResourceOptions<Destination>>>().Value;

        // Assert
        var exception = await Assert.That(act).Throws<OptionsValidationException>();
        await Assert
            .That(exception!.Message)
            .Contains("KeepForNext names 'CountryId', which isn't an editable field");
    }
}
