---
component: EnvironmentIndicator
tags: [sa-environment-frame, sa-environment-banner, sa-environment-stripe]
generated: true
---

# EnvironmentIndicator

Renders a border around the window, with a tab on the top edge that names the environment the application runs in.

<!-- structure:begin -->

## Choosing an indicator

The three tags are alternatives; render one of them, only in the environments that should show it. How the application determines its environment is up to it.

- `<sa-environment-stripe>`: a thin bar across the top of the window, with the name on a tab.
- `<sa-environment-frame>`: a border around the window, with the name on a tab at the top.
- `<sa-environment-banner>`: a bar across the top of the window, with an optional `description` after the name. The page and the `<sa-sidebar>` move down by its height.

Each is fixed to the window, so it can go anywhere in the page, typically the layout. The text on it is white or dark, whichever reads better on its colour. `custom-color` takes any CSS colour and overrides `color`; `custom-dark-color` is used in dark mode.

```razor
@if (environmentName is not null)
{
    <sa-environment-stripe label="@environmentName" color="EnvironmentStripeColor.Warning"/>
}
```

<!-- structure:end -->

## Tags

| Tag | Description |
|-----|-------------|
| `<sa-environment-frame>` | Renders a border around the window, with a tab on the top edge that names the environment the application runs in. |
| `<sa-environment-banner>` | Renders a bar across the top of the window that names the environment the application runs in. |
| `<sa-environment-stripe>` | Renders a thin bar across the top of the window, with a tab that names the environment the application runs in. |

## Attributes

### `<sa-environment-frame>`

| Attribute | Type | Default | Values |
|-----------|------|---------|--------|
| `color` | `EnvironmentFrameColor` | `Neutral` | `Neutral`, `Info`, `Success`, `Warning`, `Danger` |
| `custom-color` | `string` | `null` | — |
| `custom-dark-color` | `string` | `null` | — |
| `label` | `string` | — | — |
| `tab-size` | `EnvironmentFrameTabSize` | `Medium` | `Small`, `Medium`, `Large`, `ExtraLarge` |
| `class` | `string` | — | Extra Tailwind utilities; merged last, so it overrides defaults. |

> In Razor, enum values are written fully-qualified, e.g. `variant="ButtonVariant.Outline"`.

### `<sa-environment-banner>`

| Attribute | Type | Default | Values |
|-----------|------|---------|--------|
| `color` | `EnvironmentBannerColor` | `Neutral` | `Neutral`, `Info`, `Success`, `Warning`, `Danger` |
| `custom-color` | `string` | `null` | — |
| `custom-dark-color` | `string` | `null` | — |
| `description` | `string` | `null` | — |
| `label` | `string` | — | — |
| `class` | `string` | — | Extra Tailwind utilities; merged last, so it overrides defaults. |

### `<sa-environment-stripe>`

| Attribute | Type | Default | Values |
|-----------|------|---------|--------|
| `color` | `EnvironmentStripeColor` | `Neutral` | `Neutral`, `Info`, `Success`, `Warning`, `Danger` |
| `custom-color` | `string` | `null` | — |
| `custom-dark-color` | `string` | `null` | — |
| `label` | `string` | — | — |
| `tab-size` | `EnvironmentStripeTabSize` | `Medium` | `Small`, `Medium`, `Large`, `ExtraLarge` |
| `class` | `string` | — | Extra Tailwind utilities; merged last, so it overrides defaults. |

## Examples

*From `Pages/EnvironmentIndicator/_Intro.cshtml`*

```razor
<sa-environment-stripe label="Testing" color="EnvironmentStripeColor.Warning"/>
```

*From `Pages/EnvironmentIndicator/_Frame.cshtml`*

```razor
<sa-environment-frame label="Local" color="EnvironmentFrameColor.Info"/>
```

*From `Pages/EnvironmentIndicator/_Banner.cshtml`*

```razor
<sa-environment-banner label="Live" color="EnvironmentBannerColor.Danger" description="Changes affect live customers"/>
```

*From `Pages/EnvironmentIndicator/_CustomColor.cshtml`*

```razor
<sa-environment-banner label="Staging" custom-color="#7c3aed" custom-dark-color="#c4b5fd" description="Data is reset every night"/>
```

*From `Pages/EnvironmentIndicator/_TabSize.cshtml`*

```razor
<sa-environment-stripe label="Testing" color="EnvironmentStripeColor.Warning" tab-size="EnvironmentStripeTabSize.ExtraLarge"/>
```
