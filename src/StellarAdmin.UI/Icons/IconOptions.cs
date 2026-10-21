using System.Collections.Frozen;
using System.Diagnostics.CodeAnalysis;

namespace StellarAdmin.UI.Icons;

/// <summary>
///     Configures the icons that StellarAdmin components can render.
/// </summary>
/// <remarks>
///     The Lucide icon pack is included by default. The icons can't be changed after the options
///     are configured.
/// </remarks>
public sealed class IconOptions
{
    private readonly Dictionary<string, IconDefinition> _icons = new(
        StringComparer.OrdinalIgnoreCase
    );
    private readonly List<string> _problems = [];
    private readonly Dictionary<SemanticIconRole, string> _semanticIcons = new();
    private FrozenDictionary<string, IconDefinition>? _frozenIcons;
    private FrozenDictionary<SemanticIconRole, string>? _frozenSemanticIcons;

    /// <summary>
    ///     Gets the problems found in the icon registrations.
    /// </summary>
    /// <value>
    ///     A description of each problem, such as a semantic icon mapping that names an icon that
    ///     isn't registered. Each problem is logged when the app starts, and the admin panel shows
    ///     it in Development.
    /// </value>
    public IReadOnlyList<string> Problems => _problems;

    /// <summary>
    ///     Initializes a new instance of the <see cref="IconOptions" /> class.
    /// </summary>
    /// <remarks>
    ///     The new instance includes the Lucide icon pack and its semantic icon mappings.
    /// </remarks>
    public IconOptions()
    {
        AddIconPack<LucideIconPack>();
    }

    /// <summary>
    ///     Adds an icon.
    /// </summary>
    /// <remarks>
    ///     The icon replaces any icon already registered with the same name. Names aren't case
    ///     sensitive.
    /// </remarks>
    /// <param name="name">The name that components use to render the icon.</param>
    /// <param name="iconDefinition">The icon to add.</param>
    /// <exception cref="ArgumentNullException">
    ///     <paramref name="name" /> or <paramref name="iconDefinition" /> is
    ///     <see langword="null" />.
    /// </exception>
    /// <exception cref="InvalidOperationException">
    ///     The options have already been configured.
    /// </exception>
    public void AddIcon(string name, IconDefinition iconDefinition)
    {
        ArgumentNullException.ThrowIfNull(name);
        ArgumentNullException.ThrowIfNull(iconDefinition);
        ThrowIfFrozen();

        _icons[name] = iconDefinition;
    }

    /// <summary>
    ///     Adds the icons of an icon pack and its semantic icon mappings.
    /// </summary>
    /// <remarks>
    ///     The pack's icons replace any icons already registered with the same names. A semantic
    ///     role that the pack maps to an icon it doesn't supply is a configuration problem: it is
    ///     logged, and the mapping is skipped.
    /// </remarks>
    /// <typeparam name="TIconPack">The type of the icon pack to add.</typeparam>
    /// <exception cref="InvalidOperationException">
    ///     The options have already been configured.
    /// </exception>
    public void AddIconPack<TIconPack>()
        where TIconPack : IIconPack, new()
    {
        AddIconPack<TIconPack>(_ => { });
    }

    /// <summary>
    ///     Adds the icons of an icon pack, configured by a delegate.
    /// </summary>
    /// <remarks>
    ///     The pack's icons replace any icons already registered with the same names. A semantic
    ///     role that the pack maps to an icon it doesn't supply is a configuration problem: it is
    ///     logged, and the mapping is skipped.
    /// </remarks>
    /// <typeparam name="TIconPack">The type of the icon pack to add.</typeparam>
    /// <param name="configure">A delegate that configures how the icon pack is added.</param>
    /// <exception cref="ArgumentNullException">
    ///     <paramref name="configure" /> is <see langword="null" />.
    /// </exception>
    /// <exception cref="InvalidOperationException">
    ///     The options have already been configured.
    /// </exception>
    public void AddIconPack<TIconPack>(Action<IconPackOptions> configure)
        where TIconPack : IIconPack, new()
    {
        ArgumentNullException.ThrowIfNull(configure);
        ThrowIfFrozen();

        var registration = new IconPackOptions();
        configure(registration);

        var pack = new TIconPack();
        var icons = pack.GetIcons();

        var semanticIconMappings = registration.ImportSemanticMappings
            ? pack.GetSemanticIconMappings()
            : new Dictionary<SemanticIconRole, string>();
        var incomingIconNames = icons.Keys.ToHashSet(StringComparer.OrdinalIgnoreCase);

        foreach (var (name, definition) in icons)
        {
            _icons[registration.Prefix + name] = definition;
        }

        foreach (var (role, name) in semanticIconMappings)
        {
            if (name is null || !incomingIconNames.Contains(name))
            {
                _problems.Add(
                    $"The `{role}` role keeps its previous icon because `{typeof(TIconPack).Name}` maps it to `{name}`, which the pack doesn't supply. Map the role to an icon that `{typeof(TIconPack).Name}` supplies."
                );
                continue;
            }

            _semanticIcons[role] = registration.Prefix + name;
        }
    }

