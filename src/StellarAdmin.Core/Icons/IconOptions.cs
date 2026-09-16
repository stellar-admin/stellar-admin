using System.Diagnostics.CodeAnalysis;

namespace StellarAdmin.Icons;

/// <summary>
///     Configures the icons available to StellarAdmin.
/// </summary>
/// <remarks>
///     Includes Lucide icons by default.
/// </remarks>
public class IconOptions
{
    private readonly Dictionary<string, IconDefinition> _icons = new(
        LucideIcons.IconDefinitions,
        StringComparer.OrdinalIgnoreCase
    );

    /// <summary>
    ///     Adds an icon with a unique name.
    /// </summary>
    public void AddIcon(string name, IconDefinition iconDefinition)
    {
        ArgumentNullException.ThrowIfNull(iconDefinition);

        _icons.Add(name, iconDefinition);
    }

    /// <summary>
    ///     Adds an icon pack, replacing icons with matching names.
    /// </summary>
    public void AddIconPack<TIconPack>()
        where TIconPack : IIconPack, new()
    {
        foreach (var (name, definition) in new TIconPack().GetIcons())
        {
            _icons[name] = definition;
        }
    }

    /// <summary>
    ///     Removes all registered icons.
    /// </summary>
    public void ClearIcons()
    {
        _icons.Clear();
    }

    /// <summary>
    ///     Returns the registered icon names.
    /// </summary>
    public string[] GetIconNames()
    {
        return _icons.Keys.ToArray();
    }

    /// <summary>
    ///     Removes an icon and returns whether it was registered.
    /// </summary>
    public bool RemoveIcon(string name)
    {
        return _icons.Remove(name);
    }

    /// <summary>
    ///     Returns whether an icon exists and supplies its definition.
    /// </summary>
    public bool TryGetIcon(string name, [NotNullWhen(true)] out IconDefinition? iconDefinition)
    {
        return _icons.TryGetValue(name, out iconDefinition);
    }
}
