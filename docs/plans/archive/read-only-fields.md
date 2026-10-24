# Read-only and disabled states across the UI library

Status: completed (2026-10-24). Stage 1 in 2a1ca97; stage 2 in 3de3ed3, with the `Disabled` predicate dropped in 5d49d4c. The known gaps below are accepted until someone runs into them.

Every UI library editor follows the web platform's read-only and disabled behaviour, so the two states mean the same thing on every component. The admin panel then chooses deliberately between them (stage 2).

## Decision

Follow the platform's behaviour, as native controls do, and apply it to every editor:

- **Read-only** looks like the normal control at rest, at full contrast. It loses only the affordances for changing the value, as Chromium drops a read-only date input's picker icon. The value can't change, the control stays focusable and the value is posted. Choice controls lose their hover, since nothing on them responds; controls whose text can be selected keep it.
- **Disabled** is faded, takes no focus and isn't posted.

The plan first set out to give read-only a recognisable look, because a read-only input shows nothing to say it can't be edited. Jerrie dropped that after seeing the platform's own behaviour: browsers style a read-only field like a normal one. Consistency with native controls is the goal.

## Findings

- **Platform (Chromium, unstyled controls, 2026-10-24).**
  - Disabled controls get grey text, a faint grey fill and a faded border; a select also gets 70% opacity.
  - Read-only text-type inputs and textareas look identical to normal ones. The one difference: date, time and datetime-local inputs lose their picker icon.
  - `readonly` is ignored on select, checkbox, radio, range, colour and file; the spec says there's "no useful distinction" for those.
  - Read-only is focusable and posted. Disabled, including inside `fieldset[disabled]`, is neither.
  - Form-associated custom elements get native `disabled` handling (`formDisabledCallback`, left out of the form data). `readonly` on them only skips validation.
- **ARIA and WCAG.**
  - `aria-readonly` is supported on checkbox, combobox, listbox, radiogroup, slider, spinbutton, switch and textbox.
  - WCAG's contrast exemption covers disabled controls, not read-only ones, so read-only text keeps full contrast.
- **Other libraries.**
  - Most pass `readonly` through with no styling: MUI, Material 3, Fluent, Atlassian, Ant, Mantine, Chakra, shadcn and USWDS. Filament and Retool do the same in admin panels.
  - Carbon, Spectrum and Base UI give choice controls a read-only state through `aria-readonly`.
  - Django admin, Odoo and Salesforce render read-only fields as text.
  - Admin frameworks treat read-only in the UI separately from server enforcement. Django leaves read-only fields out of the form, Filament has `saved(false)`, Payload has field access.
- **Today's components.** Shown on ComponentPlayground's Editor States page (`sandbox/ComponentPlayground/Pages/Demo/EditorStates.cshtml`), which reports what each cell posts and its tab stops.
  - Only text inputs, the rich text editor and the list and dictionary editors act on `readonly`. On every other editor it does nothing: the value can be changed and is posted.
  - The rich text, list and dictionary editors ignore `disabled`: they stay editable and post.
  - Disabled sliders and image uploads still post.
  - Read-only list and dictionary editors post nothing and render text on a disabled fill.

## Stage 1: UI library

The prototype is `sandbox/html/read-only-fields.html`, the Editor States markup with the platform's behaviour applied. Its candidate CSS is written as the library would write it. Its script stands in for the components' behaviour (blocking clicks and keys) and is not for extraction.

Tag helpers. `readonly` and `disabled` are the standard attribute names, and the existing helpers use them. Properties bound to a tag helper are `bool?` and take a value (`readonly="true"`); attributes that pass through to the HTML element, as on `sa-input`, `sa-select` and `sa-switch`, keep working bare.

- Add `readonly` to `sa-select`, `sa-switch`, `sa-toggle`, `sa-toggle-group`, `sa-segmented-control`, `sa-checkbox-group`, `sa-radio-group`, `sa-slider`, `sa-icon-picker`, `sa-image-upload` and `sa-input-otp`. Each also writes `aria-readonly` where the role supports it.
- Add `disabled` to `sa-rich-text-editor`, `sa-list-editor` and `sa-dictionary-editor`.
- `sa-input` checkboxes and radios already pass `readonly` through; they need only the client behaviour.

Client behaviour, in JS and CSS:

