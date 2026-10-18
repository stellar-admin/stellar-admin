using StellarAdmin.Resources.Editors;
using StellarAdmin.UI;

namespace StellarAdmin.Tests.Resources.Editors;

public class IconEditorTests
{
    [Test]
    [Arguments(IconEditorLayout.Auto, null, IconPickerLayout.Picker)]
    [Arguments(IconEditorLayout.Auto, 1, IconPickerLayout.Inline)]
    [Arguments(IconEditorLayout.Auto, 24, IconPickerLayout.Inline)]
    [Arguments(IconEditorLayout.Auto, 25, IconPickerLayout.Picker)]
    [Arguments(IconEditorLayout.Picker, 3, IconPickerLayout.Picker)]
    [Arguments(IconEditorLayout.Inline, 30, IconPickerLayout.Inline)]
    public async Task ResolveLayout_ReturnsPickerLayoutForLayoutAndIconCount(
        IconEditorLayout layout,
        int? count,
        IconPickerLayout expected
    )
    {
        // Arrange
        var editor = new IconEditor
        {
            Layout = layout,
            Icons = count is { } icons
                ? Enumerable.Range(0, icons).Select(index => $"icon-{index}").ToArray()
                : null,
        };

        // Act
        var resolved = editor.ResolveLayout();

        // Assert
        await Assert.That(resolved).IsEqualTo(expected);
    }
}
