# Plan: templated tag helpers with slots

## Code audit — 2026-09-18

Current status: **completed**. StellarAdminTemplatedTagHelperBase, SlotContent/SlotOutlet helpers, samples and Dashboard FormPage/IndexPage implement view overrides and slots. Ordinary non-slot child output is intentionally discarded by the templated base.

This assessment uses the current checkout; earlier status, paths, permissions and verification notes below describe historical sessions. Runtime/browser and hosted release checks were not rerun for this documentation audit.

## Historical record

**Index status:** completed. **Indexed:** 2026-09-05. Historical record; completion is recorded in the phase/progress notes below. Deferred items require a separately scoped task.

## Progress

| Phase | Repo | State | Commit |
|---|---|---|---|
| 1 — Templated tag helper (no slots) + docs sample | stellar-admin + pro | done | OSS `e22fd7e`, pro `0da89d5` |
| 2a — Slot content capture (`sa-slot-content`) + docs sample | both | done | OSS `04982ea`, pro `95a3951` |
| 2b — Slot outlet (`sa-slot-outlet`) + templated-base integration + docs samples | both | done | OSS `9b2dffb`, pro `95da1eb` |
| 3 — Identity form pages adopt `sa-form-page` | stellar-admin-pro | done | pro `5694369` |

Stop for review after every phase. Jerrie commits.

**2026-08-12, second attempt.** The first Phase 1 (templated base + slots + an
unrequested unconsumed-fill throw, all at once) was fully reverted at Jerrie's
direction. Rebuilt in his ordering: templated tag helper *first and alone* — no slot
code exists anywhere now — with docs samples proving it. Slots come later as their own
reviewed step(s).

**Phase 1 as built:**
- OSS: `TagHelpers/StellarAdminTemplatedTagHelperBase.cs` only — abstract `ViewName`,
  `view` attribute override, virtual `GetViewModel()` defaulting to the calling view's
  model, GetView→FindView resolution, renders into the output with shared ModelState.
  Child content is currently ignored (setting output content means children never
  execute); slots will change that later.
- DocsSamples: demo `TripCardTagHelper` (`<docs-trip-card booking="...">`, view
  `_TripCard`) in `TagHelpers/`; views `Pages/Shared/_TripCard.cshtml` +
  `_TripCardCompact.cshtml`; page `Pages/TemplatedTagHelper/` (Intro + View Override
  examples); nav group "Extensibility" in `_NavigationLayout.cshtml`. Example partials
  keep explanatory Razor comments *above* the `<!-- code begin -->`/`<!-- code end -->`
  markers, which fence only the markup the generator exports; both partials are
  registered in the generator's `DemoPartials` list. The OSS `CLAUDE.md` tag-helper
  section documents the new base class.
- Verified on :5206/TemplatedTagHelper: default view renders the full card
  (London, UK / TRP-4821), `view="_TripCardCompact"` renders the compact row
  (Kyoto, Japan). Server stopped afterwards.

**Phase 2a (slot content) as built (2026-08-12, OSS `04982ea`, pro `95a3951`):**
Jerrie scoped this step to the capture side only, demonstrated by consumption in a
*custom* tag helper — no outlet, no templated-base involvement yet.

