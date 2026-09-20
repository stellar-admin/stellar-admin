using Microsoft.Extensions.DependencyInjection;

namespace StellarAdmin.Dashboard.Tests.Resources.Builders;

public class ResourceFieldsBuilderTests
{
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
            sut.AllowCreate(create =>
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
        public string Name { get; set; } = "";
        public string ReadOnly => Name;
    }
}
