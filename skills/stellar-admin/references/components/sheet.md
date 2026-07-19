---
component: Sheet
tags: [sa-sheet, sa-sheet-description, sa-sheet-footer, sa-sheet-header, sa-sheet-title]
generated: true
---

# Sheet

A panel that slides in from an edge of the screen, rendered over a native `<dialog>` element. Open and close it with the Invoker Commands API — a trigger button carrying `commandfor` and `command="show-modal"` or `command="close"`.

<!-- structure:begin -->

## Required structure

The trigger button lives **outside** `<sa-sheet>` and points at it via
`commandfor`. The sheet's own children are shallow — header (title +
description), body content, and footer.

```
(a trigger <sa-button commandfor="…" command="show-modal">)   ← outside the sheet
sa-sheet
├── sa-sheet-header
│   ├── sa-sheet-title
│   └── sa-sheet-description
├── ... body content (e.g. sa-field-group)
└── sa-sheet-footer
    └── ... action buttons (sa-button commandfor="…" command="close")
```

<!-- structure:end -->

## Tags

| Tag | Description |
|-----|-------------|
| `<sa-sheet>` | A panel that slides in from an edge of the screen, rendered over a native `<dialog>` element. Open and close it with the Invoker Commands API — a trigger button carrying `commandfor` and `command="show-modal"` or `command="close"`. |
| `<sa-sheet-description>` | Supporting description text for a sheet, shown beneath the title. |
| `<sa-sheet-footer>` | The footer region of a sheet; typically contains action buttons. |
| `<sa-sheet-header>` | The header region of a sheet; typically contains the title and description. |
| `<sa-sheet-title>` | The accessible title of a sheet, rendered as a heading in the sheet header. |

## Attributes

### `<sa-sheet>`

| Attribute | Type | Default | Values |
|-----------|------|---------|--------|
| `show-close-button` | `bool` | `true` | `true`, `false` |
| `side` | `SheetSide` | `Right` | `Top`, `Right`, `Bottom`, `Left` |
| `class` | `string` | — | Extra Tailwind utilities; merged last, so it overrides defaults. |

> In Razor, enum values are written fully-qualified, e.g. `variant="ButtonVariant.Outline"`.

## Examples

*From `Pages/Sheet/_Intro.cshtml`*

```razor
<div class="flex justify-center">
    <sa-button variant="ButtonVariant.Outline" commandfor="--sheet-intro" command="show-modal">
        Open
    </sa-button>
</div>
<sa-sheet id="--sheet-intro">
    <sa-sheet-header>
        <sa-sheet-title>Edit profile</sa-sheet-title>
        <sa-sheet-description>Make changes to your profile here. Click save when you're done.
        </sa-sheet-description>
    </sa-sheet-header>
    <sa-field-group class="grid gap-6 px-4">
        <sa-field>
            <sa-label for="--sheet-intro-name">Name</sa-label>
            <sa-input id="--sheet-intro-name" name="name" defaultValue="Ibn Battuta"/>
        </sa-field>
        <sa-field>
            <sa-label for="--sheet-intro-username">Username</sa-label>
            <sa-input id="--sheet-intro-username" name="username" defaultValue="@@ibnbattuta"/>
        </sa-field>
    </sa-field-group>
    <sa-sheet-footer>
        <sa-button variant="ButtonVariant.Outline" commandfor="--sheet-intro" command="close">
            Cancel
        </sa-button>
        <sa-button commandfor="--sheet-intro" command="close">
            Save Changes
        </sa-button>
    </sa-sheet-footer>
</sa-sheet>
```

*From `Pages/Sheet/_Sides.cshtml`*

```razor
<div class="flex justify-center gap-2">
    <sa-button variant="ButtonVariant.Outline" commandfor="--sheet-sides-top" command="show-modal">
        Top
    </sa-button>
    <sa-button variant="ButtonVariant.Outline" commandfor="--sheet-sides-right" command="show-modal">
        Right
    </sa-button>
    <sa-button variant="ButtonVariant.Outline" commandfor="--sheet-sides-bottom" command="show-modal">
        Bottom
    </sa-button>
    <sa-button variant="ButtonVariant.Outline" commandfor="--sheet-sides-left" command="show-modal">
        Left
    </sa-button>
</div>
<sa-sheet id="--sheet-sides-top" side="SheetSide.Top" class="data-[side=top]:h-[50vh]">
    <div class="p-4">
        Open on the top
    </div>
</sa-sheet>
<sa-sheet id="--sheet-sides-right" side="SheetSide.Right">
    <div class="p-4">
        Open on the right
    </div>
</sa-sheet>
<sa-sheet id="--sheet-sides-bottom" side="SheetSide.Bottom" class="data-[side=bottom]:h-[50vh]">
    <div class="p-4">
        Open on the bottom
    </div>
</sa-sheet>
<sa-sheet id="--sheet-sides-left" side="SheetSide.Left">
    <div class="p-4">
        Open on the left
    </div>
</sa-sheet>
```
