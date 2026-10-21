using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using StellarAdmin.Resources.Options;

namespace StellarAdmin.Tests.Resources.Builders;

public partial class ResourceBuilderTests
{
    [Test]
    public async Task EnableAddAnother_WhenKeptPropertyIsNoCreateField_IgnoresProperty()
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
        var options = provider.GetRequiredService<IOptions<ResourceOptions<Destination>>>().Value;

        // Assert
        await Assert
            .That(options.Problems.Single().Message)
            .Contains("KeepForNext names 'CountryId', which isn't an editable field");
        await Assert.That(options.Create!.AddAnother!.KeptFields).IsEmpty();
    }
}
