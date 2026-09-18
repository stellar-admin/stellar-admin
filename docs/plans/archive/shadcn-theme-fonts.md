# Optional shadcn-theme fonts

## Code audit — 2026-09-18

Current status: **completed**. `util/ThemeGenerator/Themes/*.custom.css`, generated shadcn theme sources and the shared theme font loader contain the optional font families, including Sera display typography.

This assessment uses the current checkout; earlier status, paths, permissions and verification notes below describe historical sessions. Runtime/browser and hosted release checks were not rerun for this documentation audit.

## Historical record

Status: completed locally. Last updated: 2026-09-14. Affected repos: workspace, OSS, website, consumer skills; Pro samples consume the shared loader.

## Decision

Apply the same optional-font policy as the seven custom themes. Luma, Mira, Rhea and Vega prefer Inter; Nova prefers Geist; Maia prefers Figtree; Lyra prefers JetBrains Mono for body and headings; Sera prefers Noto Sans for body and Playfair Display for component titles. These are accepted StellarAdmin defaults, not a claim that upstream requires these pairings. Rhea matches Luma.

The shadcn font definitions live in the generator’s maintained `Themes/*.custom.css` inputs and are emitted into each generated theme. Sans stacks fall back to system-ui, -apple-system, BlinkMacSystemFont, Segoe UI and sans-serif, except Maia, which prefers Avenir Next, Avenir and Segoe UI before system-ui/sans-serif. Lyra uses the same native mono fallback order as the custom themes. Sera headings fall back to Georgia, Times New Roman, Times and serif via `--sa-sera-font-display`. The existing `sa-font-heading` hook applies that family to app-authored headings; component titles receive it directly. Sizes, weights, line heights and tracking remain unchanged.

## Implementation

The shared sample loader requests only the selected theme’s families at weights 400–700 with `display=swap`; Pro and generated website demos consume that same loader. Both showcase pages mark their own headings with `sa-font-heading`. All themes continue to work without font downloads. Public theming docs follow the custom-theme table and per-theme tabbed installation examples; the consumer theming guide mirrors the decision.

## Verification

- ThemeGenerator completed successfully and produced only the intended font additions. `npm run build` passed, including theme coverage and all fifteen CSS bundles.
- DocsSamplesGenerator built the samples and regenerated 414 demos. Eight shadcn CSS assets and the shared loader changed; the showcase snippets/exports gained heading hooks. Other HTML changes are the loader fingerprint, generated IDs/antiforgery tokens and SVG attribute order. No pre-existing edits were present or discarded, and all generated local asset references resolve.
- Pro samples built and consumed the copied shared loader. .NET commands used installed workspace SDK 10.0.400 rather than the child repo’s 10.0.100 pin. Existing nullable/XML documentation and vulnerability-feed warnings remain.
- Website lint, type checking and production build passed, including a final run after export regeneration. Whitespace and touched-guide link checks passed.
- Browser checks passed for 128 showcase cases: eight themes × two pages × light/dark × 1440/390px × available/unavailable web fonts. Verified one selected-family stylesheet request, `display=swap`, actual rendered heading/control fonts, native fallback, and no document-level horizontal overflow. The loaded cases intercepted the actual Google Fonts requests with downloaded Latin font data; blocked cases simulated network failure. Inspected Sera and Lyra loaded/native desktop screenshots and a settled Sera dark view.
- All eight Pro themes and exported demos requested/rendered the selected fonts. Sera exported card titles used Playfair Display. Verified sample saved preferences, query overrides, live picker changes and exported storage-event switching. App CSS overrides of body/heading fonts worked; JavaScript-disabled samples requested no fonts and rendered native text.

Browser verification used Linux Chromium. Native sans/mono/serif resolved to Liberation Sans, Liberation Mono and Liberation Serif on this machine; Windows/macOS font selection was not reverified. Font metrics and wrapping intentionally vary across platforms. Temporary verification scripts, downloads and screenshots were removed after review; only agent-started servers on 5206, 5207 and 5211 were stopped. Port 5205 was untouched.

## Remaining work

No implementation work remains. Jerrie authorized committing all changes in the workspace, OSS, website and consumer skills repos; the Pro repo remains clean because it consumes the shared loader without a source change. No push or deployment was performed.
