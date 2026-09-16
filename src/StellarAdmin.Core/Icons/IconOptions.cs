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
        StringComparer.OrdinalIgnoreCase
    );
    private readonly Dictionary<SemanticIconRole, string> _semanticIcons = new();

    public IconOptions()
    {
        AddIconPack<LucideIconPack>();
    }

    /// <summary>
    ///     Adds an icon with a unique name.
    /// </summary>
    public void AddIcon(string name, IconDefinition iconDefinition)
    {
        ArgumentNullException.ThrowIfNull(iconDefinition);

        _icons.Add(name, iconDefinition);
    }

    /// <summary>
    ///     Adds an icon pack and its semantic mappings.
    /// </summary>
    public void AddIconPack<TIconPack>()
        where TIconPack : IIconPack, new()
    {
        AddIconPack<TIconPack>(_ => { });
    }

    /// <summary>
    ///     Adds an icon pack with the specified registration settings.
    /// </summary>
    public void AddIconPack<TIconPack>(Action<IconPackOptions> configure)
        where TIconPack : IIconPack, new()
    {
        ArgumentNullException.ThrowIfNull(configure);

        var registration = new IconPackOptions();
        configure(registration);

        var pack = new TIconPack();
        var icons = pack.GetIcons();

        // Register the icons
        foreach (var (name, definition) in icons)
        {
            _icons[registration.Prefix + name] = definition;
        }

        // Register the semantic mappings
        var semanticIconMappings = pack.GetSemanticIconMappings();
        if (registration.ImportSemanticMappings)
        {
            var incomingIconNames = icons.Keys.ToHashSet(StringComparer.OrdinalIgnoreCase);

            foreach (var (role, name) in semanticIconMappings)
            {
                // Validate that the semantic mapping targets an icon supplied by this pack.
                if (name is null || !incomingIconNames.Contains(name))
                {
                    throw new ArgumentException(
                        $"Icon '{name}' for role '{role}' is not supplied by this icon pack."
                    );
                }

                _semanticIcons[role] = registration.Prefix + name;
            }
        }
    }

    /// <summary>
    ///     Removes all registered icons and semantic icon mappings.
    /// </summary>
    public void ClearIcons()
    {
        _icons.Clear();
        _semanticIcons.Clear();
    }

    /// <summary>
    ///     Returns the registered icon names.
    /// </summary>
    public string[] GetIconNames()
    {
        return _icons.Keys.ToArray();
    }

    /// <summary>
    ///     Returns the icon name for a semantic role, or null if none is mapped.
    /// </summary>
    public string? GetSemanticIconName(SemanticIconRole role)
    {
        return _semanticIcons.GetValueOrDefault(role);
    }

    /// <summary>
    ///     Assigns a registered icon to a semantic role.
    /// </summary>
    public void MapSemanticIcon(SemanticIconRole role, string name)
    {
        ArgumentNullException.ThrowIfNull(name);
        if (!_icons.ContainsKey(name))
        {
            throw new ArgumentException($"Icon '{name}' is not registered.", nameof(name));
        }

        _semanticIcons[role] = name;
    }

    /// <summary>
    ///     Removes an icon and its semantic mappings, returning whether it was registered.
    /// </summary>
    public bool RemoveIcon(string name)
    {
        if (!_icons.Remove(name))
        {
            return false;
        }

        foreach (
            var role in _semanticIcons
                .Where(mapping => StringComparer.OrdinalIgnoreCase.Equals(mapping.Value, name))
                .Select(mapping => mapping.Key)
                .ToArray()
        )
        {
            _semanticIcons.Remove(role);
        }

        return true;
    }

    /// <summary>
    ///     Returns whether an icon exists and supplies its definition.
    /// </summary>
    public bool TryGetIcon(string name, [NotNullWhen(true)] out IconDefinition? iconDefinition)
    {
        return _icons.TryGetValue(name, out iconDefinition);
    }
}
