namespace StellarAdmin.TagHelpers.Icons;

public interface IIconManager
{
    string[] GetIconNames();

    bool TryGetIcon(string name, out IconDefinition? iconDefinition);
}
