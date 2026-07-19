---
component: Avatar
tags: [sa-avatar, sa-avatar-badge, sa-avatar-group, sa-avatar-group-count]
generated: true
---

# Avatar

Displays a user's image, falling back to initials or a name-derived monogram when no image is available.

## Tags

| Tag | Description |
|-----|-------------|
| `<sa-avatar>` | Displays a user's image, falling back to initials or a name-derived monogram when no image is available. |
| `<sa-avatar-badge>` | A small badge overlaid on the corner of an avatar, such as a status indicator or icon. |
| `<sa-avatar-group>` | A container that displays a set of avatars as an overlapping stack. |
| `<sa-avatar-group-count>` | A trailing element within an avatar group that displays the count of additional, unshown avatars. |

## Attributes

### `<sa-avatar>`

| Attribute | Type | Default | Values |
|-----------|------|---------|--------|
| `initials` | `string` | — | — |
| `name` | `string` | — | — |
| `size` | `AvatarSize` | `Default` | `Default`, `Small`, `Large` |
| `src` | `string` | — | — |
| `class` | `string` | — | Extra Tailwind utilities; merged last, so it overrides defaults. |

> In Razor, enum values are written fully-qualified, e.g. `variant="ButtonVariant.Outline"`.

## Examples

*From `Pages/Avatar/_Intro.cshtml`*

```razor
<sa-avatar src="/avatars/avatar-1.jpg"/>
<sa-avatar-group>
    <sa-avatar src="/avatars/avatar-1.jpg"/>
    <sa-avatar src="/avatars/avatar-2.jpg"/>
    <sa-avatar src="/avatars/avatar-3.jpg"/>
</sa-avatar-group>
```

*From `Pages/Avatar/_Badge.cshtml`*

```razor
<div class="flex flex-wrap items-center gap-2">
    <sa-avatar src="/avatars/avatar-2.jpg" size="AvatarSize.Small">
        <sa-avatar-badge/>
    </sa-avatar>
    <sa-avatar src="/avatars/avatar-2.jpg">
        <sa-avatar-badge/>
    </sa-avatar>
    <sa-avatar src="/avatars/avatar-2.jpg" size="AvatarSize.Large">
        <sa-avatar-badge/>
    </sa-avatar>
</div>
<div class="flex flex-wrap items-center gap-2">
    <sa-avatar initials="DU" size="AvatarSize.Small">
        <sa-avatar-badge/>
    </sa-avatar>
    <sa-avatar initials="DU">
        <sa-avatar-badge/>
    </sa-avatar>
    <sa-avatar initials="DU" size="AvatarSize.Large">
        <sa-avatar-badge/>
    </sa-avatar>
</div>
```

*From `Pages/Avatar/_Group.cshtml`*

```razor
<sa-avatar-group>
    <sa-avatar src="/avatars/avatar-1.jpg" size="AvatarSize.Small"/>
    <sa-avatar src="/avatars/avatar-2.jpg" size="AvatarSize.Small"/>
    <sa-avatar src="/avatars/avatar-3.jpg" size="AvatarSize.Small"/>
</sa-avatar-group>
<sa-avatar-group>
    <sa-avatar src="/avatars/avatar-1.jpg"/>
    <sa-avatar src="/avatars/avatar-2.jpg"/>
    <sa-avatar src="/avatars/avatar-3.jpg"/>
</sa-avatar-group>
<sa-avatar-group>
    <sa-avatar src="/avatars/avatar-1.jpg" size="AvatarSize.Large"/>
    <sa-avatar src="/avatars/avatar-2.jpg" size="AvatarSize.Large"/>
    <sa-avatar src="/avatars/avatar-3.jpg" size="AvatarSize.Large"/>
</sa-avatar-group>
```
