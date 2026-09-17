# Shadcn theme namespace

Status: completed — implemented, verified locally, and committed at the user’s request.
Last updated: 2026-09-13.
Affected repositories: workspace, stellar-admin, stellar-admin-pro, website, skills.

## Recommendation

Use `shadcn.<name>` as the canonical identifier for every upstream-derived theme, starting at the point where ThemeGenerator writes into the shared `Client/css/themes/` directory. Keep upstream names and generator-local override filenames unchanged. Reserve the `shadcn.` prefix for upstream-derived themes.

| Stage | Current Nova name | Proposed name |
| --- | --- | --- |
| Upstream input | `style-nova.css` | unchanged |
| Generator-local overrides | `util/ThemeGenerator/Themes/Nova.custom.css` | unchanged |
| Generated component CSS | `Client/css/themes/nova.css` | `Client/css/themes/shadcn.nova.css` |
| Temporary bundle entry | `.theme-build-*/nova.css` | `.theme-build-*/shadcn.nova.css` (derived automatically) |
| Public bundle | `wwwroot/stellar-admin.nova.css` | `wwwroot/stellar-admin.shadcn.nova.css` |
| Sample/query/storage identifier | `nova` | `shadcn.nova` |
| Display label | Nova | Nova in a Shadcn group, or Shadcn Nova in a flat list |

Apply this to luma, lyra, maia, mira, nova, rhea, sera and vega. The seven current custom themes retain their names and filenames.

## Findings

ThemeGenerator downloads eight explicitly listed upstream files, transforms their component rules, appends the corresponding generator-local overrides, and writes to `Client/css/themes/<name>.css`. Custom themes are authored directly in that same directory. Upstream themes are not discovered automatically; an upstream addition affects us only when we add it to the generator. However, the generator currently has no protection against overwriting a same-named custom file at that point.

`Client/scripts/build-theme-bundles.mjs` enumerates the shared directory and combines `base.css` with each theme source through a temporary entry. The basename becomes the public bundle identifier. `util/theme-coverage/check.mjs` uses the same basenames to check source ownership, per-component coverage, and the project's explicit `ClientOutput` declarations. DocsSamplesGenerator independently enumerates the same directory for its exported theme list and asset downloads.

Consequently, adding the prefix only to the final bundle is possible but incomplete: source collisions remain, and both build and export would need a source-to-public-name mapping. Separating generated sources into `themes/shadcn/` would also solve collisions while preserving their basenames, but requires changing discovery, coverage, and export to understand directory-based identifiers. Prefixing generated source filenames is the smaller change and preserves the existing basename convention.

No server-side theme registration API needs changing. Theme selection remains a stylesheet link. CSS classes, tokens, layering, palettes and override contents are unaffected.

## Implementation scope

