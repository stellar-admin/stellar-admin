# Creating items from a lookup

Status: **implemented, awaiting review** on branch `lookup-create` (uncommitted). Last updated: 2026-10-06.

`LookupEditor.EnableCreate()` rendered a "New" button beside "Choose {field}" while the field was empty, but the button did nothing. It now opens the referenced resource's create form in the shared dashboard sheet, and a successful save selects the new item in the editor. This picks up the [Deferred](archive/lookup-editor.md#deferred) item of the lookup editor plan. Jerrie asked for the work to be done end to end on a branch and reviewed at the end, so the phases below were implemented without checkpoints; the open decisions from the proposal were settled as judgment calls, listed under [Decisions](#decisions).

## Flow

1. **Opening the form:** New is an htmx opener like Choose: `commandfor="dashboard-sheet" command="show-modal" hx-get="{target}/createsheet?for={editor id}"`. The request goes to the **referenced** resource's controller, so its authorization applies.
2. **Saving:** `_CreateSheet` renders the target's create form in the sheet with the title, fields, submit label and Cancel. The form posts with htmx into the sheet. A rejected post comes back in the sheet with its errors, focused on the first invalid field.
3. **Returning the key:** a successful post returns `<dashboard-lookup-created for="…" value="{key}">`. The element closes the sheet and calls `selectCreated(key)` on the editor.
4. **Showing the item:** the editor fetches `{origin}/lookupselection?form=&field=&value=` from the **originating** resource. That action calls the field's `LookupItems.FindAsync` and returns the `_LookupSelection` parts in a template, with the value the form posts. The editor then sends itself the existing `lookup-select` event, which shows the selection and focuses Change. If the fetch fails, the key stands in for the title, as it does for an unknown value, so a created record is never lost.

## What changed

**Library (`StellarAdmin.Dashboard`):**

- **Finding the resource:**
  - `LookupItems` gains `public virtual Type? ItemType` (null by default). The built-in `ILookupSource` items and the EF Core items return `TEntity`.
  - `LookupEditor.EnableCreate<TResource>()` names the resource explicitly.
  - `ResourceRegistration` records the resource type and whether a create form is configured.
- **Showing New:** `LookupEditorHandler` resolves the target resource while preparing the field, and returns a new internal `LookupEditorData(Item, CreateController)` instead of the bare `LookupItem`.
  - It throws for items with no type and no explicit resource, for a type with no registered resource, or for a resource without a create form.
  - It omits the button when the user fails the resource's authorization, using the same check as the sidebar. `AddDashboard` now registers `IHttpContextAccessor` for this.
- **Controller (`ResourceController<TResource>`):**
  - New actions: `CreateSheet` (GET and POST, `?for=`) and `LookupSelection`.
  - `CreatePost` and `CreateSheetPost` share a new `SubmitCreateAsync`.
  - The binding prefix is now a parameter of `TryBindConfiguredFieldsAsync`, `ConfiguredFieldValueProvider` and `AddValidationErrors`. The sheet form binds with the prefix `Sheet`, so its IDs and names don't collide with the page form's `Entity` ones.
- **Returning the key:**
  - `ResourceOperationResult` gains `Success(string key)` and `Key`.
  - The key comes from `result.Key`, or else from the resource's `KeySelector` when the created model is the resource type.
- **Views:**
  - `_Layout` renders two sheets through a new `_RemoteSheet` partial: `dashboard-sheet`, and `dashboard-nested-sheet`, which a create form's lookups open.
  - New partials: `_CreateSheet`, `_LookupCreated` and `_LookupSelectionTemplate`.
  - The internal `ViewDataKeys.InCreateSheet` flag tells the lookup template to target the nested sheet, use the `create` form name and hide New.
  - `labels.Sheet(...)` gains `SaveErrorDescription` ("Your changes could not be saved.").
- **Script:**
  - `dashboard-remote-sheet` finds its content through `data-sheet="content"` instead of a fixed ID. Closing a sheet also aborts requests from its content, such as a pending form post.
  - New elements: `dashboard-create-sheet` (focus, and an error for a failed post that keeps what was typed) and `dashboard-lookup-created`.
  - `dashboard-lookup-editor` gains `selectCreated(key)`.

**Playground:**

