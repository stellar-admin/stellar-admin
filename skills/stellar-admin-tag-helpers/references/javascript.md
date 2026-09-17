# JavaScript and interactivity

StellarAdmin leans on the web platform wherever it can, so most components are pure server-rendered HTML and CSS. Where that isn't enough, a single bundled script adds behavior.

## The `stellar-admin.js` bundle

```razor
<script defer src="/_content/StellarAdmin.TagHelpers/stellar-admin.js" asp-append-version="true"></script>
```

It contains three things:

- the **`<sel-*>` web components** that some tag helpers render,
- the **`interestfor` polyfill**, so hover-triggered tooltips and popovers work in browsers that don't implement it natively,
- the **`window.stellarAdmin`** object with the `dialog()` and `alertDialog()` helpers.

## Web components

A handful of tag helpers render a custom element around or in place of their markup. You never author these — the tag helper emits them.

| Tag Helper | Element | What the script adds |
|------------|---------|----------------------|
| `<sa-collapsible>` | `<sel-collapsible>` | Handles the `--toggle`, `--show` and `--hide` commands; keeps `aria-expanded` in sync on the trigger buttons. |
| `<sa-dialog>`, `<sa-alert-dialog>`, `<sa-sheet>` | `<sel-dialog>` | Locks page scrolling while a modal is open; mirrors open state in `data-open`. |
| `<sa-dropdown-menu-content>`, `<sa-dropdown-menu-sub-content>` | `<sel-dropdown-menu>` | Menu semantics over a native popover: roving arrow-key focus, Home/End, type-ahead, Enter/Space activation, close-on-select, checkbox and radio items, sub-menu open/close. |
| `<sa-input-otp>` | `<sel-input-otp>` | Keeps the visual slot cells in sync with the backing `<input>`, shows the active cell and caret, enforces the pattern. |
| `<sa-questionnaire>` | `<sel-questionnaire>` | Shortcut keys for the choices; arrow keys that move between a question's answers and wrap at either end; a free-text answer and the choices standing in for each other, keeping only one of the two in the form. |
| `<sa-sidebar-wrapper>` | `<sel-sidebar>` | Tracks expanded/collapsed on desktop and the drawer state on mobile; handles `--toggle-sidebar`, `--open-mobile`, `--close-mobile`. |
| `<sa-slider>` | `<sel-slider>` | Pointer and keyboard interaction for the thumbs; keeps the range fill, `aria-valuenow` and hidden form inputs in sync. |
| `<sa-table-selection>` | `<sel-table-selection>` | Select-all / indeterminate handling and the `data-state="selected"` row highlight. |

All of these render in the **light DOM** — there is no shadow root, so the server-rendered markup stays visible to CSS, to Tailwind utilities and to your own scripts, and the elements participate in layout like any other.

Where a component can degrade gracefully it does: a `<sa-table-selection>` table still posts its checked rows without the script, and a `<sa-dialog>` still opens from a plain `command="show-modal"` button.

Custom elements are upgraded when the script runs, which with `defer` is after the document is parsed. If your own code needs to touch one — reading `selectedValues` off a `<sel-table-selection>`, say — wait for it first:

```js
await customElements.whenDefined("sel-table-selection");
```

## Invoker Commands

Opening and closing overlays uses the native [Invoker Commands API](https://developer.mozilla.org/en-US/docs/Web/API/Invoker_commands_API), so no JavaScript is required:

```razor
<sa-button commandfor="booking-dialog" command="show-modal">Edit booking</sa-button>

<sa-dialog id="booking-dialog">
    ...
    <sa-button commandfor="booking-dialog" command="close">Cancel</sa-button>
</sa-dialog>
```

Built-in commands drive `<sa-dialog>`, `<sa-alert-dialog>` and `<sa-sheet>`: use `command="show-modal"` to open and `command="close"` to close. Custom commands (`--toggle`, `--show`, `--hide`) drive `<sa-collapsible>`.

**Popovers are a different mechanism.** `<sa-popover>` and the dropdown menu's content render a native popover, so their triggers use `popovertarget="<id>"` rather than `commandfor` / `command` — see conventions.md §7.

Two constraints:

- **The invoking element must be a `<button>`** — `<sa-button>` renders one. `commandfor` and `command` on anything else do nothing.
- Invoker Commands are a recent platform feature and **StellarAdmin does not bundle a polyfill for it**. Check browser support against the audience before relying on it.

## Interest invokers

Tooltips and hover-triggered popovers use [interest invokers](https://developer.mozilla.org/en-US/docs/Web/API/Popover_API/Using_interest_invokers): an `interestfor` attribute on the trigger shows the target popover on hover, focus and long-press. The `interestfor` polyfill **is** bundled, so this works regardless of native support.

## `window.stellarAdmin.dialog(selector, options?)`

Wraps an existing `<dialog>` (such as the one `<sa-dialog>` or `<sa-sheet>` renders) so you can drive it in async/await style. Throws if the element can't be found or isn't an `HTMLDialogElement`.

```js
const result = await window.stellarAdmin.dialog("#booking-dialog").showAsync();
if (result.confirmed) {
    console.log(result.data);   // the dialog's form values
}
```

**Arguments**

| Argument | Type | Description |
|----------|------|-------------|
| `selector` | `string \| HTMLDialogElement` | A CSS selector or the element itself. |
| `options` | `{ clearReturnValueOnOpen?: boolean }` | `clearReturnValueOnOpen` (default `true`) resets `returnValue` each time `showAsync()` opens the dialog. |

**Wrapper methods**

| Method | Description |
|--------|-------------|
| `showAsync(): Promise<Result>` | Opens the dialog as a modal; resolves when it closes. |
| `confirm(data?): void` | Closes the open dialog and resolves with `confirmed: true`. With `data` omitted, the dialog's form data is serialized instead. Throws if no dialog is open. |
| `cancel(): void` | Closes the open dialog and resolves with `confirmed: false`. Throws if no dialog is open. |

**Result object**

| Property | Type | Description |
|----------|------|-------------|
| `confirmed` | `boolean` | `false` when light-dismissed, dismissed with `Esc`, closed with no `returnValue` (e.g. a `command="close"` button or a sheet's close button), or when `returnValue` is `"cancel"`. `true` otherwise. |
| `returnValue` | `string` | The dialog's `returnValue` when it closed. |
| `data` | `object \| null` | The dialog's form values via `FormData` when confirmed; `null` when cancelled or when there is no form. |

Set `returnValue` from markup with a `<form method="dialog">` whose submit buttons carry a `value` — a `command="close"` button does **not** set it.

## `window.stellarAdmin.alertDialog(selector)`

A confirmation-focused wrapper over `dialog()`, for `<sa-alert-dialog>`. `selector` takes the same `string | HTMLDialogElement` as `dialog()`. Because an alert dialog exists to await a yes/no, it resolves to a plain boolean.

```js
const alertDialog = window.stellarAdmin.alertDialog("#delete-confirm");
if (await alertDialog.confirmAsync()) {
    // perform the confirmed action
}
```

| Method | Description |
|--------|-------------|
| `confirmAsync(): Promise<boolean>` | Opens as a modal; resolves `true` when confirmed, `false` when cancelled or dismissed with `Esc`. |
| `confirm(data?): void` | Closes and resolves `true`. Throws if no dialog is open. |
| `cancel(): void` | Closes and resolves `false`. Throws if no dialog is open. |

`<sa-alert-dialog-action>` confirms; `<sa-alert-dialog-cancel>` and `Esc` cancel.

If the page already defines `window.stellarAdmin`, the bundle logs a console warning and replaces it.