- **Choice controls** (checkbox, radio, their groups and choice cards, switch, toggle, toggle group, segmented control, inline icon picker): a read-only one doesn't change on click, on Space or on the arrow keys. It has no hover, stays focusable and posts.
- **Select:** read-only hides the chevron. It takes focus but doesn't open or change, and posts.
- **Slider:** read-only can't be dragged or moved with keys. It stays focusable and posts.
- **Icon picker:** read-only hides the chevron and doesn't open. It stays focusable and posts.
- **Image upload:**
  - Read-only shows the image alone, without Replace, Remove, the drop area or the accepted-types hint, and refuses dropped files. It posts.
  - Disabled no longer posts.
- **Input OTP:** read-only also marks the inner input `readonly`.
- **Rich text editor:**
  - Read-only keeps the toolbar hidden, and its content becomes focusable.
  - Disabled fades like a disabled textarea, disables the toolbar, takes no focus and doesn't post.
- **List and dictionary editors:**
  - Read-only renders read-only inputs with no actions column, add row or drag handle. It is focusable and posts.
  - Disabled fades and disables the inputs and buttons, including the add button that the editor's script re-enables today. It takes no focus and doesn't post; a disabled `fieldset` around it should work too.
- **Slider (disabled):** no longer posts.

Behaviour changes for existing users:

- Read-only list and dictionary editors render inputs and post their values.
- Disabled sliders and image uploads stop posting.

Then, as usual for UI library changes:

- DocsSamples read-only and disabled demos.
- Website docs.
- The consumer skill references.
- Tests.
- An updated Editor States page.

### Stage 1 record

Built as planned, with these decisions:

- `sa-select`, `sa-switch`, `sa-toggle` and `sa-input` pass `readonly` through to the native element, so bare `readonly` keeps working. A shared field helper adds `aria-readonly="true"` to a read-only select, switch, toggle or checkbox; a lone radio has no role that takes it.
- The other components bind `readonly` (and `disabled` on the rich text, list and dictionary editors) as `bool?`. When both are set, disabled wins. Radio groups and segmented controls put `aria-readonly` on the radiogroup; checkbox groups and multiple toggle groups put it on each checkbox.
- Browsers ignore `readonly` on checkboxes, radios and selects, so the tag helpers render inline handlers on a read-only one (internal `ReadOnlyHandlers`): `onclick="return false"` on a checkbox or radio, which also covers its label, Space and a radio group's arrow keys, and on a select `onmousedown="this.focus(); return false"` plus an `onkeydown` that ignores the keys that open or change it while Tab and Ctrl/Cmd shortcuts still work. Every choice control rendered by a tag helper is covered; hand-written markup isn't, and a strict Content Security Policy must allow inline handlers. A first version used a page-wide `read-only.js`; Jerrie preferred the handlers in the markup. CSS drops their hover and pointer cursor, and hides a read-only select's icon.
- The slider and icon picker mark read-only with `data-readonly`, as they mark disabled. The slider ignores the pointer and keys. A read-only icon picker renders no chevron, popover or icon template, and its script does nothing.
- A read-only image upload renders the image alone (the empty image when there is none) with a disabled file input, needs no `upload-url`, and refuses drops. Its lightbox still works.
- A disabled rich text editor (`data-disabled`) renders its toolbar, which the script disables, and a disabled textarea; its content isn't editable or focusable. A read-only editor's content gets `tabindex="0"`.
- A disabled list or dictionary editor sets `disabled` on the form-associated `sel-collection-editor`, so it matches `:disabled`; its controls and marker are disabled, but not the template row. `formDisabledCallback` keeps the controls in step with the element and with a disabled fieldset. A read-only editor renders read-only inputs and selects and the marker, with no add row, template or remove buttons. `.sa-collection-editor-text` is gone.
- DocsSamples has Read Only demos for select, checkbox, radio, switch, toggle, toggle group, segmented control, checkbox group, radio group, slider, icon picker, image upload and input OTP, and Disabled demos for the rich text, list and dictionary editors. All are in the generator's list and exported to the website, whose 16 component pages document them. The consumer references were regenerated, with a "Read-only and disabled" section in `forms.md`.

Verified 2026-10-24:

