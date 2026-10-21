using StellarAdmin.Diagnostics;
using StellarAdmin.Resources.Builders;
using StellarAdmin.Resources.Displays;

namespace StellarAdmin.Tests.Resources.Builders;

public class ResourceColumnBuilderTests
{
    [Test]
    public async Task UseDisplay_ConfiguresDisplay()
    {
        // Arrange
        var sut = new ResourceColumnBuilder<Product>((Product product) => product.Name);

        // Act
        sut.UseDisplay<TextDisplay>(text => text.LineClamp = 2);
        var options = sut.Build();

        // Assert
        await Assert.That((options.Display as TextDisplay)?.LineClamp).IsEqualTo(2);
        await Assert.That(options.UsesBuiltInRendering).IsFalse();
    }

    [Test]
    public async Task UseBuiltInRendering_AfterUseDisplay_ClearsDisplay()
    {
        // Arrange
        var sut = new ResourceColumnBuilder<Product>((Product product) => product.Name);
        sut.UseDisplay<TextDisplay>();

        // Act
        sut.UseBuiltInRendering();
        var options = sut.Build();

        // Assert
        await Assert.That(options.Display).IsNull();
        await Assert.That(options.UsesBuiltInRendering).IsTrue();
    }

    [Test]
    public async Task UseDisplay_AfterUseBuiltInRendering_UsesDisplay()
    {
        // Arrange
        var sut = new ResourceColumnBuilder<Product>((Product product) => product.Name);
        sut.UseBuiltInRendering();

        // Act
        sut.UseDisplay<TextDisplay>();
        var options = sut.Build();

        // Assert
        await Assert.That(options.Display).IsTypeOf<TextDisplay>();
        await Assert.That(options.UsesBuiltInRendering).IsFalse();
    }

    [Test]
    public async Task UseDisplay_WithHandlerDisplay_KeepsDefaultDisplay()
    {
        // Arrange
        var sut = new ResourceColumnBuilder<Product>((Product product) => product.Name);
        sut.UseDisplay<PreparedDisplay>();
        var outer = ResourceProblemSink.Open();

        // Act
        var options = sut.Build();

        // Assert
        var problems = ResourceProblemSink.Close(outer);
        await Assert.That(options.Display).IsNull();
        await Assert
            .That(problems.Single().Kind)
            .IsEqualTo(ConfigurationProblemKind.ColumnDisplayWithHandler);
    }

    [Test]
    public async Task EmptyText_SetsEmptyText()
    {
        // Arrange
        var sut = new ResourceColumnBuilder<Product>((Product product) => product.Name);

        // Act
        sut.EmptyText = "Unnamed";
        var options = sut.Build();

        // Assert
        await Assert.That(options.EmptyText).IsEqualTo("Unnamed");
    }

    [Test]
    public async Task EditsField_WithPropertyPath_UsesPathAsFieldName()
    {
        // Arrange
        var sut = new ResourceColumnBuilder<Product>((Product product) => product.Name);

        // Act
        sut.EditsField(product => product.Name.Length);
        var options = sut.Build();

        // Assert
        await Assert.That(options.EditsFieldName).IsEqualTo("Name.Length");
    }

    [Test]
    public async Task EditsField_WithComputedValue_IgnoresCall()
    {
        // Arrange
        var sut = new ResourceColumnBuilder<Product>((Product product) => product.Name);
        sut.EditsField(product => product.Name.ToUpperInvariant());
        var outer = ResourceProblemSink.Open();

        // Act
        var options = sut.Build();

        // Assert
        var problems = ResourceProblemSink.Close(outer);
        await Assert.That(options.EditsFieldName).IsNull();
        await Assert
            .That(problems.Single().Kind)
            .IsEqualTo(ConfigurationProblemKind.InvalidSelector);
    }

    public sealed class Product
    {
        public string Name { get; set; } = "";
    }

    public sealed class PreparedDisplay : FieldDisplay, IFieldDisplay<PreparedDisplayHandler>
    {
        public override string TemplateName => "Prepared";
    }

    public sealed class PreparedDisplayHandler(PreparedDisplay display)
        : FieldDisplayHandler<PreparedDisplay>(display)
    {
        public override Task<object?> PrepareAsync(
            FieldDisplayContext context,
            CancellationToken cancellationToken
        ) => Task.FromResult<object?>(null);
    }
}
