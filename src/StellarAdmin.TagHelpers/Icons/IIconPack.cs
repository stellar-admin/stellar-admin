namespace StellarAdmin.TagHelpers.Icons;

public interface IIconPack
{
    IDictionary<string, IconDefinition> GetIcons();
}
