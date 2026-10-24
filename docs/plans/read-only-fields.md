# Read-only and disabled states across the UI library

Status: approved scope for stage 1, not started (2026-10-24).

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

## Stage 2: admin panel

After stage 1. The admin panel uses `disabled` today for every editor without a read-only state: select, checkbox, checkbox group, radio group, switch, toggle group, segmented control, slider, OTP and icon picker. The image upload drops its upload URL instead.

Stage 2 decides when a field is read-only and when it is disabled, since the two states play different roles, and moves the editor templates over.

Before relying on read-only values being posted, confirm that saving still ignores read-only fields; posted values must never be trusted.

## Verification so far

- Checked in headless Chromium, light and dark, on 2026-10-24:
  - the Editor States page;
  - the prototype, with scripted clicks and key presses. The read-only choice controls, select, slider and icon picker don't change, and they still post and keep their tab stops.
- Not yet checked:
  - Firefox and Safari, whose behaviour is taken from the research rather than tested;
  - the theme fixtures;
  - phone widths;
  - screen readers.
