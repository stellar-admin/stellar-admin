# Component showcase examples

## Code audit — 2026-09-18

Current status: **completed**. `docs/DocsSamples/Pages/Showcase/` retains Masonry and ThemeShowcase with shared GalleryParts; `docs/DocsSamples/Client/css/site.css` contains the flexible tile layout. Both are registered in `docs/DocsSamplesGenerator/Generator.cs`; the website has both exports and its theming page embeds `showcase-theme-showcase.html`. Earlier visual-review and layout-selection checkpoints are closed.

This assessment uses the current checkout; earlier status, paths, permissions and verification notes below describe historical sessions. Runtime/browser and hosted release checks were not rerun for this documentation audit.

## Historical record

Status: implemented; visual review pending. Last updated: 2026-09-13.

## Scope and decisions

Build three alternative screens in OSS DocsSamples for reviewing themes and potentially embedding on the website: a booking workspace, a project workspace, and a traveller support inbox. The user clarified that interactivity means ordinary component behaviour; sample data stays static and there is no simulated saving, filtering, messaging, or backend workflow.

The pages use existing OSS tag helpers, theme tokens, and local sample images. Each has a reusable Razor partial and a page under `/Showcase/Bookings`, `/Showcase/Project`, or `/Showcase/Support`. The samples navigation includes a Showcases group. Add `?clean` to render without the sample navigation or demo padding, and use the existing `theme` and `mode` query parameters to select appearance.

## Verification

DocsSamples builds successfully with 10 existing nullable-property warnings. Commands ran from the workspace with installed SDK 10.0.400, rather than the nested repo's 10.0.100 pin. Browser checks cover all three pages at 1280px in Ledger light and Aurora dark, 1000px in shadcn.nova light, and 390px in Ledger dark. Tag helpers render, local images load, dialogs open and close, and there is no document-level horizontal overflow. Separately exercised the status dropdown, checkbox, switch, segmented control, accordion, and normal sample navigation. Screenshots and temporary verification scripts are in `/tmp/showcase-*` and `/tmp/check-showcase-*.mjs`.

## Review and follow-up

Choose and refine the preferred screen with Jerrie. Desktop height varies with theme metrics, currently approximately 750–860px; mobile layouts stack. Website embedding and static demo generation are not part of this implementation and remain future work if requested. Changes are uncommitted in `stellar-admin/` (Showcase pages and navigation) and the workspace (this record and plan index). No website, Pro, or consumer-skills changes.

## Component galleries (2026-09-13)

Jerrie liked the application examples but requested additional compositions that prioritise component breadth without an application workflow. Added `/Showcase/Gallery` (nine compact cards), `/Showcase/Mosaic` (nine cards mixing imagery, forms, messages, data, and overlays), and `/Showcase/Specimen` (twelve specimens in four open comparison rows). Twelve shared partials under `Pages/Showcase/GalleryParts` keep the compositions consistent. The original three application examples are retained. All pages support the existing clean, theme, and mode query parameters and appear in sample navigation.

The new pages cover buttons and groups, disabled/loading states, inputs and validation, selects, OTP, textareas and input groups, checkboxes, radios, switches, segmented and toggle controls, range sliders, avatars, badges, skeletons, spinners, alerts, progress, attachments, breadcrumbs, tabs, pagination, dialogs, sheets, popovers, dropdowns, accordions, messages, bubbles, tables, carousel, empty states, and confirmation dialogs. Actions are component demonstrations with static content. The OTP specimen uses `max-w-fit` to keep its deliberately oversized invisible input overlay from extending beyond the page.

Verification: DocsSamples build passes with the same 10 existing warnings using workspace SDK 10.0.400. All three galleries checked at 1280px with Ledger light and Aurora dark, 1000px with shadcn.nova light, and 390px with Ledger dark: no document overflow, duplicate IDs, unrendered tag helpers, or broken images. Exercised dialog, sheet, confirmation, popover, dropdown, accordion, switch, checkbox, radio, segmented control, OTP typing, and carousel navigation. Screenshots are `/tmp/showcase-{Gallery,Mosaic,Specimen}-*.png`; temporary scripts are `/tmp/check-galleries.mjs` and `/tmp/gallery-controls.mjs`. Gallery and Mosaic desktop heights are approximately 875–965px across those themes; the open Spectrum comparison is longer. Preview server left running on port 5206 for review. Website integration remains future work.

## Specimen export and theming docs (2026-09-13)