- **Categories:** a new Category EF Core resource (index with search and paging, create, edit).
- **Products:** product create and edit both use a shared `UseCategories` lookup with `EnableCreate()`; product create previously used `SelectEditor`.
- **Gallery:** the airports become a `GalleryAirportStore` shared by the lookup source and a new in-memory "Airports" resource. Its create form uses a separate model, and its handler returns `Success(code)`, so the gallery's `EnableCreate` field works and shows the handler-key path.

**Consumer reference:** `skills/stellar-admin-dashboard/references/setup.md` covers `EnableCreate<TResource>()`, how New finds the resource, how the key is found (key selector or `Success(key)`), the errors it throws, authorization, nested lookups, `SaveErrorDescription`, and `Success(key)` in the write-operations paragraph.

## Decisions

1. **Finding the resource:** inferred from the items' type, with `EnableCreate<TResource>()` overriding it. `ItemType` is public and virtual, so custom `LookupItems` can opt in.
2. **Misconfiguration:** a missing resource or create form throws when the form renders. Authorization only hides the button.
3. **Value versus key:** the lookup's value must be the referenced resource's key. `LookupSelection` converts the key to the field's type with the invariant culture, and returns the value formatted in the current culture, as search results do. There is no second "find by key" path.
4. **Nested lookups:** implemented rather than deferred, because it was cheap once the sheet stopped hard-coding its content ID. One level only: lookups in the create sheet open `dashboard-nested-sheet` and have no New.
5. **New in the search sheet:** not done; still a follow-up.
6. **Create models other than the resource:** these have no key selector, so `ResourceOperationResult.Success(key)` was added. If a handler returns no key, the sheet closes and nothing is selected; the record exists and can be searched for.
7. **Sheet layout:** sections always stack in the sheet, whatever the resource's section layout, because the sheet is too narrow to split them. Cancel says "Cancel", hard-coded like the page form's Cancel.
8. **Selection model:** `LookupSelection` passes `FindAsync` a fresh form model: the create factory's model for create forms, and an uninitialized instance of the edit model for edit forms. EF Core items then query the projection by value.
9. **htmx settling:** the sheet form swaps with `settle:0`. Otherwise htmx copies the old inputs' attributes onto the new ones during settling, and the focused invalid field lost its posted value. This was found in the browser.

## Follow-ups and observations

- **Sheet padding:** the form's scroll area uses `px-6` like the lookup picker. Sheet header and footer padding varies by theme (p-4 to p-8), so in themes with p-4 the fields sit 8px inside the header and footer. A theme-level sheet body padding would fix both the picker and the form.
- **Nested sheet stacking:** the nested sheet covers the create sheet completely, since both are the same width on the same side.
- **Key-less handlers:** the editor isn't told when a created record has no key; a message could explain why nothing was selected.
- **Failed selection fetch:** when the fetch for the display fails, only the key is shown; the description and media come back when the form reloads.

## Verification

2026-10-06:

- **Tests:** solution-wide `dotnet test --solution StellarAdmin.slnx --configuration Release` passed, 737 tests.
  - Dashboard integration tests: 318. New tests are `LookupCreateTests` (22) and `DashboardSheetTests.Layout_RendersSharedAndNestedSheets`. The two existing `EnableCreate` tests now register a category resource, and the test `Category` fixture became a mutable class backed by a `CategoryStore`.
  - EF Core integration tests: 67, with the new `ResourceLookupTests.CreateFromLookup_SelectsCreatedEntity`. It creates through the edit page's New URL and checks the generated key and the selection display.
- **Formatting and references:** touched C# files formatted with CSharpier and the script with oxfmt. `SkillsGenerator --check` reports no drift.
- **Browser:** playground on port 5207 (Development) against a scratch copy of `app.db`, so the tracked database is unchanged. Headless Chromium over CDP, all checks passing, no console errors:
  - **Product create:** New opens "Add new Category" with focus in Name. An empty save keeps the sheet open with the error and focus on Name. "Garden tools" closes the sheet, shows the card with its avatar and description, sets the value and focuses Change. Search then finds it. Cancel leaves the field empty and returns focus to New. A post to a missing URL shows the sheet form's error and keeps the typed name. A product saved with a newly created category redirects to the index.
  - **Gallery:** creating OSL / Oslo / Norway shows the code chip, title and description. A duplicate code shows the handler's error, focuses Code and keeps the typed values.
  - **Nested:** a products create sheet's Category opens the picker in the nested sheet over it. Choosing closes only the nested sheet, selects the category in the create form, keeps the typed name and focuses Change.
  - **Screenshots reviewed:** the create sheet, the rejected form, the created card, the failed post, the created airport and the nested picker.
