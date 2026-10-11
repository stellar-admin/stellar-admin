---
name: prototype-component
description: >-
  Visual-first workflow for designing a new StellarAdmin component: build an HTML prototype in
  sandbox/html/ against the real built stylesheet (stellar-admin.css), iterate on the visuals with
  Jerrie until approved, and only then extract tag helpers and CSS. Use when designing a new
  built-in component, exploring visual variants (density, spacing, edge treatments), or whenever
  component work starts from "what should this look like" rather than a settled design.
---

# Prototype-first component design

Design the visuals in a throwaway sandbox page first; extract library code only after Jerrie approves what he sees. Do not start with tag helpers or library CSS: a rebuild and a DocsSamples restart per change is slow to iterate, a static page is cheap.

Reference example: `sandbox/html/secondary-depth.html`, a prototype written against `stellar-admin.css`: plain CSS reading the tokens, labeled options side by side, light and dark, a theme picker. Older prototypes (`app-header.html`, `sidebar.html`) predate the token stylesheet; their theme switchers name bundles that no longer exist.

## Process contract

1. **Settle the API shape first** (tag names, composition model, attributes). Use decisions already made in the conversation; ask focused questions for consequential unresolved choices. The prototype then only has to answer visual questions.
2. **Build one prototype page** in `sandbox/html/<component>.html` showing the live candidate plus labeled comparison strips for every open visual question.
3. **Iterate with Jerrie.** Screenshot, share, adjust values, repeat. He picks winning values per strip; variants do NOT become enums. There is one opinionated default, which themes vary through theme variables; authors adjust single instances with their own classes (the utilities layer beats the component layers by design).
4. **Extract** tag helpers and CSS from the approved candidate, following the product guide at `docs/repos/stellar-admin.md`: the rules go in `Client/css/components/<component>.css`, in `@layer sa.components`, reading only theme variables, foundation tokens and the component's own private `--_*` variables (`var(--sa-color-surface)`, `var(--sa-radius-control)`, `calc(4 * var(--sa-unit))`). There are no per-theme files. A treatment that must differ between themes is a theme variable; adding one is the [theme skill](../create-custom-theme/SKILL.md)'s job and needs Jerrie's approval.
5. **The prototype is a point-in-time artifact.** Code flows prototype → library, never back. After extraction, do NOT rewrite the prototype to use the shipped component classes; it stays as the record of the exploration.

## Prototype page recipe

Head of the page:

- **The real built assets**, so you judge true rendering and interactivity works: `<link rel="stylesheet" href="../../src/StellarAdmin.TagHelpers/wwwroot/stellar-admin.css">` and `<script defer src="../../src/StellarAdmin.TagHelpers/wwwroot/stellar-admin.js"></script>`. Build them first if `wwwroot/` is empty (`npm run build` in `src/StellarAdmin.TagHelpers/Client/`).
- **An empty theme link after it**, `<link rel="stylesheet" id="theme">`, that the control bar points at a theme fixture (`../../util/theme-check/presets/<name>.css`) or clears for the default theme.
- **The candidate's CSS in a `<style>` block, written as the library writes it**: plain CSS in `@layer sa.components`, reading the tokens. Extraction is then close to a copy, and the candidate follows every theme and density as the shipped components do. Use only names listed in [theme tokens](../../../docs/design/tokens.md); a private `--_*` variable of another component is not a dependency the extracted CSS may keep.
- **Fonts**: Inter (the default theme's) plus the families of the fixtures the picker offers (Lexend, IBM Plex Sans, Figtree), from Google Fonts.

Layout-only scaffolding (the page grid, the strip frames, captions) is the prototype's own CSS and is not extracted. Tailwind is not needed. If a prototype does use the Tailwind browser build (`https://cdn.jsdelivr.net/npm/@tailwindcss/browser@4`) for quick scaffolding, copy `Client/css/tailwind-adapter.css` into its `<style type="text/tailwindcss">` block so the utilities read the tokens, and link `stellar-admin.css` first so its layer order holds.

Body:

- Use the **exact HTML the tag helpers emit** for surrounding real components (correct `data-slot`s and `sa-*` classes). Don't hand-derive it; capture it from a running DocsSamples: `curl "http://localhost:5206/DocsStatic?name=<Component>%2F_Intro&layout=_CleanLayout"`. Shorten generated ids and update every `commandfor` to match.
- **The live candidate in its real position** (for example the top of the sidebar inset), with the classes it would emit and its rules in the candidate stylesheet.
- **Labeled comparison strips** below, one `<section>` per open question (density, spacing, edge treatment, arrangement…), each strip a full-width framed figure with a caption naming the values it uses. Wire every interactive control for real (same `commandfor`); the library JS keeps multiple triggers in sync.
- **A control bar** fixed at the bottom right: the theme picker (the default and at least Ops and Soft, the dense-sharp and pill-roomy ends), `.dark` on `<html>`, and a `data-sa-density="compact"` / `"comfortable"` toggle on the main area when spacing is in question. Buttons flipping `data-variant` on the relevant element where the candidate has variants.
- Sandbox content uses the Voyager Travel theme like the docs.

## Iteration loop

- Serve the repo root: `python3 -m http.server 8321 --bind 127.0.0.1` from the product repository root (background it; never ports 5205/5206). Page: `http://localhost:8321/sandbox/html/<name>.html`. The page also works via `file://`. Give Jerrie both options, and leave the server running for him while he reviews.
- Screenshot via the existing Chromium/CDP workflow; no Playwright. Use the installed Chromium executable (the VRT tool supports `--browser` / `CHROME_PATH`); ad hoc Node CDP scripts need global WebSocket support. Start Chromium with `--headless=new --remote-debugging-port=<port>`. Wait for the fonts before capturing.
- Capture the states that matter, not just the default: desktop and mobile widths, toggled and collapsed states, each layout variant (`inset` especially: a rounded card changes edge rendering), dark mode, and the theme fixtures, which move radius, density, outlines and elevation the furthest.
- `util/theme-check/contrast.mjs` also runs on a prototype page that reads `?mode=light|dark` (see its README): the candidate's text should pass in the fixtures and the random themes, light and dark.

## Gotchas

- Port 5205 is Jerrie's own DocsSamples instance; always use 5206, and `dotnet run --no-launch-profile` so launchSettings can't grab 5205. But `--no-launch-profile` drops the Development environment, which silently disables `_content` static web assets (pages render unstyled, assets 500), so always set `ASPNETCORE_ENVIRONMENT=Development`. From the product repository: `ASPNETCORE_ENVIRONMENT=Development dotnet run --project docs/DocsSamples --no-launch-profile --urls http://localhost:5206`. See [development guidance](../../../docs/development.md) for conditional local build workarounds.
- DocsSamples has no Razor runtime compilation; restart it after editing a partial.
- Stop every DocsSamples instance you started. The prototype's static server stays up for review until Jerrie is done.

Read the product `AGENTS.md` and [OSS development guide](../../../docs/repos/stellar-admin.md) before implementation. This workflow respects the user's current scope and authorization; a previously approved API or visual design does not need repeated approval.