1. **OSS generation and build:** change only ThemeGenerator's output filename to `shadcn.<upstream-name>.css`; preserve its upstream URLs, override lookup, and provenance comments. Rename the eight checked-in generated sources without regenerating their contents from the live upstream. Rename their coverage keys throughout `coverage.json` and their `ClientOutput` entries. Keep execution-time MSBuild output registration. The bundler already handles dots through basename derivation.
2. **Namespace enforcement:** extend coverage validation to require `shadcn.` for upstream ownership and forbid it for custom ownership. Add a focused generator guard against overwriting an existing hand-authored file. Future upstream additions must receive the prefix automatically, even if their unprefixed names match custom themes.
3. **Stale build output:** remove the eight obsolete unprefixed generated bundles from existing `wwwroot` output as part of the migration; verify they cannot enter the next package. Scope cleanup to the known obsolete outputs and account for a future custom theme legitimately owning an unprefixed name. Do not delete arbitrary CSS files or ship permanent aliases.
4. **Samples and Pro:** update both sample layouts' allowlists, the OSS appearance selector values, the Pro shell's hardcoded Nova link, and ComponentPlayground's Vega link. Preserve the chosen default theme in each application (OSS currently Ledger; Pro samples currently Nova). Separate display labels from identifiers so users do not see `Shadcn.nova`. Update relevant runnable HTML prototypes that link the renamed bundles.
5. **Docs exporter:** source enumeration will pick up the renamed IDs automatically. Update its Nova fallback and stylesheet-link parsing. The current regex permits one optional dotted fingerprint after a single-segment theme name, so `shadcn.nova` can be misread as theme `shadcn` with fingerprint `nova`. Match known theme identifiers, escaped and followed by an optional fingerprint, and verify both fingerprinted and plain URLs. Retain stable exported bundle names and correct duplicate-asset filtering.
6. **Website:** update `demoThemes`, the default, and labels. Unknown saved `demo-theme` values fall back to `shadcn.nova` in both the website and exported iframe script. No saved-preference migration or version tracking is needed. Regenerate demos and assets through DocsSamplesGenerator after preserving existing edits; do not hand-edit generated pages. Its cleanup already removes old exported assets.
7. **Verification tools:** allow canonical dotted identifiers in `util/visual-regression/vrt.mjs` (it currently accepts only lowercase letters). Update the upstream IDs in `util/segmented-control/check.mjs`. Check any theme-specific parsing and URL construction found during implementation.
8. **Documentation:** update OSS README, website installation/theming pages, consumer theming/setup guidance, workspace repo guidance, and development workflow examples that link an upstream bundle. Document the reserved namespace and migration. Leave upstream references and generator-local override paths unchanged. Historical plans can retain historical filenames.

## Compatibility

This changes the public stylesheet URLs. Recommend a coordinated breaking rename with a migration note: consumers replace `stellar-admin.nova.css` with `stellar-admin.shadcn.nova.css`, and similarly for the other seven themes. Avoid permanent old-name bundle aliases because they would occupy names intended to remain available to custom themes. Confirm release/version treatment when scheduling implementation; this proposal does not authorize publishing.

Use canonical IDs in sample query parameters (`?theme=shadcn.nova`) and tool arguments. Old sample links may fall back to the application's default; document this rather than adding indefinite ambiguous aliases. Unknown saved themes fall back to the application default.

## Validation and acceptance

- Run the coverage checker and CSS build; verify exactly 15 expected bundles, including eight `stellar-admin.shadcn.*.css` bundles and seven unchanged custom bundles, with no obsolete upstream bundles.
- Compare pre-change and post-change compiled CSS under equivalent build conditions. This is a naming migration; investigate any content differences instead of accepting upstream regeneration drift.
- Add focused checks for namespaced source ownership and the exporter parsing of plain and fingerprinted URLs. Verify the hypothetical custom `nova` and upstream `shadcn.nova` identifiers remain distinct.
- Build the OSS and affected Pro projects; inspect static web assets and a locally packed NuGet package for correct filenames and absence of legacy artifacts.
- Exercise OSS and Pro samples using `shadcn.nova` plus an unchanged custom theme; check stylesheet responses and theme switching. Verify VRT accepts `--theme shadcn.nova` and preserves the same visual result in light/dark modes.
- Regenerate website exports, verify all theme asset requests resolve, and exercise inline demos, standalone previews, selector labels, and migration of a previously saved upstream choice. Run website lint, type checking, and production build.
- Search active code/docs for obsolete public names, allowing only deliberate migration mappings and historical/source-input references. Inspect Git status and diffs separately in each affected repository.

## Completed implementation and verification

Implemented the coordinated rename across all five repositories. ThemeGenerator writes `shadcn.<name>.css`, retains original upstream inputs and override lookups, and refuses to overwrite hand-authored CSS. The coverage checker enforces the reserved namespace. The bundle build removes known obsolete unprefixed assets only when no current source owns the old name. Sample and website selectors, Pro shell links, prototypes, docs, and consumer guidance use canonical IDs. Website/iframe preferences fall back to `shadcn.nova` for unknown saved themes, without migration or version tracking. At the user’s request, OSS `appearance.js` uses its existing default fallback for unknown saved themes, without migration or version tracking. The OSS menu labels its Shadcn group explicitly.

