---
component: Kbd
tags: [sa-kbd, sa-kbd-group]
generated: true
---

# Kbd

Displays a single keyboard key or keystroke.

## Tags

| Tag | Description |
|-----|-------------|
| `<sa-kbd>` | Displays a single keyboard key or keystroke. |
| `<sa-kbd-group>` | Groups several `<sa-kbd>` elements to represent a keyboard shortcut or key sequence. |

## Examples

*From `Pages/Kbd/_Intro.cshtml`*

```razor
<sa-kbd-group>
    <sa-kbd>⌘</sa-kbd>
    <sa-kbd>⇧</sa-kbd>
    <sa-kbd>⌥</sa-kbd>
    <sa-kbd>⌃</sa-kbd>
</sa-kbd-group>
<sa-kbd-group>
    <sa-kbd>Ctrl</sa-kbd>
    <span>+</span>
    <sa-kbd>B</sa-kbd>
</sa-kbd-group>
```

*From `Pages/Kbd/_KbdGroup.cshtml`*

```razor
<sa-kbd-group>
    <sa-kbd>Ctrl</sa-kbd>
    <sa-kbd>Shift</sa-kbd>
    <sa-kbd>P</sa-kbd>
</sa-kbd-group>
```

*From `Pages/Kbd/_InputGroup.cshtml`*

```razor
<sa-input-group>
    <sa-input-group-input />
    <sa-input-group-addon>
        <sa-kbd>Space</sa-kbd>
    </sa-input-group-addon>
</sa-input-group>
```
