using StellarAdmin.Dashboard.Resources.Builders;
using StellarAdmin.Dashboard.Resources.Options;
using StellarAdmin.Dashboard.Tests.Support;

namespace StellarAdmin.Dashboard.Tests.Resources.Builders;

public partial class FormFieldBuilderTests
{
    [Test]
    public async Task Editor_WhenCommonAndTypedConfigurationInterleave_PreservesAllValues()
    {
        // Arrange
        var options = new TestResourceOptions();
        var resource = new ResourceBuilder<TestEntity>(options);
        FormFieldBuilder sut = null!;
        resource.Create(page =>
            page.Fields(fields =>
                fields.AddSection(
                    "Details",
                    section =>
                        section.Fields(children => sut = children.Add(entity => entity.Condition))
                )
            )
        );

        // Act
        sut.Editor(editor => editor.ClassNames.Root = "condition-root")
            .Editor<RadioEditorOptions>(editor =>
                editor.ClassNames.Option.Root = "condition-choice"
            )
            .Editor(editor => editor.ClassNames.Label = "condition-label")
            .Editor<RadioEditorOptions>(editor => editor.ClassNames.Control = "condition-options");

        // Assert
        var editor = (RadioEditorOptions)options.CreatePage.Fields.Last().Editor;
        await Assert.That(editor.ClassNames.Root).IsEqualTo("condition-root");
        await Assert.That(editor.ClassNames.Label).IsEqualTo("condition-label");
        await Assert.That(editor.ClassNames.Control).IsEqualTo("condition-options");
        await Assert.That(editor.ClassNames.Option.Root).IsEqualTo("condition-choice");
    }

    [Test]
    public async Task Editor_WhenFieldIsConfigured_DoesNotModifySiblingOptions()
    {
        // Arrange
        var options = new TestResourceOptions();
        var resource = new ResourceBuilder<TestEntity>(options);
        FormFieldBuilder sut = null!;
        resource.Create(page =>
            page.Fields(fields =>
                fields.AddSection(
                    "Details",
                    section =>
                        section.Fields(children => sut = children.Add(entity => entity.Condition))
                )
            )
        );

        // Act
        sut.Editor(editor => editor.ClassNames.Root = "condition-root");

        // Assert
        await Assert
            .That(options.CreatePage.Fields.Last().Editor.ClassNames.Root)
            .IsEqualTo("condition-root");
        await Assert.That(options.CreatePage.Fields.First().Editor.ClassNames.Root).IsNull();
    }

    [Test]
    public async Task Editor_WhenOptionsTypesConflict_ThrowsDescriptiveError()
    {
        // Arrange
        var options = new TestResourceOptions();
        var resource = new ResourceBuilder<TestEntity>(options);
        FormFieldBuilder sut = null!;
        resource.Create(page =>
            page.Fields(fields =>
                fields.AddSection(
                    "Details",
                    section =>
                        section.Fields(children => sut = children.Add(entity => entity.Condition))
                )
            )
        );
        sut.Editor<RadioEditorOptions>(_ => { });

        // Act
        Action act = () => sut.Editor<OtherEditorOptions>(_ => { });

        // Assert
        var exception = await Assert.That(act).Throws<InvalidOperationException>();
        await Assert.That(exception!.Message).Contains("Condition");
        await Assert.That(exception!.Message).Contains("OtherEditorOptions");
    }

    [Test]
    public async Task Template_WhenNestedFieldIsConfigured_PreservesTitleAndEditor()
    {
        // Arrange
        var options = new TestResourceOptions();
        var resource = new ResourceBuilder<TestEntity>(options);
        FormFieldBuilder sut = null!;
        resource.Create(page =>
            page.Fields(fields =>
                fields.AddSection(
                    "Details",
                    section =>
                        section.Fields(children => sut = children.Add(entity => entity.Condition))
                )
            )
        );

        // Act
        sut.Template("Currency")
            .Title("Retail price")
            .Editor(editor => editor.ClassNames.Root = "retail-price");

        // Assert
        var field = options.CreatePage.Fields.Last();
        await Assert.That(field.Template).IsEqualTo("Currency");
        await Assert.That(field.Title).IsEqualTo("Retail price");
        await Assert.That(field.Editor.ClassNames.Root).IsEqualTo("retail-price");
    }

    private sealed class OtherEditorOptions : EditorOptions { }
}
