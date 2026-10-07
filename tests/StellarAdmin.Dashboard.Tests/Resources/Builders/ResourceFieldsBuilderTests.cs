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

    [Test]
    [Arguments("columns")]
    [Arguments("columnTier")]
    [Arguments("fieldSpan")]
    [Arguments("spanTier")]
    [Arguments("groupSpan")]
    [Arguments("sectionSpan")]
    public async Task Fields_WithGridValueOutOfRange_ThrowsArgumentOutOfRangeException(
        string setting
    )
    {
        // Arrange
        var services = new ServiceCollection();
        var sut = services.AddStellarAdmin().AddDashboard().AddResource<Product>();

        // Act
        Action act = () =>
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
                    }
                })
            );

        // Assert
        await Assert.That(act).Throws<ArgumentOutOfRangeException>();
    }

    public sealed class Product
    {
        public string Name { get; set; } = "";
        public string ReadOnly => Name;
    }
}
