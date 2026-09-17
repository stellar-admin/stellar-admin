# Icons

## Built-in icons

StellarAdmin ships the [Lucide](https://lucide.dev/) icon set. It is registered by `AddStellarAdmin()`, so every Lucide icon is available from `<sa-icon>` by name with no further setup:

```razor
<sa-icon name="plane"/>
```

- Names are the kebab-case Lucide names (`circle-arrow-left`, `layout-dashboard`, `map-pinned`), matched **case-insensitively**.
- When no icon matches, `<sa-icon>` renders a placeholder "not found" icon rather than nothing, so a typo is visible on the page rather than silent.
- Icons render as inline `<svg>` using `currentColor`, so they take the color and size of their surroundings. Size and color them with `class` (`size-5`, `text-muted-foreground`), not with attributes. `stroke-width` passes through to the `<svg>`.

Icons compose naturally inside other components:

```razor
<sa-button><sa-icon name="plus"/>New booking</sa-button>
```

## Adding your own icons

Custom icons are registered on the builder returned by `AddStellarAdmin()`, then used exactly like the built-in ones. An icon is an `IconDefinition`: the attributes for the `<svg>` element plus the ordered list of shapes (`path`, `circle`, `rect`, …) drawn inside it.

Draw on the same 24 x 24 grid with a 2 px `currentColor` stroke to stay visually consistent with Lucide. Any attribute you put on `<sa-icon>` (such as `class` or `stroke-width`) takes precedence over the definition's attributes.

### A single icon — `AddIcon`

```csharp
using System.Collections.Immutable;
using StellarAdmin.Icons;

builder.Services.AddStellarAdmin()
    .AddIcon("voyager-suitcase", new IconDefinition(
        new Dictionary<string, string>
        {
            ["xmlns"] = "http://www.w3.org/2000/svg",
            ["width"] = "24",
            ["height"] = "24",
            ["viewBox"] = "0 0 24 24",
            ["fill"] = "none",
            ["stroke"] = "currentColor",
            ["stroke-width"] = "2",
            ["stroke-linecap"] = "round",
            ["stroke-linejoin"] = "round",
        },
        [
            new SvgShape("rect", new Dictionary<string, string>
            {
                ["x"] = "3", ["y"] = "7", ["width"] = "18", ["height"] = "13", ["rx"] = "2",
            }.ToImmutableDictionary()),
            new SvgShape("path", new Dictionary<string, string>
            {
                ["d"] = "M8 7V5a2 2 0 0 1 2-2h4a2 2 0 0 1 2 2v2",
            }.ToImmutableDictionary()),
        ]))
    .AddTagHelpers();
```

```razor
<sa-icon name="voyager-suitcase"/>
```

**Duplicate names cause an exception when icon options are resolved**, including names already used by a built-in icon. To replace a built-in icon, register a pack.

### An icon pack — `AddIconPack<T>`

For more than a handful of icons, or to override built-in ones, implement `IIconPack` and register it. A pack returns a dictionary of names to definitions; a pack icon whose name is already registered **replaces** the existing one.

```csharp
public class VoyagerIconPack : IIconPack
{
    private static readonly Dictionary<string, string> SvgAttributes = new()
    {
        ["xmlns"] = "http://www.w3.org/2000/svg",
        ["width"] = "24",
        ["height"] = "24",
        ["viewBox"] = "0 0 24 24",
        ["fill"] = "none",
        ["stroke"] = "currentColor",
        ["stroke-width"] = "2",
        ["stroke-linecap"] = "round",
        ["stroke-linejoin"] = "round",
    };

    public IDictionary<string, IconDefinition> GetIcons()
    {
        return new Dictionary<string, IconDefinition>
        {
            ["voyager-compass"] = new IconDefinition(SvgAttributes,
            [
                Shape("circle", ("cx", "12"), ("cy", "12"), ("r", "9")),
                Shape("path", ("d", "m15.5 8.5-2 5-5 2 2-5z")),
            ]),
        };
    }

    private static SvgShape Shape(string name, params (string Name, string Value)[] attributes)
    {
        return new SvgShape(name, attributes.ToImmutableDictionary(a => a.Name, a => a.Value));
    }
}
```

```csharp
builder.Services.AddStellarAdmin()
    .AddIconPack<VoyagerIconPack>()
    .AddTagHelpers();
```

`TIconPack` must have a parameterless constructor. Packs are read when icon options are first resolved. Each service provider has its own icon options; configure icons before building the application.

Icon registration uses `IconOptions` in `StellarAdmin.Icons`. Configure it through `services.Configure<IconOptions>(...)` using `AddIcon`, `AddIconPack<TIconPack>`, `RemoveIcon`, and `ClearIcons`. Use `GetIconNames` to list registered names and `TryGetIcon` to look up a definition. Tag helpers read the configured options directly. Treat the options and icon definitions as read-only after startup.

### Prefixing an icon pack

Use the registration callback to keep a pack's icon names separate from other packs:

```csharp
builder.Services.AddStellarAdmin()
    .AddIconPack<VoyagerIconPack>(pack =>
    {
        pack.Prefix = "voyager:";
        pack.ImportSemanticMappings = false;
    });
```

`Prefix` is literal: an icon named `compass` becomes `voyager:compass`, used as `<sa-icon name="voyager:compass" />`. Only the prefixed name is registered. Include your preferred separator in the prefix; null or an empty string leaves names unchanged. Lookups and removal remain case-insensitive. Reusing the same full name still replaces the existing icon.

`ImportSemanticMappings` defaults to `true`. When enabled, the pack's semantic mapping targets receive the same prefix automatically, and supplied roles replace existing mappings. Set it to `false` to skip reading, validating, and importing the pack's mappings. Existing mappings remain unchanged, although replacing an unprefixed icon still affects roles using that name. You can assign a prefixed icon to a role explicitly with `MapSemanticIcon(role, "voyager:compass")`.

The callback receives `IconPackOptions` and is also available on `IconOptions.AddIconPack<TIconPack>(...)`. Calling `AddIconPack<TIconPack>()` retains unprefixed names and imports semantic mappings.

## Semantic component icons

Built-in components request icons by purpose, such as `PaginationEllipsis` or `DropdownIndicator`. Icon packs can supply these mappings by implementing `GetSemanticIconMappings()` alongside `GetIcons()`:

```csharp
public IReadOnlyDictionary<SemanticIconRole, string> GetSemanticIconMappings()
{
    return new Dictionary<SemanticIconRole, string>
    {
        [SemanticIconRole.PaginationEllipsis] = "my-dots",
        [SemanticIconRole.DropdownIndicator] = "my-caret-down",
    };
}
```

Each mapped name must exist in the icons supplied by that same pack, even if an icon with that name is already registered; otherwise registration throws `ArgumentException`. Later packs replace mappings for the roles they supply and preserve other mappings. Existing packs can omit this method, which defaults to an empty mapping.

Override a role using an already registered icon:

```csharp
builder.Services.Configure<IconOptions>(icons =>
{
    icons.MapSemanticIcon(SemanticIconRole.PaginationEllipsis, "ellipsis-vertical");
});
```

To replace Lucide completely, configure `IconOptions` with `ClearIcons()` followed by `AddIconPack<MyIconPack>()`. Clearing removes both icons and mappings. A replacement pack should map all the roles below; missing roles render the existing missing-icon placeholder. Partial mappings are allowed. `RemoveIcon(name)` also removes mappings targeting that name, case-insensitively. Replacing a named definition updates every role that refers to it.

`GetSemanticIconName(role)` returns the mapped name, or `null` when unmapped. `<sa-icon>` continues to accept only an explicit icon name; it has no semantic role attribute. Existing component child-content overrides remain supported.

| Role | Default icon |
| --- | --- |
| `DropdownIndicator` | `chevron-down` |
| `SubmenuIndicator` | `chevron-right` |
| `BreadcrumbSeparator` | `chevron-right` |
| `AccordionIndicator` | `chevron-down` |
| `BreadcrumbEllipsis` | `ellipsis` |
| `PaginationEllipsis` | `ellipsis` |
| `PaginationPrevious` | `chevron-left` |
| `PaginationNext` | `chevron-right` |
| `PaginationFirst` | `chevron-first` |
| `PaginationLast` | `chevron-last` |
| `CarouselPrevious` | `chevron-left` |
| `CarouselNext` | `chevron-right` |
| `Close` | `x` |
| `Loading` | `loader-circle` |
| `CheckboxSelected` | `check` |
| `RadioSelected` | `circle` |
| `MenuItemSelected` | `check` |
| `ChoiceSelected` | `check` |
| `SortAscending` | `arrow-up` |
| `SortDescending` | `arrow-down` |
| `SortUnsorted` | `chevrons-up-down` |
| `ToggleSidebar` | `panel-left` |
| `ScrollToEnd` | `arrow-down` |
| `OtpSeparator` | `minus` |

Supply glyphs compatible with component styling: the accordion indicator points down when closed and rotates 180 degrees when open, loading icons rotate continuously, carousel navigation adapts to orientation and text direction, and `ScrollToEnd` rotates for scrolling to the start. Dashboard view icons and application-selected icon names are outside this semantic mapping.

## API reference

| Member | Description |
|--------|-------------|
| `AddIcon(name, iconDefinition)` | Registers a single icon under a new name. Duplicate names throw when icon options are resolved. |
| `AddIconPack<TIconPack>()` | Instantiates `TIconPack` and registers every icon it returns. Same-named icons replace existing ones. |
| `IconOptions.MapSemanticIcon(role, name)` | Assigns a registered icon to a semantic role. |
| `IconOptions.GetSemanticIconName(role)` | Returns the mapped icon name, or `null`. |
| `IIconPack.GetSemanticIconMappings()` | Returns role-to-name mappings (`IReadOnlyDictionary<SemanticIconRole, string>`); optional. |
| `IIconPack.GetIcons()` | Returns the icons in the pack, keyed by name (`IDictionary<string, IconDefinition>`). |
| `IconDefinition.Attributes` | Attributes rendered on the `<svg>` element (`viewBox`, `fill`, `stroke`, …). Attributes on `<sa-icon>` take precedence. |
| `IconDefinition.Shapes` | The shapes rendered inside the `<svg>`, in order (`List<SvgShape>`). |
| `SvgShape.Name` | The SVG element name — `path`, `circle`, `rect`, `line`, `polyline`. |
| `SvgShape.Attributes` | Attributes on the shape element — `d`, `cx`, `cy`, `r` (`IImmutableDictionary<string, string>`). |

Types live in the `StellarAdmin.Icons` namespace.