Jerrie selected Specimen and authorized exporting it and embedding it in the theming documentation. Registered `Showcase/_Specimen` in the Pro-hosted DocsSamplesGenerator with a padded OSS `_ShowcaseLayout`. Specimen now uses explicit shared-partial paths so rendering works through `/DocsStatic`. The generator now checks the HTTP response status before writing a demo: the first export exposed a missing-partial error that was previously written as an HTML artifact while generation reported success.

Ran the full 413-demo generator into `/tmp/stellar-specimen-export`, then copied only `showcase-specimen.html`, its generated source include, and the new referenced `site.0cr0yl9dtk.css` asset into the website. Existing theme bundles and image/JavaScript assets already matched. No unrelated generated demos were replaced. The theming page now leads with an interactive Demo and theme picker, plus a standalone-preview link. Enabled Fumadocs' existing `full` frontmatter option in the docs route and selected it for the theming page to preserve the specimen's desktop columns.

Verification: export succeeds for all 413 registered demos; website lint, type checking, and production build pass. Browser verification at `http://localhost:3001/docs/tag-helpers/theming` confirms the rendered specimen, all 15 theme options, a live switch to Ledger, dark-mode synchronization, dialog opening, auto-resizing, and no horizontal overflow at 390px. Screenshot: `/tmp/theming-specimen-desktop.png`; verification script: `/tmp/verify-theme-specimen.mjs`. Generator used workspace SDK 10.0.400 and reported NU1900 vulnerability-feed warnings; export completed successfully. Preview website left running on port 3001 for review.

The export and docs embedding requested in this session are complete. Uncommitted changes span OSS (samples/layout), Pro (generator registration and HTTP status check), website (docs, full-page option, generated specimen/snippet/CSS), and workspace (plan records). Home-page integration remains unrequested.

## Responsive masonry showcases (2026-09-13)

Added three more compositions at Jerrie's request: `/Showcase/Masonry` (compact cards), `/Showcase/MasonryTiles` (tinted tiles with imagery first), and `/Showcase/MasonryEditorial` (numbered studies with open surfaces and dividing rules). Each includes all twelve shared component modules in a different arrangement, has a reusable partial, supports the existing clean/theme/mode query parameters, and appears in Showcase navigation. Sample-only CSS uses automatic column widths and indivisible items so layouts adapt to their container, reading down each column before continuing to the next. No JavaScript layout dependency or simulated data workflows. None of these pages have been registered for export or added to the website.

Verification: DocsSamples build succeeds with 10 existing nullable warnings and one NU1900 vulnerability-feed warning, using installed workspace SDK 10.0.400 instead of the nested 10.0.100 pin. Browser checks on all three pages at 320, 390, 740, 1000, 1280, and 1600px across Ledger, shadcn.nova, and Aurora in light/dark modes confirm one through four columns, no horizontal document overflow, no fragmented items, no duplicate IDs, no unrendered tag helpers, and no broken images. Visually inspected desktop cards, desktop editorial, and mobile tiles. Dialog, sheet, confirmation, switch, popover, accordion, and reflow without overlap pass on all three pages; navigation links are present. Temporary scripts: `/tmp/check-masonry.mjs` and `/tmp/masonry-interactions.mjs`; screenshots: `/tmp/showcase-Masonry*.png`. Agent preview restarted on port 5206; Jerrie's port 5205 untouched. This addition changes only OSS samples and workspace plan records; earlier Pro and website edits remain intact and uncommitted.

## Editorial button and input groups (2026-09-13)

Added dedicated Button groups and Input groups studies to Editorial, bringing it to fourteen sections. New partials demonstrate grouped icon buttons, a text segment between buttons, a split button with an options popover, currency prefix/suffix addons, an embedded search button, and a textarea with block-start/block-end addons and a help popover. Only Editorial includes the two new partials; other showcases and exports are unchanged.

Verification: DocsSamples build passes with the same 11 existing warnings under workspace SDK 10.0.400. Checked Editorial at 320–1600px across Ledger, Aurora, and shadcn.nova light/dark: no document overflow, fragmented studies, duplicate IDs, unrendered tag helpers, or broken images. Both new popovers work; existing overlays, switch, accordion, and overlap checks pass. Visually reviewed the desktop composition. Agent preview remains on port 5206. No export or commit performed.

## Final selection and exports (2026-09-13)

Jerrie selected only Masonry Editorial and Specimen. Removed the other seven sample compositions and their unused header partial. Renamed the retained pages/partials to Masonry and ThemeShowcase and reduced navigation to those two entries. Shared component partials remain in GalleryParts. Removed unused masonry variant CSS. Masonry's introductory header now belongs to its sample page, leaving `_Masonry` as the controls-only layout for export; the fourteen numbered section titles remain.

