---
component: Layout
tags: [sa-group, sa-page-container, sa-stack]
generated: true
---

# Layout

A horizontal flex layout that arranges its children in a row, with configurable alignment, spacing, and justification.

<!-- structure:begin -->

## What's in here

"Layout" is a folder of **three independent layout primitives**, not one component family. The summary above describes `<sa-group>` only, because it sorts first. Read the Tags table below for each one:

| Tag | Use it for |
|-----|------------|
| `<sa-page-container>` | The outermost wrapper for a page's content — centers it, applies the page gutter, spaces its children vertically, and optionally constrains the max width. There is **no** `<sa-container>`. |
| `<sa-stack>` | A vertical column. |
| `<sa-group>` | A horizontal row. |

`<sa-stack>` and `<sa-group>` are the spacing primitives to reach for instead of hand-rolling flex utilities — control rhythm with their `gap` / `align` / `justify` enum attributes.

<!-- structure:end -->

## Tags

| Tag | Description |
|-----|-------------|
| `<sa-group>` | A horizontal flex layout that arranges its children in a row, with configurable alignment, spacing, and justification. |
| `<sa-page-container>` | A wrapper for the main content of a page. It centers the content horizontally, applies a consistent gutter, spaces its children vertically, and can constrain the content to a maximum width appropriate for the type of page. |
| `<sa-stack>` | A vertical flex layout that arranges its children in a column, with configurable alignment, spacing, and justification. |

## Attributes

### `<sa-group>`

| Attribute | Type | Default | Values |
|-----------|------|---------|--------|
| `align` | `GroupAlign` | `Start` | `Stretch`, `Center`, `Start`, `End`, `Baseline` |
| `gap` | `GroupGap` | `Default` | `ExtraSmall`, `Small`, `Default`, `Large`, `ExtraLarge` |
| `justify` | `GroupJustify` | `Start` | `Center`, `Start`, `End`, `SpaceBetween`, `SpaceAround` |
| `class` | `string` | — | Extra Tailwind utilities; merged last, so it overrides defaults. |

> In Razor, enum values are written fully-qualified, e.g. `variant="ButtonVariant.Outline"`.

### `<sa-page-container>`

| Attribute | Type | Default | Values |
|-----------|------|---------|--------|
| `width` | `PageContainerWidth` | `Full` | `Full`, `Large`, `Medium`, `Small` |
| `class` | `string` | — | Extra Tailwind utilities; merged last, so it overrides defaults. |

### `<sa-stack>`

| Attribute | Type | Default | Values |
|-----------|------|---------|--------|
| `align` | `StackAlign` | `Stretch` | `Stretch`, `Center`, `Start`, `End` |
| `gap` | `StackGap` | `Default` | `ExtraSmall`, `Small`, `Default`, `Large`, `ExtraLarge` |
| `justify` | `StackJustify` | `Start` | `Center`, `Start`, `End`, `SpaceBetween`, `SpaceAround` |
| `class` | `string` | — | Extra Tailwind utilities; merged last, so it overrides defaults. |

## Examples

*From `Pages/PageContainer/_Medium.cshtml`*

```razor
<sa-page-container width="PageContainerWidth.Medium">
    <sa-page-header>
        <sa-breadcrumb>
            <sa-breadcrumb-list>
                <sa-breadcrumb-item>
                    <sa-breadcrumb-link href="#">Destinations</sa-breadcrumb-link>
                </sa-breadcrumb-item>
                <sa-breadcrumb-separator/>
                <sa-breadcrumb-page>Kyoto, Japan</sa-breadcrumb-page>
            </sa-breadcrumb-list>
        </sa-breadcrumb>
        <sa-page-header-title>Kyoto, Japan</sa-page-header-title>
        <sa-page-header-description>
            Destination profile, seasonal pricing, and package availability.
        </sa-page-header-description>
        <sa-page-header-actions>
            <sa-button variant="ButtonVariant.Outline">
                <sa-icon name="pencil"/>
                Edit
            </sa-button>
        </sa-page-header-actions>
    </sa-page-header>
    <sa-card>
        <sa-card-header>
            <sa-card-title>Overview</sa-card-title>
        </sa-card-header>
        <sa-card-content>
            <dl class="grid gap-x-4 gap-y-3 text-sm sm:grid-cols-2">
                <div>
                    <dt class="text-muted-foreground">Region</dt>
                    <dd class="mt-1">Asia-Pacific</dd>
                </div>
                <div>
                    <dt class="text-muted-foreground">Status</dt>
                    <dd class="mt-1">
                        <sa-badge>Active</sa-badge>
                    </dd>
                </div>
                <div>
                    <dt class="text-muted-foreground">Peak season</dt>
                    <dd class="mt-1">March &ndash; May, October &ndash; November</dd>
                </div>
                <div>
                    <dt class="text-muted-foreground">Active packages</dt>
                    <dd class="mt-1">12</dd>
                </div>
            </dl>
        </sa-card-content>
    </sa-card>
    <sa-card>
        <sa-card-header>
            <sa-card-title>Description</sa-card-title>
        </sa-card-header>
        <sa-card-content>
            <p class="text-sm text-muted-foreground">
                Ancient temples, tranquil bamboo groves, and traditional ryokan stays make Kyoto one of
                Voyager Travel&apos;s most requested destinations. Our packages combine guided cultural
                tours with free days for independent exploration, and all itineraries include a
                dedicated local host.
            </p>
        </sa-card-content>
    </sa-card>
</sa-page-container>
```

