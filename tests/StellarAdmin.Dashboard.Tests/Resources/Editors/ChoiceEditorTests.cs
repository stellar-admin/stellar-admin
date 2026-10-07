using Microsoft.AspNetCore.Mvc.Rendering;
using StellarAdmin.Dashboard.Resources.Editors;
using TUnit.Assertions.Enums;

namespace StellarAdmin.Dashboard.Tests.Resources.Editors;

public class ChoiceEditorTests
{
    [Test]
    public async Task UseItems_SelectListItems_MapsToChoiceItems()
    {
        // Arrange
        var europe = new SelectListGroup { Name = "Europe", Disabled = true };
        var sut = new SelectEditor();

        // Act
        sut.UseItems([
            new SelectListItem
            {
                Text = "Lisbon",
                Group = europe,
                Disabled = true,
            },
            new SelectListItem("Rome", "rome", true) { Group = europe },
        ]);
        var items = await sut.ItemsLoader!(null!, CancellationToken.None);

        // Assert
        await Assert
            .That(items)
            .IsEquivalentTo(
                [
                    new ChoiceItem("Lisbon", "Lisbon")
                    {
                        Disabled = true,
                        Group = new("Europe") { Disabled = true },
                    },
                    new ChoiceItem("rome", "Rome") { Group = new("Europe") { Disabled = true } },
                ],
                CollectionOrdering.Matching
            );
    }

    [Test]
    public async Task UseItems_ChoiceItems_TakesSnapshot()
    {
        // Arrange
        var choices = new List<ChoiceItem> { new("lisbon", "Lisbon") };
        var sut = new RadioGroupEditor();

        // Act
        sut.UseItems(choices);
        choices.Add(new("rome", "Rome"));
        var items = await sut.ItemsLoader!(null!, CancellationToken.None);

        // Assert
        await Assert.That(items).IsEquivalentTo([new ChoiceItem("lisbon", "Lisbon")]);
    }
}
