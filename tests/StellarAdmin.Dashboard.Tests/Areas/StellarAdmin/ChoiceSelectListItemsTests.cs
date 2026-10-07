using StellarAdmin.Dashboard.Areas.StellarAdmin;
using StellarAdmin.Dashboard.Resources.Editors;
using TUnit.Assertions.Enums;

namespace StellarAdmin.Dashboard.Tests.Areas.StellarAdmin;

public class ChoiceSelectListItemsTests
{
    [Test]
    public async Task Create_GroupedChoices_GathersEachGroupAtItsFirstChoice()
    {
        // Arrange
        ChoiceItem[] choices =
        [
            new("lisbon", "Lisbon") { Group = new("Europe") },
            new("any", "Anywhere"),
            new("tokyo", "Tokyo") { Group = new("Asia") },
            new("rome", "Rome") { Group = new("Europe") },
        ];

        // Act
        var items = ChoiceSelectListItems.Create(choices);

        // Assert
        await Assert
            .That(items.Select(item => item.Value))
            .IsEquivalentTo(["lisbon", "rome", "any", "tokyo"], CollectionOrdering.Matching);
        await Assert.That(items[0].Group).IsSameReferenceAs(items[1].Group);
        await Assert.That(items[2].Group).IsNull();
        await Assert.That(items[3].Group!.Name).IsEqualTo("Asia");
    }

    [Test]
    public async Task Create_DisabledChoicesAndGroups_KeepsDisabledState()
    {
        // Arrange
        ChoiceItem[] choices =
        [
            new("lisbon", "Lisbon") { Disabled = true },
            new("tokyo", "Tokyo") { Group = new("Asia") { Disabled = true } },
        ];

        // Act
        var items = ChoiceSelectListItems.Create(choices);

        // Assert
        await Assert.That(items[0].Disabled).IsTrue();
        await Assert.That(items[1].Group!.Disabled).IsTrue();
        await Assert.That(items.Any(item => item.Selected)).IsFalse();
    }
}