Verification completed on 2026-09-13:

- CSS build passed. All 15 compiled bundles are byte-for-byte identical to a pre-change build in the same environment. All eight checked-in source renames and all eight exported website CSS renames preserve their original contents exactly.
- Coverage passed for 55 components × 15 themes. All nine coverage tests passed, including upstream prefix enforcement, reserved custom-name rejection, and coexistence of custom `ledger` with hypothetical upstream `shadcn.ledger`.
- ThemeGenerator, DocsSamplesGenerator/OSS samples/library, and Pro samples/library builds passed. A local NuGet pack passed and contains exactly 15 theme CSS assets: eight namespaced upstream bundles and seven custom bundles, with no legacy upstream filenames.
- Exporter regex checks passed for plain and fingerprinted Nova, Vega, and Ledger URLs and rejection of an unknown theme. Regenerated all 412 demos; every exported page has the theme marker and valid referenced CSS assets. Generated HTML includes existing nondeterminism in SVG attribute order, generated IDs, and antiforgery tokens; no generated output was restored or hand-edited.
- Browser checks passed for all 15 theme asset URLs on both OSS and Pro; Nova and Ledger in light and dark modes; sample selector behavior and saved Vega migration; all eight legacy choices in standalone exported demos; and live demo switching to Ledger.
- Website browser checks passed for saved Vega migration, readable Shadcn labels, selecting Nova, and inline iframe synchronization. Additional preference checks covered custom/default choices, the one-time guard, and unavailable storage.
- VRT accepted `--theme shadcn.nova` and captured the sample index at desktop/mobile widths in light mode, plus index and Button at desktop/mobile widths in dark mode. These are CLI smoke checks, not a full visual-regression matrix; unchanged CSS was verified by byte comparison.
- Website lint, type checking, and production build passed. Git whitespace checks passed in all five repositories. BOM and newline conventions were preserved. Remaining active references to unprefixed upstream public IDs are deliberate migration/cleanup inputs and upgrade examples.

Environment notes: used installed .NET SDK 10.0.400 from the workspace because the OSS-pinned 10.0.100 is not installed; did not change the SDK pin. Used the installed pnpm 10.34.5 executable after the default pnpm 11 wrapper failed opening its database. .NET validation required single-worker builds in this sandbox and used the cached CSharpier 1.3.0 DLL for formatting. Builds reported existing NuGet vulnerability-feed access and C# documentation/nullability warnings. The website prerender build required local sockets outside the sandbox. Temporary browser profiles, logs, screenshots, package, and focused verification harnesses are under `/tmp/shadcn-*`.

## Handoff

The user authorized implementation after reviewing the analysis. The user subsequently authorized committing all changes. OSS commit: `13d95a0`; Pro: `ee883c1`; website: `4e158c0f`; consumer skills: `1e84b72`. Workspace guidance and this record are included in the accompanying workspace commit. No pushes, publishes, or deployments were performed. Implementation and local verification are complete. The temporary OSS, Pro, and website servers started for verification have been stopped. Release/version selection remains a separate publishing decision; consumers need the documented stylesheet URL migration when this ships.

Follow-up: removed the OSS sample preference migration at the user’s request. Its existing allowlist resolution falls back to Ledger when the saved theme is unknown. JavaScript syntax validation passed; `appearance.js` now matches its pre-migration implementation.

Follow-up: at the user’s request, removed saved-preference migration/version tracking from `Generator.cs` and the matching website reader. Both use the existing default fallback. Regenerated demos from the updated generator. The earlier migration checks above record historical verification of the initial implementation and no longer describe the final behavior.

Final fallback verification: generator export completed for all 412 demos; website lint, type checking, and production build passed. Focused checks confirmed missing/legacy/unknown saved choices fall back to `shadcn.nova`, valid upstream/custom choices remain selected, and neither reader writes migration state. No version-tracking keys remain in active sample, generator, website, or exported demo code.
