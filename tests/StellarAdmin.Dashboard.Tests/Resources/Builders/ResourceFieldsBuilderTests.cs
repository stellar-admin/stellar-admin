using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using StellarAdmin.Dashboard.Resources.Options;

namespace StellarAdmin.Dashboard.Tests.Resources.Builders;

public class ResourceFieldsBuilderTests
{
    [Test]
    public async Task Clear_WithEarlierFields_ReplacesFieldsThroughOptionsPipeline()
    {
        // Arrange
        var services = new ServiceCollection();
        var sut = services.AddStellarAdmin().AddDashboard().AddResource<Product>();
        sut.Create(create => create.Fields(fields => fields.Add(product => product.Id)));

        // Act
        sut.Create(create =>
            create.Fields(fields =>
            {
                fields.Clear();
                fields.Add(product => product.Name, field => field.Title = "Product name");
            })
        );
        using var provider = services.BuildServiceProvider();
        var options = provider.GetRequiredService<IOptions<ResourceOptions<Product>>>().Value;

        // Assert
        await Assert.That(options.Create.Fields.Count).IsEqualTo(1);
        await Assert.That(options.Create.Fields[0].FieldName).IsEqualTo("Name");
        await Assert.That(options.Create.Fields[0].Title).IsEqualTo("Product name");
    }

    [Test]
    public async Task Add_WithSeparateProviders_CreatesIndependentFieldOptions()
    {
        // Arrange
        var services = new ServiceCollection();
        var sut = services.AddStellarAdmin().AddDashboard().AddResource<Product>();
        sut.Create(create => create.Fields(fields => fields.Add(product => product.Name)));
        using var first = services.BuildServiceProvider();
        using var second = services.BuildServiceProvider();

        // Act
        var firstOptions = first.GetRequiredService<IOptions<ResourceOptions<Product>>>().Value;
        var secondOptions = second.GetRequiredService<IOptions<ResourceOptions<Product>>>().Value;

        // Assert
        await Assert
            .That(firstOptions.Create.Fields[0])
            .IsNotSameReferenceAs(secondOptions.Create.Fields[0]);
        await Assert
            .That(firstOptions.Create.Fields[0].Editor)
            .IsNotSameReferenceAs(secondOptions.Create.Fields[0].Editor);
    }

    [Test]
    [Arguments("nested")]
    [Arguments("method")]
    [Arguments("readOnly")]
    public async Task Add_WithUnsupportedExpression_RejectsField(string expression)
    {
        // Arrange
        var services = new ServiceCollection();
        var sut = services.AddStellarAdmin().AddDashboard().AddResource<Product>();

        // Act
        Action act = () =>
            sut.Create(create =>
                create.Fields(fields =>
                {
                    switch (expression)
                    {
                        case "nested":
                            fields.Add(product => product.Name.Length);
                            break;
                        case "method":
                            fields.Add(product => product.Name.ToUpper());
                            break;
                        case "readOnly":
                            fields.Add(product => product.ReadOnly);
                            break;
                    }
                })
            );

        // Assert
        await Assert.That(act).Throws<ArgumentException>();
    }

    public sealed class Product
    {
        public int Id { get; set; }
        public string Name { get; set; } = "";
        public string ReadOnly => Name;
    }
}
