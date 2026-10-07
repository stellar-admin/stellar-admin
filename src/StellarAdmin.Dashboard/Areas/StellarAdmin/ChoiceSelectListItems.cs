using Microsoft.AspNetCore.Mvc.Rendering;
using StellarAdmin.Dashboard.Resources.Editors;

namespace StellarAdmin.Dashboard.Areas.StellarAdmin;

// Maps choices to select options. Each group's choices move up to the position of its first choice, so a group renders
// as one optgroup, and every option of a group shares one SelectListGroup, which the select compares by reference.
internal static class ChoiceSelectListItems
{
    public static List<SelectListItem> Create(IReadOnlyList<ChoiceItem> choices)
    {
        var groups = new Dictionary<ChoiceGroup, (int Position, SelectListGroup Group)>();
        var options = new List<(int Position, SelectListItem Item)>(choices.Count);
        for (var index = 0; index < choices.Count; index++)
        {
            var choice = choices[index];
            var position = index;
            SelectListGroup? group = null;
            if (choice.Group is { } choiceGroup)
            {
                if (!groups.TryGetValue(choiceGroup, out var entry))
                {
                    entry = (
                        index,
                        new SelectListGroup
                        {
                            Name = choiceGroup.Text,
                            Disabled = choiceGroup.Disabled,
                        }
                    );
                    groups.Add(choiceGroup, entry);
                }

                (position, group) = entry;
            }

            options.Add(
                (
                    position,
                    new SelectListItem(choice.Text, choice.Value, false, choice.Disabled)
                    {
                        Group = group,
                    }
                )
            );
        }

        return options.OrderBy(option => option.Position).Select(option => option.Item).ToList();
    }
}