    /// <summary>
    ///     Removes all registered icons and semantic icon mappings.
    /// </summary>
    /// <exception cref="InvalidOperationException">
    ///     The options have already been configured.
    /// </exception>
    public void ClearIcons()
    {
        ThrowIfFrozen();
        _icons.Clear();
        _semanticIcons.Clear();
    }

    /// <summary>
    ///     Returns the names of the registered icons.
    /// </summary>
    /// <returns>The names of the registered icons.</returns>
    public string[] GetIconNames()
    {
        return _frozenIcons?.Keys.ToArray() ?? _icons.Keys.ToArray();
    }

    /// <summary>
    ///     Returns the name of the icon mapped to a semantic role.
    /// </summary>
    /// <param name="role">One of the enumeration values that specifies the semantic role.</param>
    /// <returns>
    ///     The name of the mapped icon, or <see langword="null" /> if no icon is mapped to
    ///     <paramref name="role" />.
    /// </returns>
    public string? GetSemanticIconName(SemanticIconRole role)
    {
        return _frozenSemanticIcons?.GetValueOrDefault(role)
            ?? _semanticIcons.GetValueOrDefault(role);
    }

    /// <summary>
    ///     Maps a registered icon to a semantic role.
    /// </summary>
    /// <remarks>
    ///     Components render the icon wherever they need the icon for <paramref name="role" />. An
    ///     icon name that isn't registered is a configuration problem: it is logged, and the
    ///     mapping is skipped.
    /// </remarks>
    /// <param name="role">One of the enumeration values that specifies the semantic role.</param>
    /// <param name="name">The name of the registered icon.</param>
    /// <exception cref="ArgumentNullException">
    ///     <paramref name="name" /> is <see langword="null" />.
    /// </exception>
    /// <exception cref="InvalidOperationException">
    ///     The options have already been configured.
    /// </exception>
    public void MapSemanticIcon(SemanticIconRole role, string name)
    {
        ArgumentNullException.ThrowIfNull(name);
        ThrowIfFrozen();
        if (!_icons.ContainsKey(name))
        {
            _problems.Add(
                $"The `{role}` role keeps its previous icon because `MapSemanticIcon` maps it to `{name}`, which isn't registered. Register `{name}`, or map the role to an icon that's registered."
            );

            return;
        }

        _semanticIcons[role] = name;
    }

    /// <summary>
    ///     Removes an icon and its semantic icon mappings.
    /// </summary>
    /// <param name="name">The name of the icon to remove.</param>
    /// <returns>
    ///     <see langword="true" /> if the icon was registered and is removed; otherwise,
    ///     <see langword="false" />.
    /// </returns>
    /// <exception cref="InvalidOperationException">
    ///     The options have already been configured.
    /// </exception>
    public bool RemoveIcon(string name)
    {
        ThrowIfFrozen();
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
    ///     Gets the icon registered with a name.
    /// </summary>
    /// <param name="name">The name of the icon to get.</param>
    /// <param name="iconDefinition">
    ///     When this method returns, contains the icon, or <see langword="null" /> if no icon is
    ///     registered with <paramref name="name" />. This parameter is treated as uninitialized.
    /// </param>
    /// <returns>
    ///     <see langword="true" /> if an icon is registered with <paramref name="name" />;
    ///     otherwise, <see langword="false" />.
    /// </returns>
    public bool TryGetIcon(string name, [NotNullWhen(true)] out IconDefinition? iconDefinition)
    {
        return _frozenIcons?.TryGetValue(name, out iconDefinition)
            ?? _icons.TryGetValue(name, out iconDefinition);
    }

    // Called once the options are configured, so renders share a registry nothing can change
    internal void Freeze()
    {
        _frozenIcons ??= _icons.ToFrozenDictionary(StringComparer.OrdinalIgnoreCase);
        _frozenSemanticIcons ??= _semanticIcons.ToFrozenDictionary();
    }

    private void ThrowIfFrozen()
    {
        if (_frozenIcons != null)
        {
            throw new InvalidOperationException(
                "Icons can't be changed after the icon options are configured. Register icons on the StellarAdminUIBuilder instead."
            );
        }
    }
}
