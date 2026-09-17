---
component: Popover
tags: [sa-popover, sa-popover-description, sa-popover-header, sa-popover-title]
generated: true
---

# Popover

A floating panel of rich content anchored to a trigger element, rendered as a native popover.

## Tags

| Tag | Description |
|-----|-------------|
| `<sa-popover>` | A floating panel of rich content anchored to a trigger element, rendered as a native popover. |
| `<sa-popover-description>` | The descriptive body text of a popover, shown beneath the title. |
| `<sa-popover-header>` | The header region of a popover; typically contains the title and description. |
| `<sa-popover-title>` | The title heading of a popover. |

## Attributes

### `<sa-popover>`

| Attribute | Type | Default | Values |
|-----------|------|---------|--------|
| `position` | `PositionArea` | `Bottom` | `TopCenter`, `TopSpanLeft`, `TopSpanRight`, `Top`, `LeftCenter`, `LeftSpanTop`, `LeftSpanBottom`, `Left`, `BottomCenter`, `BottomSpanLeft`, `BottomSpanRight`, `Bottom`, `RightCenter`, `RightSpanTop`, `RightSpanBottom`, `Right`, `TopLeft`, `TopRight`, `BottomLeft`, `BottomRight` |
| `class` | `string` | — | Extra Tailwind utilities; merged last, so it overrides defaults. |

> In Razor, enum values are written fully-qualified, e.g. `variant="ButtonVariant.Outline"`.

## Examples

*From `Pages/Popover/_Intro.cshtml`*

```razor
<sa-button variant="ButtonVariant.Outline" popovertarget="--popover-intro">
    <sa-icon name="sliders-horizontal" class="text-muted-foreground"/>
    Configure View
</sa-button>
<sa-popover id="--popover-intro">
    <div class="flex flex-col gap-y-3">
        <sa-field-set>
            <sa-field-legend variant="FieldLegendVariant.Label">Sort By</sa-field-legend>
            <sa-field-group data-slot="radio-group">
                <sa-field orientation="FieldOrientation.Horizontal">
                    <sa-input type="radio" name="sort-by" value="departure" id="sort-departure" checked/>
                    <sa-field-label for="sort-departure" class="font-normal">
                        Departure Date
                    </sa-field-label>
                </sa-field>
                <sa-field orientation="FieldOrientation.Horizontal">
                    <sa-input type="radio" name="sort-by" value="posted" id="sort-posted"/>
                    <sa-field-label for="sort-posted" class="font-normal">
                        Date Posted
                    </sa-field-label>
                </sa-field>
            </sa-field-group>
        </sa-field-set>
        <sa-separator orientation="SeparatorOrientation.Horizontal"/>
        <sa-field-set>
            <sa-field-legend variant="FieldLegendVariant.Label">View As</sa-field-legend>
            <sa-field-group data-slot="radio-group">
                <sa-field orientation="FieldOrientation.Horizontal">
                    <sa-input type="radio" name="view-as" value="departure" id="view-list" checked/>
                    <sa-field-label for="view-list" class="font-normal">
                        List
                    </sa-field-label>
                </sa-field>
                <sa-field orientation="FieldOrientation.Horizontal">
                    <sa-input type="radio" name="view-as" value="posted" id="view-gallery"/>
                    <sa-field-label for="view-gallery" class="font-normal">
                        Gallery
                    </sa-field-label>
                </sa-field>
            </sa-field-group>
        </sa-field-set>
    </div>
</sa-popover>
```

*From `Pages/Popover/_JsApi.cshtml`*

```razor
<sa-stack align="StackAlign.Center">
    <sa-group>
        <sa-button variant="ButtonVariant.Outline" id="--popover-js-api-button-open">
            Open
        </sa-button>
        <sa-button variant="ButtonVariant.Outline" id="--popover-js-api-button-close">
            Close
        </sa-button>
        <sa-button variant="ButtonVariant.Outline" id="--popover-js-api-button-toggle">
            Toggle
        </sa-button>
    </sa-group>
    <sa-avatar src="/avatars/avatar-1.jpg" id="--popover-js-api-avatar"/>
</sa-stack>
<sa-popover id="--popover-js-api" popover="manual">
    <sa-stack>
        <sa-skeleton class="h-4 w-[250px]"/>
        <sa-skeleton class="h-4 w-[250px]"/>
    </sa-stack>
</sa-popover>
<script>
    const apiPopover = document.getElementById('--popover-js-api');
    const apiPopoverAvatar = document.getElementById('--popover-js-api-avatar');
    const apiPopoverButtonOpen = document.getElementById('--popover-js-api-button-open');
    const apiPopoverButtonClose = document.getElementById('--popover-js-api-button-close');
    const apiTooltipButtonToggle = document.getElementById('--popover-js-api-button-toggle');

    apiPopoverButtonOpen.addEventListener('click', () => {
        apiPopover.showPopover({
            source: apiPopoverAvatar
        });
    });
    apiPopoverButtonClose.addEventListener('click', () => {
        apiPopover.hidePopover();
    });
    apiTooltipButtonToggle.addEventListener('click', () => {
        apiPopover.togglePopover({
            source: apiPopoverAvatar
        });
    });
</script>
```
