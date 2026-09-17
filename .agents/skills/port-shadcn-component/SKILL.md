---
name: port-shadcn-component
description: >-
  End-to-end workflow for porting a shadcn/ui component into StellarAdmin.TagHelpers: research the
  upstream source, work out which half of the CSS is already shipped, write the tag helpers, add
  DocsSamples demos, the website docs page and the generated skills reference. Use whenever adding
  or porting a component that already exists upstream — "port the X component", "add the X
  component", "add X from Base UI / shadcn / base ui", or a link to
  ui.shadcn.com/docs/components/base/X — where X is any shadcn or Base UI component name
  (bubble, message, combobox, toast, ...).
---

# Porting a shadcn component

Touches five repos. The upstream-derived visuals are settled upstream. Custom themes have their own maintained design specifications; infer their treatment from those rules and existing components without requiring a fresh design handoff.

For an authorized end-to-end port, carry the work through CSS, tag helpers, DocsSamples demos, website docs, and consumer references. Make routine Phase 0 decisions using the defaults below and report them. Respect a narrower user request (such as analysis only), explicit review checkpoints, and existing authorization; this skill does not authorize commits or publishing. Ask about a consequential unresolved design choice only when it blocks the next step.

Worked example — the Bubble port, as shipped: `TagHelpers/Bubble/`, `docs/DocsSamples/Pages/Bubble/`, `../website/content/docs/tag-helpers/components/bubble.mdx`.

## The fidelity rule

**For upstream-derived themes, duplicate shadcn's utility classes, `data-slot` names and `data-*` state attributes exactly.** Custom themes preserve the shared DOM/state contract but intentionally use their own visual language. Record consequential inferred decisions in the custom theme specification.

Verify against the **compiled bundle**, not the class strings you wrote — arbitrary variants like `[button,a]:` only prove out after Tailwind compiles them.

Keep upstream classes that are inert for us, so a later sibling component needs no CSS change (Bubble kept `group-data-[align=end]/message:self-end`, which needs a `Message` we have not built).

## Phase 0 — settle the design

Decide these yourself and record them in the final report:

- **Polymorphic content.** Upstream renders one element polymorphically; we ship sibling tags. Default to shipping both the anchor and the button variant where upstream supports both.
- **Enums.** Dedicated per component — never reuse another's. Bubble needed four; `BubbleAlign` and `BubbleReactionsAlign` both being start/end is correct, not duplication.
- **Sibling components.** shadcn often ships a family. Default to the component asked for plus whatever it cannot render without; note the rest as follow-ups.
- **Location.** This product repository owns all product features.

## Phase 1 — research

shadcn ships three bases: `base` (Base UI), `aria` (React Aria), `radix` (Radix, published as the `new-york-v4` style). **Read the Base UI source** — it is the docs-site default and the only variant written in our two-layer shape:

```bash
curl -s https://raw.githubusercontent.com/shadcn-ui/ui/refs/heads/main/apps/v4/registry/bases/base/ui/<component>.tsx
```

It has no HTTP registry endpoint (`/r/styles/base/`, `/r/bases/base/`, `/r/@base/` all 404). The only JSON the site serves is the Radix one, worth fetching for the Phase 2 cross-check:

```bash
curl -s https://ui.shadcn.com/r/styles/new-york-v4/<component>.json | jq -r '.files[].content'
```

Read `https://ui.shadcn.com/docs/components/base/<component>` for the accessibility guidance and for **what each part is semantically for** — a grouping subcomponent usually has a specific meaning (a `MessageGroup` groups consecutive messages from *one* sender, not a whole conversation). Getting that wrong produces demos that compile and mislead.

Keep notes as you go — anatomy, prop table, behavioural notes, a11y guidance — for the docs page in Phase 5. Use a tracked plan only when the task needs continuity across sessions; keep it in `docs/plans/` and update the index.

## Phase 2 — CSS

Check what is already shipped first; it usually collapses this phase to four rules. `util/ThemeGenerator` sweeps *every* `.cn-*` rule out of upstream's `style-<theme>.css`, so a new component's themed rules often land in our bundles before we write any C#:

```bash
grep 'sa-<component>' src/StellarAdmin.TagHelpers/Client/css/themes/shadcn.vega.css
```

**Do not re-run ThemeGenerator.** The rules are usually already there, and a run drags in unrelated upstream drift across every existing component — a separate chore needing its own VRT comparison.

What is left is the structural residue, and the Base UI source states it outright:

```jsx
// bases/base — cn-* marker + structural utilities
className={cn("cn-bubble-group flex min-w-0 flex-col", className)}

// new-york-v4 (radix) — everything inlined, no cn-* markers
className={cn("flex min-w-0 flex-col gap-2", className)}
```