Registered both retained partials for export. Generated all 414 demos into `/tmp/stellar-showcases-final`, then copied only the two selected HTML demos, their source includes, and the changed referenced `site.1qz1du1udn.css` asset. Removed the superseded specimen HTML/include and its now-unreferenced sample CSS asset. The theming docs now reference `showcase-theme-showcase.html`; Masonry is available at `showcase-masonry.html` without adding it to a docs page.

Verification: generator and DocsSamples build pass (existing nullable/NU1900 warnings; workspace SDK 10.0.400). Website lint, type checks, and production build pass. Browser checks cover both renamed samples and both exports at 1280 and 390px: no horizontal overflow or broken images. Masonry export contains fourteen studies and no introductory heading, description, or badge; its options popover works. The theming page references the renamed export; all eight obsolete route names return 404. Screenshot: `/tmp/final-masonry-export.png`; script: `/tmp/verify-final-showcases.mjs`. Diff checks pass in OSS, Pro, and website. Changes remain uncommitted across those repos and workspace plans. Agent preview remains on 5206 and website on 3001.

## Narrower docs preview (2026-09-14)

After Jerrie removed full-width rendering from the theming page, replaced ThemeShowcase's single-column-to-three-column 900px jump with an auto-fitting grid: 15rem minimum study width and 1rem gaps. The grid now fits two columns from 496px of available grid space and three from 752px; narrower containers retain one column. Existing wide-layout section labels remain unchanged. The shared navigation sample allows its inner tab list to wrap, keeping Sera's wider tabs within their study; Masonry receives that same adjustment. No documentation page-width change.

Regenerated the 414 website demos and the spectrum source include. The shared sample CSS fingerprint changes across exports; reviewed other HTML differences as SVG attribute ordering (with attribute values preserved), antiforgery tokens and consistently remapped generated IDs. Only the two showcase HTML files have additional markup changes. All referenced exported assets resolve.

Verification: sample build and generator pass using workspace SDK 10.0.400 (11 existing sample warnings, including NU1900); website lint, type checking and final production build pass. Chromium checked all 15 themes at ten preview widths from 360–1280px: no document horizontal overflow after the tab-wrap adjustment. Visually inspected phone, two-column and three-column layouts. Minor existing spinner/focus paint can extend a few pixels beyond a study; no navigation spill remains. Changes are uncommitted in OSS, website and this workspace; Pro remains clean. No remaining implementation work.

## Alternative flow exploration (2026-09-14)

Jerrie requested alternatives before selecting another layout because some controls need more width than an equal-column grid provides. A temporary live review at `http://localhost:8321/` compares flexible tiles with different preferred widths, flowing CSS columns, broader grouped panels, and the current spectrum. The review includes preview-width, theme and light/dark controls; candidates reuse the exported real component markup and assets. Files are under `/tmp/stellar-spectrum-review/`; the local review server is intentionally left running for Jerrie. The earlier source and generated edits remain intact; no candidate has been applied or committed. Next step: obtain Jerrie's layout preference and refine that candidate before implementation.

## Flexible tiles implemented (2026-09-14)

Jerrie selected alternative A and authorized implementation. ThemeShowcase now renders twelve independent sections in the approved order, using wrapping flex rows rather than the four category grids. Compact tiles request 220px, medium tiles 280px and wide tiles 340px, with proportional growth into spare row space. Gaps are 24px, reduced to 20px when the showcase container is at most 400px. Navigation, text inputs, structured data and conversation receive the wider sizing. Styles belong to the samples' `Client/css/site.css`; the library theme bundles are unchanged. The retained navigation tab wrapping works across the wider-font themes.

Regenerated all 414 website demos and the spectrum source include. Compared generated HTML with a pre-implementation snapshot: only the spectrum changes beyond stylesheet fingerprints, SVG attribute ordering, antiforgery tokens and consistently remapped generated IDs. All referenced local assets resolve. Sample build and generator pass under workspace SDK 10.0.400 (11 existing build warnings); website lint, type checking and production build pass. Chromium verified 180 combinations (15 themes, light/dark, 360/390/560/760/1024/1280px), with twelve tiles and no document overflow or significant tile spill in every case. The dialog still opens; the implemented docs-width screenshot matches the approved composition.

Implementation is complete. Jerrie authorized committing all pending changes in OSS, website and this workspace record. Pro and consumer skills remain clean. No push was requested. The alternative-review server remains available on 8321 using a frozen copy of the pre-implementation assets; the implementation verification server on 5211 was stopped. No pending implementation work.