- `npm run build`, `npm run check` and oxfmt in `src/StellarAdmin.UI/Client`; the solution build (Release).
- All five test suites, 1,982 tests, after the library change; the UI unit suite (794) again after the new tests. The two list and dictionary read-only tests were rewritten for the new behaviour; new tests cover each component's read-only and disabled rendering.
- The Editor States page in headless Chromium with real mouse and key input: the read-only checkbox (click, label, Space), switch, toggle, radio, radio group (click, label, arrow), segmented control, toggle group, choice card and inline icon picker don't change; the read-only select keeps its value under clicks, ↓ and End, takes focus and hides its icon; the read-only slider stays put under ArrowRight and a track click while the normal one moves; the read-only picker doesn't open; OTP typing doesn't change the code. Every cell posts and takes tab stops as the platform does, except the read-only image upload, which has no tab stop. No console errors. Screenshots in light and dark match the prototype.
- The 16 new demos render through DocsStatic; the SkillsGenerator check and the skill bundle validation pass; the website's `pnpm lint`, `pnpm types:check` and `pnpm build` pass.

After the switch to inline handlers (2026-10-24): all five test suites (1,982), the website export, lint, type check and build, the same Editor States interaction run with the same results, and real clicks on the normal checkbox, radio, switch, checkbox group and segmented control, which still change. In a bare page without scripts, Chromium 151 lets a click and Space change a `readonly` checkbox and a click change a `readonly` radio; `onclick="return false"` stops both, and the arrow keys then move focus in a radio group without changing the selection.

Not checked: Firefox, Safari, touch (whether a mouse down handler keeps a mobile select closed), the theme fixtures, phone widths and screen readers.

## Stage 2: admin panel

The admin panel supports both states, with distinct roles, and every editor template renders read-only as `readonly` and disabled as `disabled`.

### Where it starts (2026-10-24 catalogue)

- A template's read-only flag is `FieldEditorView.IsReadOnly`: the field's `FormFieldOptions.IsReadOnly`, which nothing sets (no builder method), or, for a field without a binder, the property's metadata (`[ReadOnly(true)]`, `[Editable(false)]` or no public setter). MVC's tag helpers don't read that metadata; model binding uses it to skip read-only properties.
- The admin panel has no disabled setting. Disabled appears only as the waiting state of a dependent field, and as the way ten templates render read-only.
- Already `readonly`: text, date, date-time and time inputs, textarea, rich text, list and dictionary editors. Rendered `disabled` for read-only: select, checkbox, switch, checkbox group, radio group (radios composed in the template), toggle group (per item), segmented control, slider, OTP and icon picker. The image upload sets `disabled="!canUpload"`, which since stage 1 also fades it and stops its value posting, including outside a resource form. The lookup and multi-lookup sheets render static text when read-only.
- Grids edit only text, date, time, select, checkbox, switch, segmented control, icon, image and lookup fields; list and dictionary cells show badges, and the other editors have no grid template. The grid leaves read-only fields out of its editing rows.
- Edit binds the posted values onto the loaded record, limited to the form's field names; create binds onto a new model. Validation errors are scoped to the bound fields.

### Decisions (Jerrie, 2026-10-24)

- **Read-only**: the value matters, but can't be changed here. It looks normal, takes focus and posts. Sources: the property's metadata, and `.ReadOnly()` or `.ReadOnly(predicate)` on the field.
- **Disabled**: the field doesn't apply right now. It fades, takes no focus and doesn't post. Sources: `.Disabled()` on the field, and the dependent-field waiting state, which keeps its current behaviour.
- Disabled wins when both apply. The server ignores posted values for both: binding leaves the field out, decided against the record as loaded (edit) or the new model (create), never against posted values.
- A read-only predicate takes the record and comes now. A re-rendered form evaluates it against the model it renders.
- `Disabled` takes no predicate (dropped after stage 2 was committed). Disabled means "doesn't apply right now", which usually follows what the user is changing on the form; a server-side check against the stored record would show the state as of the last save. The live version is the later work below, and its API may differ.
- Reacting live to other fields' values in the browser is later work, likely on the dependent-fields mechanism.
- Grids keep showing a read-only field's display value in place of an editor, now per row when a predicate decides it; a disabled field renders its editor disabled.

### Public API

```csharp
// ResourceFieldBuilder<TResource>
public ResourceFieldBuilder<TResource> ReadOnly();
public ResourceFieldBuilder<TResource> ReadOnly(Func<TResource, bool> predicate);
public ResourceFieldBuilder<TResource> Disabled();

// FormFieldOptions: IsReadOnly replaces the IsReadOnly property, which is unshipped
public bool IsReadOnly(object model);
public bool IsDisabled { get; }

// FormFieldProperties and FieldEditorView<TEditor>, for custom templates
public bool IsDisabled { get; }
```

