---
name: prototype-component
description: >-
  Visual-first workflow for designing a new StellarAdmin component: build an HTML prototype in
  sandbox/html/ against the real built theme bundle, iterate on the visuals with
  Jerrie until approved, and only then extract tag helpers and CSS. Use when designing a new
  built-in component, exploring visual variants (density, spacing, edge treatments), or whenever
  component work starts from "what should this look like" rather than a settled design.
---

# Prototype-first component design

Design the visuals in a throwaway sandbox page first; extract library code only after Jerrie approves what he sees. Do not start with tag helpers or theme CSS — the 8-theme generator pipeline is expensive to iterate, the sandbox is cheap.

Reference example: `sandbox/html/app-header.html` (the prototype that produced `sa-app-header`). `sandbox/html/sidebar.html` is an older, cruder variant (hand-inlined tokens instead of the real bundle) — follow the app-header recipe, not that one.

## Process contract

1. **Settle the API shape first** (tag names, composition model, attributes). Use decisions already made in the conversation; ask focused questions for consequential unresolved choices. The prototype then only has to answer visual questions.
2. **Build one prototype page** in `sandbox/html/<component>.html` showing the live candidate plus labeled comparison strips for every open visual question.
3. **Iterate with Jerrie.** Screenshot, share, adjust values, repeat. He picks winning values per strip; variants do NOT become enums — one opinionated default per theme, author overrides via utility classes (utilities layer beats component layers by design).
4. **Extract** tag helpers + CSS from the approved candidate (follow the product guide at `docs/repos/stellar-admin.md`: rules go in `components.css`; theme-aware spacing references the generated per-theme variables via `gap-y-(--sa-gap-md)` / `var(--sa-gap-*)`. Only a rule that must differ per theme beyond what those variables express goes in `util/ThemeGenerator/Themes/<Theme>.custom.css`, copied verbatim into that theme's generated file on the next generator run).
5. **The prototype is a point-in-time artifact.** Code flows prototype → library, never back. After extraction, do NOT rewrite the prototype to use the shipped `sa-*` component classes; it stays as the record of the exploration, with raw utility classes.

## Prototype page recipe

Head of the page (all four pieces matter):

- **Real built assets**, so you judge true theme rendering and interactivity works: `<link id="theme-css" href="../../src/StellarAdmin.TagHelpers/wwwroot/stellar-admin.shadcn.nova.css">` and `<script defer src="../../src/StellarAdmin.TagHelpers/wwwroot/stellar-admin.js">`. Build them first if `wwwroot/` is empty (`npm run build` in `src/StellarAdmin.TagHelpers/Client/`).
- **Tailwind v4 browser build** for prototype-only utilities: `<script src="https://cdn.jsdelivr.net/npm/@tailwindcss/browser@4"></script>`.
- **Token vocabulary** in a `<style type="text/tailwindcss">` block — copy the `@theme inline` mapping from `Client/css/theme-tokens.css` (plus `@theme { --font-sans: "Geist", sans-serif; }`) so `bg-background` / `border-border` etc. resolve against whichever bundle is linked.
- **Preflight fix** in the same block — the browser build's preflight lands after the bundle's base layer and resets `border-color` to `currentColor`; restore it: `@layer base { *, ::after, ::before, ::backdrop, ::file-selector-button { border-color: var(--color-border); } }`
- Geist font links, same as the docs `_Layout.cshtml`.

Body:

- Use the **exact HTML the tag helpers emit** for surrounding real components (correct `data-slot`s and `sa-*` classes). Don't hand-derive it — capture it from a running DocsSamples: `curl "http://localhost:5206/DocsStatic?name=<Component>%2F_Intro&layout=_CleanLayout"`. Shorten generated ids (e.g. `--sa-sidebar-proto`) and update every `commandfor` to match.
- **Live candidate in its real position** (e.g. top of the sidebar inset), its classes grouped and commented by the future structural-vs-themed split (`structural: flex shrink-0 items-center` / `themed: h-14 gap-2 border-b px-4`).
- **Labeled comparison strips** below, one `<section>` per open question (density, spacing, edge treatment, arrangement…), each strip a full-width framed figure with a caption naming its exact classes. Wire every interactive control for real (same `commandfor`) — the library JS keeps multiple triggers in sync.
- **Control bar** fixed bottom-right: buttons flipping `data-variant` on the relevant element, swapping the theme `<link>` href (shadcn.nova ↔ shadcn.vega minimum), and toggling `.dark` on `<html>`. All component styling is attribute-driven, so flipping `data-*` restyles everything live.
- Sandbox content uses the Voyager Travel theme like the docs.

## Iteration loop

- Serve the repo root: `python3 -m http.server 8321 --bind 127.0.0.1` from the product repository root (background it; never ports 5205/5206). Page: `http://localhost:8321/sandbox/html/<name>.html`. The page also works via `file://` — give Jerrie both options, stop the server when done.
- Screenshot via the existing Chromium/CDP workflow — no Playwright. Use the installed Chromium executable (the VRT tool supports `--browser` / `CHROME_PATH`); ad hoc Node CDP scripts need global WebSocket support. Start Chromium with `--headless=new --remote-debugging-port=<port>`. Wait ~3s after navigation for the browser Tailwind build + fonts before capturing.
- Capture the states that matter, not just the default: desktop + mobile widths, toggled / collapsed states, each sidebar variant (`inset` especially — rounded card changes edge rendering), dark mode, and at least one contrasting theme.

## Gotchas

- Port 5205 is Jerrie's own DocsSamples instance — always use 5206, and `dotnet run --no-launch-profile` so launchSettings can't grab 5205. But `--no-launch-profile` drops the Development environment, which silently disables `_content` static web assets (pages render unstyled, assets 500) — always set `ASPNETCORE_ENVIRONMENT=Development`. From the product repository: `ASPNETCORE_ENVIRONMENT=Development dotnet run --project docs/DocsSamples --no-launch-profile --urls http://localhost:5206`. See [development guidance](../../../docs/development.md) for conditional local build workarounds.
- DocsSamples has no Razor runtime compilation — restart it after editing a partial.
- Always stop every server you started.

Read the product `AGENTS.md` and [OSS development guide](../../../docs/repos/stellar-admin.md) before implementation. This workflow respects the user's current scope and authorization; a previously approved API or visual design does not need repeated approval.