**The structural rule is the Base UI class string minus its `cn-*` marker.** It goes in `Client/css/components.css`, inserted alphabetically. Marker classes that emit no CSS (`group/bubble`) stay in the C# class list, not the stylesheet, and need no `@source inline()` entry — real utilities appearing only in C# string literals do.

**Cross-check:** the Radix string is the union of our two layers. Diff it against `themed ∪ structural`; anything in neither has been dropped.

| Layer | File | Wins over |
|---|---|---|
| Themed | `Client/css/themes/<theme>.css`, nested `@layer components.theme` | — |
| Structural | `Client/css/components.css`, directly in `@layer components` | themed (outer layer beats nested sub-layer) |
| Author utilities | consumer markup | both |

### Custom themes and coverage

Every shipped theme must support the new component. Read [Ledger's specification](../../../docs/design/themes/ledger.md) for its palette, geometry, interaction rules, and inference method. Compose missing designs from established primitives; do not import a generated upstream theme as a fallback or copy the prototype's `.ldg-*` markup. Preserve existing tag helper semantics and author utility precedence.

Update `util/theme-coverage/coverage.json` after reviewing each theme. The manifest records component tags and styling hooks, so new subcomponents and variant hooks also require a review. Use `reviewed` with actual theme rule evidence, or `shared-only` with a rationale when shared/composed styles are sufficient. Never add empty rules merely to satisfy coverage. `pending` is rejected by builds.

```bash
node util/theme-coverage/check.mjs
node --test util/theme-coverage/check.test.mjs
```

Ledger's CSS is hand-authored. ThemeGenerator must only write upstream-derived files; it must not own custom themes. Add the real example and inspect light/dark and interactive states before marking coverage reviewed. DocsSamples supports `?theme=ledger&mode=dark`; VRT supports `--theme ledger --mode dark`.

Build the CSS directly — it fails loudly where `dotnet build` can succeed over a silently failed CSS build:

```bash
cd src/StellarAdmin.TagHelpers/Client && npm run build:css
```

Then check the new rules in `wwwroot/stellar-admin.shadcn.nova.css` against upstream.

## Phase 3 — tag helpers

`src/StellarAdmin.TagHelpers/TagHelpers/<Component>/`, one file per type. Pattern reference: `TagHelpers/Bubble/`.

**Naming: component prefix first, element role last** — `sa-bubble-link-content`, `sa-sidebar-menu-button`, `sa-item-link`, with class names to match. `sa-linkbutton` is a deliberate exception: standalone, not part of a `sa-button` compound.

- Nullable enum properties resolved at the top of `ProcessAsync`: `var effectiveVariant = Variant ?? BubbleVariant.Default;`
- `GetDataAttributeText()` on each enum in a C# 14 `extension(...)` block; the extension class is `internal` and gets no XML docs.
- `data-slot` on the primary element, plus `data-*` for every piece of state the CSS keys off.
- `JoinCssClasses(..., output.GetUserSuppliedClass())` — user classes last so they win.
- **Synchronous** `ProcessAsync` returning `Task.CompletedTask` unless you relocate child content. Never call `GetChildContentAsync` in a decorate-only helper — content renders automatically, and fetching without writing it back drops it.
- Shared `<Component>ContentRenderingHelper` when several tags render one surface as different elements (`BubbleContentRenderingHelper`, `ItemRenderingHelper`).
- Anchors derive `StellarAdminAnchorTagHelperBase`, inject `IHtmlGenerator`, and `await ApplyRouteAttributesAsync(...)` before rendering for the full `asp-*` set.
- Buttons set `type="button"` when the author has not, so one inside a form does not submit it.
- Member ordering, logical-section spacing, and mandatory `if`/`else` braces per `docs/conventions/csharp-file-organization.md`; XML docs per `docs/conventions/xml-documentation.md` (brief consumer-facing wording in multiline indented `<summary>`/`<remarks>` blocks, no mention of shadcn). Do not confuse a one-sentence summary with single-line XML formatting; CSharpier alone does not enforce these rules.
- Share IDs and resolved options through an internal sealed `<Component>Context`, published with `SetContext` before children render and read with `GetContext<T>` in children. Use required init-only context properties; resolve defaults once in the parent. Do not expose internal parent Tag Helper properties merely for children to read via `GetParentTagHelper`. Follow the context guidance in `docs/repos/stellar-admin.md`; `CarouselContext` is a reference.

Build, then format only the folder you touched. Expect 0 warnings.

```bash
dotnet build src/StellarAdmin.TagHelpers/StellarAdmin.TagHelpers.csproj
# From the product repository root, after dotnet tool restore:
dotnet csharpier format src/StellarAdmin.TagHelpers/TagHelpers/<Component>
```

## Phase 4 — DocsSamples

`docs/DocsSamples/Pages/<Component>/` — `Index.cshtml`, `Index.cshtml.cs`, one `_<Name>.cshtml` per demo. Copy the scaffolding from `Pages/Bubble/`.

- Wrap the snippet the docs should publish in `<!-- code begin -->` / `<!-- code end -->`. **The published snippet must be copy-pasteable on its own** — if the component needs a layout container to work (the flex wrapper a message thread sits in), that container goes *inside* the markers. Only page chrome the reader doesn't need stays outside.
- Content uses the **Voyager Travel** theme.
- Register the page in `Pages/Shared/_NavigationLayout.cshtml`, right `DemoGroup`, alphabetically. VRT needs no registration — it lists the `Pages/` directory.

**Demos are compositions, not specimen strips.** Show the component doing a job a reader would actually give it; demo an interactive variant in the context that makes it obviously the right choice. Bubble's link and button demos had to be rebuilt from isolated bubbles into realistic threads.

**Use each subcomponent for what it means**, per the Phase 1 notes — a demo that nests things in a structurally valid but semantically wrong way teaches the wrong API.

```bash
ASPNETCORE_ENVIRONMENT=Development \
  dotnet run --project docs/DocsSamples --no-launch-profile --urls http://localhost:5206
```

Check desktop and mobile widths, light and dark. There is no dark toggle — force it over CDP with `document.documentElement.classList.add('dark')` before capturing. **Stop the server when done.** Port 5205 is Jerrie's own instance; never touch it.

## Phase 5 — website docs

Register each partial in `docs/DocsSamplesGenerator/Generator.cs` as a `new("<Component>/_<Name>")` entry in `DemoPartials`, alphabetically, then run it:

```bash
dotnet run --project docs/DocsSamplesGenerator
```

The generator regenerates all demos, and SVG attribute order plus antiforgery tokens can differ between runs. Record pre-existing edits first. Run `bash .agents/skills/port-shadcn-component/scripts/classify-generator-output.sh ../website` from the product repository for a read-only classification, then review the actual diffs. Normalized equality can hide real SVG edits and must never trigger automatic restores. Use isolated worktrees or a pre-generation snapshot if output cleanup is needed; preserve existing edits. Check that changed theme bundles are scoped to the intended classes.

Write `../website/content/docs/tag-helpers/components/<component>.mdx` by hand and add its slug to `meta.json` under the right group. Structure, per `bubble.mdx`:

- Frontmatter `title` / `description`, then `import { Demo } from "@/components/demo";`
- Intro demo → `## Usage` with the composition skeleton → `## Examples`, one `###` per demo (prose paragraph + `<Demo>` + `<include>`).
- Fold the a11y guidance into the relevant example section, not a separate heading.
- `## API Reference`, one `###` per tag in **anatomical** order (root then children, not alphabetical), each with a `<TypeTable>`.
- Anchor-backed tags get `<include>./_shared/routing-attributes.mdx</include>` under `#### Routing attributes`.
- Prose: bracketed `<sa-thing>` for tags, bare for attributes and classes, "Tag Helper" capitalised, `...` never `…`, **no hard wrapping**.

Then run `pnpm lint`, `pnpm types:check`, and `pnpm build` from `../website/`.

## Phase 6 — skills reference

Add curated examples to `util/SkillsGenerator/skills.examples.json` — the partials that best teach the component, including at least one real composition. Paths must match the `Pages/` tree; missing files are skipped **silently**.

```bash
dotnet run --project util/SkillsGenerator
dotnet run --project util/SkillsGenerator -- --check   # must report no drift
```

Under `skills/stellar-admin-tag-helpers/references/`, `components/*.md` and `components-index.md` are generated from the UI library. Dashboard components are generated separately under `skills/stellar-admin-dashboard/references/`. Only marked `<!-- structure:begin -->` / `<!-- structure:end -->` regions inside component files are hand-authored and preserved. The other reference guides are handwritten; keep them consistent with the public docs.

## Phase 7 — verify and hand over

- No unresolved `sa-*` elements in the rendered HTML. A tag helper that fails to match leaks its custom element into the output, and nothing else will tell you.
- `dotnet build` clean with 0 warnings; `pnpm lint` clean; `SkillsGenerator --check` reports no drift.
- Screenshot the DocsSamples page and the built docs page.
- Every server stopped, ports free.

Report, in one message: what shipped in each repo, the Phase 0 decisions you took, **every deviation from upstream with its reasoning**, and anything deferred. Then list what is uncommitted per repo, and stop. Commit or push only if the user has authorized that action in the conversation; completing the component alone does not authorize publishing changes.

## Environment and instructions

Read the product `AGENTS.md`, the relevant repository guide under `docs/repos/`, and [development guidance](../../../docs/development.md) before executing commands. Paths in the phases are product-root-relative unless a working directory is stated. That guide records conditional SDK, IPv6, prune-data, formatter, and sample-server workarounds; do not assume they apply on every machine. Use the pinned SDK and local tools where available.