The configuration checks that need editable fields (add another, dependencies, inline rows, image stores) treat only a disabled field, or one that is always read-only, as not editable; a field a read-only predicate decides is editable for them.

### Work

- Builder methods, options and per-record evaluation; binding leaves read-only and disabled fields out of the bound field names.
- Templates, form and grid: the ten `disabled` editors switch to `readonly`; every editor gains disabled. The radio group and toggle group set the group's read-only state, not just each item's. The lookups keep their static read-only rendering and render a disabled trigger, with no posted value, when disabled. The image upload separates being unable to upload from read-only.
- Grid rows decide read-only per row.
- Tests, AdminPlayground galleries, the website's field docs and the consumer skill references.

### Stage 2 record

Built as agreed, with these decisions made while building:

- `FieldStates` (internal) decides a field's state for a record, adding the property's metadata to the field's configuration, and the controller binds only the fields that are neither read-only nor disabled: edit and inline edit against the loaded record, create and kept add-another values against the new model. Validation errors of the fields left out are dropped, as for any unbound property, and handler errors that name them go to the summary.
- `FieldEditorView.IsReadOnly` is false when the field is disabled, so templates pass `readonly="@IsReadOnly" disabled="@IsDisabled"` and precedence lives in one place.
- The radio group template gains `role="radiogroup"`, labelled by its legend, which carries `aria-readonly`.
- The image upload is read-only when it can't upload (read-only, or outside a resource form) and disabled when the field is.
- The lookups wrap their editable layout in `<fieldset class="contents" role="none" disabled>` when disabled, which disables the buttons and the hidden values. The dependent-field script now skips controls that match `:disabled`, which includes those in a disabled fieldset.
- The grid decides editors from the editing row's model up front (`EditModel` or `NewItem`), since a column's edit template is registered before any row renders; `GetColumnFields` no longer filters read-only fields.
- The configuration checks (add another, dependencies, inline rows, image stores) and the upload and lookup endpoints treat only a field that is always read-only or disabled as not editable.

Verification (2026-10-24):

- Tests: `tests/StellarAdmin.IntegrationTests/Resources/FieldStateTests.cs` covers binding (read-only, disabled, a predicate against the stored record, create, a read-only list from `.ReadOnly()` and from `[Editable(false)]`), rendering for eleven editors in both states, the radio group's state, precedence, a disabled lookup, and inline rows. With the metadata check removed, the `[Editable(false)]` list test fails: MVC binds the posted rows into a read-only collection property. Suites: StellarAdmin.Tests 333, StellarAdmin.IntegrationTests 717, StellarAdmin.EntityFrameworkCore.IntegrationTests 154, all passing.
- Headless Chromium on the AdminPlayground galleries (every gallery has a `Disabled()` example; the new "Read-only and disabled" gallery shows a read-only predicate): every read-only example renders `readonly` at full contrast, focusable and posting; every disabled one is faded (0.5), unfocusable and not posting; no console errors. Light and dark screenshots of the new gallery, and of the lookup and multi-lookup galleries.
- Not checked: Firefox, Safari, screen readers, phone widths, an inline-row grid in the browser.

Fixed after the stage 2 browser check: a disabled bound checkbox, switch or toggle posted `false` through the framework's hidden companion input, which it renders after the checkbox or at the end of the form, so a plain MVC form would clear a checked value. `CheckboxHiddenInput` (internal, UI library) leaves that input out when the checkbox is disabled. Tests in the input, switch and toggle tag helper tests fail without it; UI library tests 799 and admin integration tests 717 pass, and the AdminPlayground's disabled checkbox and switch no longer post.

Known gaps:

- A dependent field refreshed in the browser is rendered against the partial model the refresh binds, so a predicate reading other properties can decide differently there than on the page.
- A read-only multi-lookup posts nothing, unlike the other read-only editors; the server ignores it either way.

## Prototype verification

- Checked in headless Chromium, light and dark, on 2026-10-24:
  - the Editor States page;
  - the prototype, with scripted clicks and key presses. The read-only choice controls, select, slider and icon picker don't change, and they still post and keep their tab stops.
- Not yet checked:
  - Firefox and Safari, whose behaviour is taken from the research rather than tested;
  - the theme fixtures;
  - phone widths;
  - screen readers.