- OSS: `SlotTagHelper` reworked into `TagHelpers/SlotContentTagHelper.cs` (root
  folder, not the `Slot/` subfolder the spec below suggested) targeting
  `<sa-slot-content name="...">`; body is Jerrie's original unchanged — register on
  `ParentTagHelper` via `TryAddNamedSlot`, suppress own output; throws on blank name,
  missing StellarAdmin parent, or duplicate name. Base class: `Init`'s exclusion check
  now names `SlotContentTagHelper`; `TryAddNamedSlot`/`TryGetNamedSlot` gained XML
  docs and `[NotNullWhen(true)]` on the out param. Slot content type stayed
  `TagHelperContent` (spec's `IHtmlContent` switch not needed yet) and no protected
  `NamedSlots` dictionary view was added — revisit both if/when the templated base
  needs to enumerate fills. Deleted `SlotTestTagHelper.cs` and the commented
  playground markup; OSS `CLAUDE.md` tag-helper section documents named slots.
- DocsSamples: `TagHelpers/BookingSummaryTagHelper.cs`
  (`<docs-booking-summary booking="...">`) is the teaching artifact for hosting slots
  in a consumer's own tag helper: `GetChildContentAsync()` runs the
  `<sa-slot-content>` children, `TryGetNamedSlot("actions", ...)` renders the slot in
  the header, and the remaining children become the body. Page `Pages/SlotContent/`
  (renamed from `Pages/Slot/` on 2026-08-12, pro `5ef4611`; single
  Intro example, comment above the code markers), nav item "Slot Content" in the
  Extensibility group, partial registered in `DemoPartials`.
- Verified on :5206/Slot: the Edit button renders inside the header next to
  "London, UK" and the fill element itself emits nothing. Server stopped afterwards.

**Phase 2b (slot outlet) as built (2026-08-12, OSS `9b2dffb`, pro `95da1eb`):** Jerrie
asked for the simplest possible shape and approved it ("so much simpler").

- OSS: `StellarAdminTemplatedTagHelperBase.ProcessAsync` now starts with
  `GetChildContentAsync()` so `<sa-slot-content>` children register on the templated
  helper through the ordinary parent-stack mechanism (other child output is discarded),
  and it puts **itself** — not a registry — into the `ViewDataDictionary` it builds,
  under internal `SlotHostViewDataKey` (always set, so a nested templated render never
  sees an outer host). New `TagHelpers/SlotOutletTagHelper.cs`
  (`<sa-slot-outlet name="...">`, required name, no default slot): one sync `Process`
  that reads the host from `ViewContext.ViewData` and calls the public
  `TryGetNamedSlot`; filled → emit content (fallback children never execute), unfilled
  or no host (view used as a plain `<partial>`) → own children render as fallback. No
  wrapper tag. **The unconsumed-fill throw was deliberately dropped** (see amended
  decision below); there is no consumed-tracking anywhere.
- DocsSamples: dedicated `TagHelpers/BookingCardTagHelper.cs`
  (`<docs-booking-card booking="...">`, view `Pages/Shared/_BookingCard.cshtml` with
  the "actions" outlet + default "View details" button in the card footer) — a first
  outlet demo reusing `_TripCard` was rebuilt as this separate component at Jerrie's
  request so the Templated Tag Helper page stays free of slot concepts (`_TripCard`
  restored byte-identical). Page `Pages/SlotOutlet/` with Intro (fill renders a
  button group in the footer) and Fallback (no fill → default button) examples; nav
  item "Slot Outlet"; partials in `DemoPartials`.
- Verified on :5206: SlotOutlet Intro shows Check in / Cancel trip in the footer,
  Fallback shows View details, and /TemplatedTagHelper contains no slot markup.
  Server stopped afterwards.

**Phase 3 as built (2026-08-13, pro `5694369`):** all five steps landed, with one
deviation from the spec below: `FormPageTagHelper` takes the form view model as an
**explicit required `model` attribute** (`<sa-form-page model="Model"/>`) and overrides
`GetViewModel()`, instead of relying on the base's implicit calling-view-model default —
Jerrie's preference (explicit over implicit), matching the docs demos. The class lives at
`Areas/StellarAdmin/TagHelpers/FormPageTagHelper.cs`. Whether the base's implicit default
should go entirely is deliberately deferred (see below). Smoke test passed both ways:
the RCL `_FormPage` resolves from RCL views, and a temporary app-side
`Areas/StellarAdmin/Views/Shared/_FormPage.cshtml` shadow won (deleted after). Verified
in IdentitySimplePlayground on :5206: Save + Delete side by side on Users/Roles edit
(trigger inside the main form as a non-submitting invoker, dialog after the form, no
`sa-slot-*` residue in the HTML), invalid role-name POST re-renders with field errors
(ModelState flows through the templated render), delete POST 302s to the index and the
row is gone, and a headless-chromium CDP check confirmed the dialog opens on the trigger
click and closes on cancel. `app.db` restored and servers stopped afterwards.

**Follow-up: Users/Create + four-outlet contract (2026-08-13, pro `9a5016a`):** the
first deferred item is done and the slot names were reworked at Jerrie's direction.
`Users/Create.cshtml` is now `<sa-form-page model="Model">` filling a slot with its
password fieldset (`EditorFor` over the view model's `Password`/`ConfirmPassword`) —
the near-copy of the form chrome is deleted. This works because slot content executes
in the calling page's context: the editors bind against `UserCreateViewModel`, keep
their unprefixed names, and resolve the `Password` editor template normally, so the
controller's `CreateUserPasswordInput` parameter bind is untouched. `_FormPage` now
exposes **four outlets**: `pre-form-fields` (between the validation summary and the
field list), `post-form-fields` (after the field list; the create page's password
fieldset), and inside the horizontal actions field `pre-form-actions` (before submit)
and `post-form-actions` (after submit; the edit pages' delete trigger) — renamed from
the single `extra-fields`/`form-actions`. Unfilled outlets have no fallback children
and render nothing. One normalization: the create page's validation summary now sits
inside the `sa-field-group` like every other form page. Verified on :5206: field order
(UserName, Email, password fieldset, submit), mismatched-password round trip
(compare error + retained values), successful create (302, row in db), edit pages
unchanged; `app.db` restored, servers stopped.

**Follow-up: `sa-index-page` (2026-08-13, pro `a4e2304`):** the second deferred item is
done. `IndexPageTagHelper` (`Areas/StellarAdmin/TagHelpers/`, same shape as the form
one; the required `model` attribute is typed `IIndexViewModel`, which is public though
`[EditorBrowsable(Never)]`) renders `_IndexPage`; `Users/Index.cshtml` and
`Roles/Index.cshtml` are `<sa-index-page model="Model"/>`. `_IndexPage` gained one slot
pair — `pre-page-actions` / `post-page-actions` around the create button in
`sa-page-header-actions` — for apps adding actions (Export etc.) without shadowing the
chrome; grid-adjacent slots (`pre-grid`/`post-grid` inside `#result`) were considered
and deliberately **not** added until a real use case asks. Verified by before/after
capture of three index pages: byte-identical after normalizing per-request antiforgery
tokens (the remaining SVG attribute-order diffs were proven pre-existing per-process
noise in the icon rendering — same code, restarted server, same 124 shuffled lines);
unfilled outlets add only whitespace; a temporary app-side `Users/Index.cshtml` shadow
(own `@addTagHelper` lines) filling `post-page-actions` rendered its button after
New user, Roles untouched, removed cleanly. **Known separate issue:** the icon tag
helper emits SVG attributes in per-process-random order — harmless in browsers, noisy
for future HTML-snapshot/VRT comparisons; belongs to the OSS repo as its own task.

## Why

The Users/Roles Edit pages render the delete button through `_DeleteConfirmDialog`, which
wraps its trigger in its own `sa-page-container` — so the button lands in a weird spot
below the Save button instead of next to it. Save lives inside the shared `_FormPage`
partial, and nothing can inject markup into a specific place inside a partial: the tag
helper slot mechanism (`SlotTagHelper` + the parent stack in `StellarAdminTagHelperBase`)
cannot cross a partial boundary, because each partial is a separate `RazorPage` with its
own `TagHelperScopeManager`, and `context.Items` flows parent→child only.

## Settled design (discussion 2026-08-12)

A **templated tag helper**: a tag helper whose rendering *is* a view resolved through
normal view discovery (`ICompositeViewEngine.GetView` relative to the executing file,
then `FindView` — the exact two-step already proven in
`DataGridColumnTagHelper.ResolveDisplayTemplate`/`RenderTemplate` in the pro repo). The
tag helper is a thin shell; all markup stays in a `.cshtml` a consumer can override by
path shadowing, so both extension layers survive: shadow the view to restructure
everything, or fill a slot to inject into one spot.

Slots then work with **no cross-view transport problem**: capture happens in the page's
tag tree (children of the templated component), and the base class hands the captured
content into the view through the `ViewDataDictionary` it constructs itself.

Naming is **option 2** from the discussion — the Blazor `SectionContent`/`SectionOutlet`
shape with slot vocabulary:

- `<sa-slot-content name="x">...</sa-slot-content>` — filler; direct child of a templated
  component; captured and suppressed in place.
- `<sa-slot-outlet name="x">fallback</sa-slot-outlet>` — outlet; used inside the rendered
  view; emits the filled content, or its own children as fallback when unfilled.

Explicitly decided:

- The delete button goes **through the slot** from `Edit.cshtml` — `_FormPage` stays
  ignorant of delete. (Model-driven rendering of the delete trigger inside `_FormPage`
  was considered and rejected.)
- No `slot="..."` attribute-sugar filler (rejected: hijacks a real global HTML attribute,
  fiddly late-order capture of arbitrary elements). Explicit elements only.
- The mechanism (base class + both slot tag helpers) is **OSS**, in
  `StellarAdmin.TagHelpers`. Docs samples before any Identity adoption.
- ~~A fill naming a slot the view never rendered **throws** after the view renders~~ —
  **dropped in Phase 2b** for simplicity: a fill nobody reads renders nothing,
  everywhere, matching the general-slots behavior below. No consumed-tracking exists;
  reintroduce only if Jerrie asks.
- **Slots are a general capability of every StellarAdmin tag helper** (Phase 1 review
  decision, 2026-08-12): storage lives on `StellarAdminTagHelperBase` (public
  `TryAddNamedSlot`/`TryGetNamedSlot`, protected `NamedSlots`), and
  `<sa-slot-content>` registers with its nearest parent unconditionally, so any
  current or future tag helper — including consumer-authored ones — can accept and
  consume named slots without being templated. The templated base is just one
  consumer. Consequence: a fill inside a component that never reads its slots renders
  nothing; that is documented behavior, not an error.
- An outlet rendered outside a templated render (the view used as a plain
  `<partial>`) renders its fallback children — views stay usable standalone.

## Phase 1 — OSS mechanism (repo: `stellar-admin`)

New files in `src/StellarAdmin.TagHelpers/`:

1. **`TagHelpers/StellarAdminTemplatedTagHelperBase.cs`** (root, next to
   `StellarAdminTagHelperBase`) — public abstract base, constructor-injects
   `ICompositeViewEngine`, `[ViewContext]`-bound `ViewContext`. Shape:
   - `protected abstract string ViewName { get; }` — the subclass's default view name.
   - `[HtmlAttributeName("view")] public string? View { get; set; }` — per-instance
     override; app-relative paths (`/`, `~/`) used verbatim, like DataGrid's `template`.
   - `protected virtual object? GetViewModel()` — defaults to the executing page's
     `ViewContext.ViewData.Model`, so a wrapper whose view models the page's own model
     needs no override.
   - Slot storage: the dictionary + consumed-set live on the base instance (the old
     `_namedSlots` idea, relocated here); `SlotContentTagHelper` registers into it.
   - `ProcessAsync`: `GetChildContentAsync()` executes children (fills register
     themselves and suppress; this is legitimate relocation, not the forbidden
     fetch-and-reappend). Leftover non-whitespace child content becomes the **default
     slot**. Resolve the view (`GetView(ViewContext.ExecutingFilePath, …)` →
     `FindView`, throw with searched locations on failure). Render with
     `new ViewDataDictionary<object?>(ViewContext.ViewData, model)` — the copy shares
     `ModelState`, so validation summaries inside the view keep working — with the slot
     registry stuffed under an internal ViewData key, into a `StringWriter` via a nested
     `ViewContext` (mirror `DataGridColumnTagHelper.RenderTemplate`). Afterwards, any
     registered fill not consumed by an outlet → `InvalidOperationException` naming it.
     Output: no tag, content = the rendered HTML.
2. **`TagHelpers/Slot/SlotContentTagHelper.cs`** — `<sa-slot-content name="x">`.
   Rework of the existing `SlotTagHelper` (same capture-and-suppress body). Required
   `name`; registers with its nearest `ParentTagHelper` via `TryAddNamedSlot` (throws
   only when there is no StellarAdmin parent at all); duplicate name → throw.
3. **`TagHelpers/Slot/SlotOutletTagHelper.cs`** — `<sa-slot-outlet name="x">`.
   `name` optional; omitted = the default slot. Reads the registry from
   `ViewContext.ViewData`; filled → emit content and mark consumed; unfilled or no
   registry present → render its own children as fallback. Emits no wrapper tag.

Cleanup in the same phase:

- Delete `SlotTestTagHelper.cs` (commented-out experiment).
- Delete `SlotTagHelper.cs`. `_namedSlots` / `TryAddNamedSlot` / `TryGetNamedSlot`
  **stay on `StellarAdminTagHelperBase`** (content type now `IHtmlContent`, plus a
  protected `NamedSlots` dictionary view for hosts that consume all slots, like the
  templated base); the `this is not SlotTagHelper` special case in `Init` becomes
  `this is not SlotContentTagHelper` (fills still don't push themselves, so nested
  content sees the component being filled as its parent).

No CSS work: none of these emit styled markup, so no `components.css`, no safelist, no
theme touches. XML docs per `docs/conventions/xml-documentation.md` (these are
consumer-facing — the base class is a public extension point); member ordering per
`csharp-file-organization.md`.

**Verify:** `dotnet build src/StellarAdmin.TagHelpers/StellarAdmin.TagHelpers.csproj
-p:AllowMissingPrunePackageData=true` builds clean. Behavior is exercised for real in
Phase 2 — no unit-test project exists yet. **Checkpoint: review with Jerrie.**

## Phase 2 — Docs samples (repo: `stellar-admin-pro`, `docs/DocsSamples`)

The base class is abstract, so the sample needs a concrete demo component — which is
itself the documentation of "how you build one":

1. **Demo templated tag helper** in `docs/DocsSamples/TagHelpers/` (the
   `@addTagHelper *, DocsSamples` line already picks this folder up):
   `<docs-trip-card>` — Voyager Travel themed (e.g. a trip summary card), view
   `Pages/Shared/_TripCard.cshtml`, one named slot (`actions`, with a fallback button)
   plus the default slot for body content.
2. **Sample pages** under `Pages/SlotContent/` following the house pattern (`Index.cshtml` of
   `<docs-example>` blocks, one partial per example):
   - `_Intro` — the concept in a sentence or two.
   - `_Fallback` — `<docs-trip-card/>` with nothing filled: fallback content renders.
   - `_NamedSlot` — filling `actions` via `<sa-slot-content>`.
   - `_DefaultSlot` — body content flowing to an unnamed `<sa-slot-outlet>`.
   - `_ViewOverride` — the `view` attribute pointing at an alternative card view.
   All example copy in the Voyager Travel theme; "..." never "…".
3. Register the page wherever the component index requires (`StaticData.cs` /
   generators — follow whatever the last added component needed).

**Verify:** run DocsSamples on **5206** with `ASPNETCORE_ENVIRONMENT=Development`
(required for `_content` assets), browse `/Slot`, exercise every example; stop my server
instance afterwards (Jerrie runs his own on 5205). **Checkpoint: review with Jerrie.**

## Phase 3 — Identity form pages (repo: `stellar-admin-pro`)

Do step 1 as a smoke test **before** building the rest on top of it — it is the one
untested assumption (view discovery from a controller-context area view living in an
RCL, including app shadowing).

1. **`FormPageTagHelper`** (`<sa-form-page>`) in `StellarAdmin.Identity` — subclass with
   `ViewName => "_FormPage"`; the default `GetViewModel` already passes the page's
   `FormViewModel` through. Add `@addTagHelper *, StellarAdmin.Identity` to the RCL's
   `Areas/StellarAdmin/Views/_ViewImports.cshtml`. Smoke-test in
   `IdentitySimplePlayground`: the tag helper resolves the RCL `_FormPage`, and an
   app-side `Areas/StellarAdmin/Views/Shared/_FormPage.cshtml` shadow still wins.
   (Note for later consumer docs: an app's *shadowed page views* use the app's own
   `_ViewImports`, so consumers overriding `Edit.cshtml` need the `@addTagHelper` line
   in their area imports.)
2. **`_FormPage.cshtml`** — add `<sa-slot-outlet name="form-actions"/>` beside the
   submit button inside the existing horizontal `sa-field`.
3. **`Users/Edit.cshtml` and `Roles/Edit.cshtml`** — replace
   `<partial name="_FormPage"/>` with:
   ```cshtml
   <sa-form-page>
       <sa-slot-content name="form-actions">
           <sa-button type="button" variant="ButtonVariant.Destructive"
                      command="show-modal" commandfor="delete-confirm-dialog">@Model.Delete!.Title</sa-button>
       </sa-slot-content>
   </sa-form-page>
   <partial name="_DeleteConfirmDialog" model="Model.Delete"/>
   ```
   The trigger is a `type="button"` invoker, legal inside the main `<form>`; the dialog
   (which contains its own forms) stays outside it.
4. **`Roles/Create.cshtml`** — becomes `<sa-form-page/>`. (`Users/Create.cshtml` stays
   standalone; see deferred.)
5. **`_DeleteConfirmDialog.cshtml`** — strip the `sa-page-container` + trigger button;
   the partial becomes just the `<sa-alert-dialog>`.

**Verify** in `IdentitySimplePlayground`: Users and Roles edit pages show Save and
Delete side by side; dialog opens, cancel closes, confirm deletes; a validation-error
round trip still renders errors (ModelState flows through the shared ViewData); create
pages unchanged. Restore the committed `app.db` after any form-POST testing.
**Checkpoint: review with Jerrie.**

## Deferred / follow-ups

- ~~`Users/Create.cshtml` onto `<sa-form-page>`~~ — done 2026-08-13 (pro `9a5016a`,
  see the follow-up block above).
- ~~`_IndexPage` as a templated component (`sa-index-page`)~~ — done 2026-08-13 (pro
  `a4e2304`, see the follow-up block above; page-actions slots only, grid slots
  deliberately deferred).
- `slot="..."` attribute sugar as a filler shorthand (coexists with the explicit
  elements) — only if real usage asks for it.
- Whether `StellarAdmin.Shell` composite screens should adopt the base.
- ~~Whether `StellarAdminTemplatedTagHelperBase.GetViewModel`'s implicit default (the
  calling view's model) should go~~ — **decided and done 2026-08-13 (OSS `2e085c6`)**:
  Jerrie does not want the implicit model; `GetViewModel()` is now abstract, so every
  subclass states its model explicitly (all four existing subclasses already
  overrode it — no other changes). OSS `CLAUDE.md` bullet updated to match.