*From `Pages/Stack/_Gap.cshtml`*

```razor
<p>Extra small</p>
<sa-stack gap="StackGap.ExtraSmall" class="font-mono text-sm leading-6 font-bold bg-muted rounded-lg">
    <div class="flex items-center justify-center rounded-lg bg-primary text-primary-foreground p-4">01</div>
    <div class="flex items-center justify-center rounded-lg bg-primary text-primary-foreground p-4">02</div>
    <div class="flex items-center justify-center rounded-lg bg-primary text-primary-foreground p-4">03</div>
</sa-stack>
<p>Small</p>
<sa-stack gap="StackGap.Small" class="font-mono text-sm leading-6 font-bold bg-muted rounded-lg">
    <div class="flex items-center justify-center rounded-lg bg-primary text-primary-foreground p-4">01</div>
    <div class="flex items-center justify-center rounded-lg bg-primary text-primary-foreground p-4">02</div>
    <div class="flex items-center justify-center rounded-lg bg-primary text-primary-foreground p-4">03</div>
</sa-stack>
<p>Default</p>
<sa-stack class="font-mono text-sm leading-6 font-bold bg-muted rounded-lg">
    <div class="flex items-center justify-center rounded-lg bg-primary text-primary-foreground p-4">01</div>
    <div class="flex items-center justify-center rounded-lg bg-primary text-primary-foreground p-4">02</div>
    <div class="flex items-center justify-center rounded-lg bg-primary text-primary-foreground p-4">03</div>
</sa-stack>
<p>Large</p>
<sa-stack gap="StackGap.Large" class="font-mono text-sm leading-6 font-bold bg-muted rounded-lg">
    <div class="flex items-center justify-center rounded-lg bg-primary text-primary-foreground p-4">01</div>
    <div class="flex items-center justify-center rounded-lg bg-primary text-primary-foreground p-4">02</div>
    <div class="flex items-center justify-center rounded-lg bg-primary text-primary-foreground p-4">03</div>
</sa-stack>
<p>Extra large</p>
<sa-stack gap="StackGap.ExtraLarge" class="font-mono text-sm leading-6 font-bold bg-muted rounded-lg">
    <div class="flex items-center justify-center rounded-lg bg-primary text-primary-foreground p-4">01</div>
    <div class="flex items-center justify-center rounded-lg bg-primary text-primary-foreground p-4">02</div>
    <div class="flex items-center justify-center rounded-lg bg-primary text-primary-foreground p-4">03</div>
</sa-stack>
```

*From `Pages/Group/_Justify.cshtml`*

```razor
<p>Start</p>
<sa-group justify="GroupJustify.Start" class="font-mono text-sm leading-6 font-bold bg-muted rounded-lg">
    <div class="flex items-center justify-center rounded-lg bg-primary text-primary-foreground p-4">01</div>
    <div class="flex items-center justify-center rounded-lg bg-primary text-primary-foreground p-4">02</div>
    <div class="flex items-center justify-center rounded-lg bg-primary text-primary-foreground p-4">03</div>
</sa-group>
<p>Center</p>
<sa-group justify="GroupJustify.Center" class="font-mono text-sm leading-6 font-bold bg-muted rounded-lg">
    <div class="flex items-center justify-center rounded-lg bg-primary text-primary-foreground p-4">01</div>
    <div class="flex items-center justify-center rounded-lg bg-primary text-primary-foreground p-4">02</div>
    <div class="flex items-center justify-center rounded-lg bg-primary text-primary-foreground p-4">03</div>
</sa-group>
<p>End</p>
<sa-group justify="GroupJustify.End" class="font-mono text-sm leading-6 font-bold bg-muted rounded-lg">
    <div class="flex items-center justify-center rounded-lg bg-primary text-primary-foreground p-4">01</div>
    <div class="flex items-center justify-center rounded-lg bg-primary text-primary-foreground p-4">02</div>
    <div class="flex items-center justify-center rounded-lg bg-primary text-primary-foreground p-4">03</div>
</sa-group>
<p>Justify between</p>
<sa-group justify="GroupJustify.SpaceBetween"
           class="font-mono text-sm leading-6 font-bold bg-muted rounded-lg">
    <div class="flex items-center justify-center rounded-lg bg-primary text-primary-foreground p-4">01</div>
    <div class="flex items-center justify-center rounded-lg bg-primary text-primary-foreground p-4">02</div>
    <div class="flex items-center justify-center rounded-lg bg-primary text-primary-foreground p-4">03</div>
</sa-group>
<p>Justify around</p>
<sa-group justify="GroupJustify.SpaceAround" class="font-mono text-sm leading-6 font-bold bg-muted rounded-lg">
    <div class="flex items-center justify-center rounded-lg bg-primary text-primary-foreground p-4">01</div>
    <div class="flex items-center justify-center rounded-lg bg-primary text-primary-foreground p-4">02</div>
    <div class="flex items-center justify-center rounded-lg bg-primary text-primary-foreground p-4">03</div>
</sa-group>
```
