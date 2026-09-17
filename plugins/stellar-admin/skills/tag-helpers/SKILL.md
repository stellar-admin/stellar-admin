---
name: tag-helpers
description: >-
  Builds ASP.NET Core MVC / Razor Pages UIs with StellarAdmin Tag Helpers (the sa-*
  elements from the StellarAdmin.TagHelpers package). Covers project setup, the component
  catalog (tags, attributes, enum values, examples), and the conventions that make
  StellarAdmin markup correct — attribute pass-through, fully-qualified enums in Razor,
  the Invoker Commands API for overlays, model binding and routing attributes. Use when
  editing .cshtml or .razor files that use StellarAdmin, when scaffolding a new
  StellarAdmin page or form, or when the user mentions StellarAdmin, StellarAdmin Tag
  Helpers, or any sa-* component.
metadata:
  author: StellarAdmin
---

# Building UIs with StellarAdmin Tag Helpers

StellarAdmin Tag Helpers is a library of **ASP.NET Core Tag Helpers** that mirror [shadcn/ui](https://ui.shadcn.com/), for building MVC / Razor Pages UIs. It ships as the `StellarAdmin.TagHelpers` NuGet package. Every component is a server-rendered `<sa-*>` element. Interactivity that HTML and CSS can't do alone comes from small bundled web components (`<sel-*>`), which you never write by hand — the tag helpers emit them.

Use this skill whenever you write or edit Razor markup that uses StellarAdmin.

## How to use this skill

The detail lives in `references/`, loaded on demand — open only what the task needs:

- **`references/setup.md`** — is StellarAdmin installed and wired up? Read this when a project is new to StellarAdmin, or when components render as plain unstyled HTML (missing CSS/JS, or a conflicting CSS framework).
- **`references/conventions.md`** — the rules that make StellarAdmin markup *correct*. **Read this before writing any non-trivial StellarAdmin markup**; the mistakes it prevents (bare enum strings, broken overlays, classes that silently do nothing) are the common ones.
- **`references/components-index.md`** — one-line-per-component table (tags + summary). Scan this to find the right component.
- **`references/components/<name>.md`** — per-component reference: exact tag names, every attribute with its type / default / allowed values, and working examples taken from the StellarAdmin docs samples. Open the specific component(s) you're using.
- **`references/icons.md`** — `<sa-icon>` names, and registering custom icons or an icon pack.
- **`references/javascript.md`** — the `stellar-admin.js` bundle: which components need it, Invoker Commands, and the promise-based `window.stellarAdmin.dialog()` / `alertDialog()` helpers.
- **`references/templated-views.md`** — writing your own tag helpers that render through a Razor view, and passing content into named slots with `<sa-slot-content>` / `<sa-slot-outlet>`.

For focused tasks there are companion skills: **[forms](../forms/SKILL.md)** (fields, validation, model binding), **[layout](../layout/SKILL.md)** (app shell, page structure, cards), and **[theming](../theming/SKILL.md)** (themes, dark mode, design tokens).

## The six things to get right (details in conventions.md)

1. **Enums are written fully-qualified in Razor**, not as bare strings: `variant="ButtonVariant.Outline"`, not `variant="outline"`.
2. **Unlisted attributes pass straight through.** Anything not in a component's attribute table — `id`, `style`, `data-*`, `aria-*`, `hx-*`, and the native attributes of the element it renders — is forwarded to the rendered element verbatim.
3. **`class` is appended, not replaced.** Your classes join the component's own. Whether an arbitrary utility like `bg-primary` *exists* depends on the app having its own Tailwind build — see conventions.md §3 before reaching for one.
4. **Overlays open declaratively — never with `onclick`** — but by two different mechanisms. Modals (`<sa-dialog>`, `<sa-alert-dialog>`, `<sa-sheet>`) use the Invoker Commands API: a trigger **button** carries `commandfor="<id>"` and `command="show-modal"` / `close`. Popovers (`<sa-popover>`, dropdown menu content) use the Popover API instead: the trigger carries `popovertarget="<id>"`, with no `command`. Don't mix them.
5. **Kebab-case attributes.** A C# property `ShowCloseButton` binds to `show-close-button`.
6. **Composite components have a required tag hierarchy** (sidebar, accordion, field, dropdown menu). Follow the nesting shown in the component's reference — children must sit inside their parent, in order.

## Never hand-author `<sel-*>` elements or runtime `data-*` state

Those are emitted by the tag helpers and driven by the bundled script. Your job is the `<sa-*>` markup only.

## Online documentation

<https://www.stellaradmin.com/docs/tag-helpers>
