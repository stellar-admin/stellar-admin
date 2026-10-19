# Icon keywords and icon pack refresh

Status: approved (2026-10-19); phase 1 implemented, awaiting review. Follows [icon-picker-editor](icon-picker-editor.md), whose picker already searches `keywords` from `icons-url` but gets none from StellarAdmin's own packs.

## Goal

Searching the icon picker for "pool" or "internet" should find `waves-ladder` and `wifi`. The packs' upstream sources publish keywords next to the icon data we already import, so ship them with the icons, and refresh the packs to the current releases so the two match.

## Findings (2026-10-19)

- `src/StellarAdmin.UI/Icons/Definitions/readme.txt` lists unversioned unpkg URLs. The files were fetched by hand. Lucide is from lucide-static 1.46.0, and the Tabler version isn't recorded.
- Lucide: `lucide-static/tags.json` maps every icon name to its tags. At 1.59.0 every icon has tags, about 8 each, and some tags are emoji. Without emoji the tags are 185 KB raw, 46 KB gzip.
- Tabler: `@tabler/icons/icons.json` has `tags` (and a `category`) per icon, for outline and filled together. The tags alone are 589 KB raw, 110 KB gzip.
- Refresh delta to the current releases: Lucide 1.46.0 → 1.59.0 adds 57 icons and removes `music`, `music-2`, `music-3` and `music-4`, which nothing in this repository uses. Tabler outline 3.49.0 adds 54 icons and removes none, and filled is unchanged.

## Proposal

1. **Pinned refresh.** A small script, `util/icon-packs/update.mjs`, downloads pinned versions (Lucide 1.59.0, Tabler 3.49.0) of the icon nodes and the keyword files. It writes `Definitions/Lucide.json`, `TablerOutline.json` and `TablerFilled.json` as now, plus `Lucide.keywords.json` and `Tabler.keywords.json` holding just `{ "name": ["tag", …] }`. The readme records the versions and the command.
2. **Generator.** `IconsFromJsonDefinitionGenerator` reads the matching keywords file and passes each icon's keywords to its definition. An icon without keywords gets an empty list.
3. **Public API (StellarAdmin.UI):** `IconDefinition` gains `Keywords` and a constructor overload. The existing constructor stays, with no keywords.
4. **Picker.** A child `<sa-icon>` inside `sa-icon-picker` carries its registered keywords (`data-icon-keywords`), so search on a curated list matches keywords too. The script already matches keywords from `icons-url`.
5. **Admin endpoint.** `IconCatalog` writes `keywords` for icons that have them, as the documented payload allows. Measured estimate for Lucide: 90 KB → about 135 KB gzip. The version hash changes with the keywords, so cached copies refresh.
6. **Docs.** The DocsSamples icons endpoint and the exported `icons.json` include keywords. Update the icon picker page (search matches the packs' keywords), the icons page and the UI skill's icons reference (`Keywords`, the pack versions), and the admin skill's line that search matches names only.
7. **Tests.** Generator output for an icon with and without keywords, `IconDefinition` copying the list, the picker's child keywords, the endpoint payload, and a check that every semantic mapping and every literal icon name in the samples still resolves after the refresh.

## API

```csharp
public sealed class IconDefinition
{
    public IconDefinition(IReadOnlyDictionary<string, string> attributes, IReadOnlyList<SvgShape> shapes); // unchanged
    public IconDefinition(IReadOnlyDictionary<string, string> attributes, IReadOnlyList<SvgShape> shapes, IReadOnlyList<string> keywords);

    public IReadOnlyList<string> Keywords { get; } // words that search matches besides the name; empty by default
}
```

## Decisions (Jerrie, 2026-10-19)

1. No aliases: the removed Lucide `music` icons are dropped.
2. Emoji are dropped from Lucide's tags.
3. Tabler's categories are ignored.

## Phases

1. Refresh script, pinned packs and keyword files, the generator, `IconDefinition.Keywords`, the sample and semantic-mapping audit, and tests.
2. Picker child keywords, the admin endpoint payload, docs, skills and website export.

## Phase 1 implementation (2026-10-19)

- `util/icon-packs/update.mjs` pins `lucide-static@1.59.0` and `@tabler/icons@3.49.0`. It writes the three node files unchanged from upstream (with the BOM the repository's files have), and `Definitions/Keywords/{pack}.json`, one line per icon. Lucide's tags lose emoji but keep currency symbols such as `€`. Tabler's few numeric tags (`911`, `420`, `0`–`9`) are written as strings. The readme lists the pinned sources. A second run leaves no diff.
- Refresh result: Lucide 1,838 → 1,891 icons (the `music` icons dropped, no aliases), Tabler outline 5,130 → 5,184, Tabler filled unchanged at 1,054. Every icon in all three packs has keywords.
- `IconsFromJsonDefinitionGenerator` pairs each pack file with `Keywords/{pack}.json` (the `Keywords` folder isn't treated as a pack) and passes the keywords to the new `IconDefinition` constructor. The csproj adds `Icons/Definitions/Keywords/*.json` as additional files.
- `IconDefinition` gains the `keywords` constructor overload and `Keywords`. The existing constructor gives an empty list.
- Audit: nothing in this repository or the website uses the removed `music` icons, and all 36 semantic mappings of each pack resolve.
- Size: `StellarAdmin.UI.dll` (Debug) grows from 4.7 MB to 5.7 MB with the keyword strings.
- Packaging fix (separate commit): the Razor SDK treated the definition JSON files as content, so the package carried them twice (`content/` and `contentFiles/`, about 9 MB uncompressed) and consumers got them as content. `StellarAdmin.UI.csproj` now removes `Icons/Definitions/**` from `Content`; they stay generator inputs. The package has no JSON definitions and drops from 2.6 MB to 1.3 MB, the build output has no `Icons/` folder, and the UI unit suite still passes (641).
- Verified: 8 new unit tests (`IconDefinition` keywords; upstream keywords in each pack, including a numeric Tabler tag). Full suites pass: StellarAdmin.UI.Tests 641, StellarAdmin.Tests 253, IntegrationTests 576, UI.IntegrationTests 19, EF IntegrationTests 97. The solution builds without errors, and its warnings are pre-existing.
