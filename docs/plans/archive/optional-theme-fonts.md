# Optional custom-theme fonts

Status: completed locally. Last updated: 2026-09-14. Affected repos: workspace, OSS, Pro, website, consumer skills.

## Decision and scope

Jerrie reviewed the original and proposed native typography on both DocsSamples showcase pages for all seven custom themes and accepted the native results, while preferring the character of the original web fonts. Keep each original family first, followed by the reviewed native stack. Consumers can omit font-provider links for zero font downloads, or load the preferred fonts with `display=swap`. A late swap can change text wrapping; `display=optional` is an application choice when avoiding that swap matters more than always using the web font.

The maintained decisions and exact fallback order are recorded in [Aurora](../../design/themes/aurora.md), [Concourse](../../design/themes/concourse.md), [Ice](../../design/themes/ice.md), [Ledger](../../design/themes/ledger.md), [Meridian](../../design/themes/meridian.md), [Observatory](../../design/themes/observatory.md) and [Parallax](../../design/themes/parallax.md). Sizes, weights, tracking and line heights are unchanged. Native selection depends on the operating system; the accepted comparison covered Linux Chromium, including Ledger without Avenir.

## Implementation

OSS theme sources retain their original fonts and add the approved sans and mono fallbacks. No theme stylesheet fetches fonts or needs JavaScript for fallback. The sample-only `stellar-admin/docs/DocsSamples/wwwroot/js/theme-fonts.js` loader requests one Google Fonts stylesheet for the selected theme, retaining `display=swap`. DocsSamples calls it after resolving query parameters and saved preferences, and when the theme picker changes.

Pro copies the same loader into its sample `wwwroot` before static-asset discovery; the copy is ignored by Git. A linked external Content item built successfully but produced an empty development-server response, so the physical copy is intentional. Its layout selects fonts from the validated theme query parameter. The website generator exports the loader and invokes it when initializing or switching the website's demo theme. The unrelated font-override demonstration retains its explicit custom-font request.

Regenerated all 414 website demos and their assets. All seven custom theme assets changed; generated HTML includes the new loader and removes unconditional font links. Regeneration also consolidates the prior sample CSS assets into the current fingerprinted output, which includes the showcase's `mb-6` utility, and retains generated SVG attribute-order changes. No pre-existing edits were present or discarded. Updated public theming documentation and the handwritten consumer theming skill; generated component references were not affected.

## Verification

- `npm run build` in the OSS library Client directory passed, including theme coverage and all fifteen CSS bundles.
- The docs generator built DocsSamples and generated 414 demos successfully. Pro samples built successfully, including the corrected shared-asset copy. Commands used installed workspace SDK 10.0.400 rather than the child repo's 10.0.100 pin; existing nullable/XML warnings and cached NU1900 vulnerability-feed warnings were reported.
- Browser checks passed for 112 showcase cases: seven themes × two pages × light/dark × 1440/390px × available/unavailable web fonts. Successful loads used locally embedded original font data supplied to the requested Google Fonts stylesheet through browser interception; unavailable loads simulated a failed request. Verified selected family requests, `display=swap`, actual rendered fonts and no document-level horizontal overflow. Inspected desktop native and loaded captures.
- Verified that a saved DocsSamples preference requests only its selected fonts, that query parameters take precedence, and that live picker changes retain a single font stylesheet. All seven Pro theme requests passed after fixing the static-asset copy.
- Verified all seven exported demo selections and live storage-event switching. Script-disabled sample rendering made no font stylesheet request and displayed native text. All generated local asset references resolve to existing files.
- Website `pnpm lint`, `pnpm types:check` and `pnpm build` passed. The production build required local socket access for prerendering. Diff whitespace checks passed across affected repos.

The local comparison gallery, downloaded font data, verification scripts and screenshots were removed at Jerrie’s request after review. The comparison server on port 5210 and temporary sample and verification servers were stopped; Jerrie’s port 5205 was untouched. Verification findings are preserved above.

## Remaining work

No implementation work remains in the authorized scope. Windows/macOS typography and desktop-native select popups were not reverified; native font differences across platforms remain intentional. Jerrie authorized commits across all five affected repos. No publish or deployment was performed.
