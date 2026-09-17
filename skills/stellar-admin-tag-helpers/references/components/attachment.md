---
component: Attachment
tags: [sa-attachment, sa-attachment-action, sa-attachment-actions, sa-attachment-button-trigger, sa-attachment-content, sa-attachment-description, sa-attachment-group, sa-attachment-link-trigger, sa-attachment-media, sa-attachment-title]
generated: true
---

# Attachment

A file or image presented with its media, metadata, upload state, and actions.

## Tags

| Tag | Description |
|-----|-------------|
| `<sa-attachment>` | A file or image presented with its media, metadata, upload state, and actions. |
| `<sa-attachment-action>` | A single action control on an attachment, such as removing or retrying it. |
| `<sa-attachment-actions>` | The region of an attachment that holds its action controls. |
| `<sa-attachment-button-trigger>` | An attachment trigger rendered as a button, making the whole attachment activatable while leaving its actions clickable. |
| `<sa-attachment-content>` | The region of an attachment that holds its title and description. |
| `<sa-attachment-description>` | The supporting text of an attachment, such as its file type, size, or upload status. |
| `<sa-attachment-group>` | A horizontally scrolling row of attachments that snaps each one into view. |
| `<sa-attachment-link-trigger>` | An attachment trigger rendered as an anchor, making the whole attachment a clickable link while leaving its actions clickable. |
| `<sa-attachment-media>` | The media region of an attachment, holding a file-type icon or a thumbnail preview. |
| `<sa-attachment-title>` | The name of an attachment, truncated to fit and shimmering while it uploads or processes. |

## Attributes

### `<sa-attachment>`

| Attribute | Type | Default | Values |
|-----------|------|---------|--------|
| `orientation` | `AttachmentOrientation` | `Horizontal` | `Horizontal`, `Vertical` |
| `size` | `AttachmentSize` | `Default` | `Default`, `Small`, `ExtraSmall` |
| `state` | `AttachmentState` | `Done` | `Idle`, `Uploading`, `Processing`, `Error`, `Done` |
| `class` | `string` | — | Extra Tailwind utilities; merged last, so it overrides defaults. |

> In Razor, enum values are written fully-qualified, e.g. `variant="ButtonVariant.Outline"`.

### `<sa-attachment-action>`

| Attribute | Type | Default | Values |
|-----------|------|---------|--------|
| `size` | `ButtonSize` | `IconExtraSmall` | `Default`, `ExtraSmall`, `Small`, `Large`, `Icon`, `IconExtraSmall`, `IconSmall`, `IconLarge` |
| `variant` | `ButtonVariant` | `Ghost` | `Default`, `Destructive`, `Outline`, `Secondary`, `Ghost`, `Link` |
| `class` | `string` | — | Extra Tailwind utilities; merged last, so it overrides defaults. |

### `<sa-attachment-link-trigger>`

| Attribute | Type | Default | Values |
|-----------|------|---------|--------|
| `asp-action` | `string` | `null` | — |
| `asp-area` | `string` | `null` | — |
| `asp-controller` | `string` | `null` | — |
| `asp-fragment` | `string` | — | — |
| `asp-host` | `string` | — | — |
| `asp-page` | `string` | `null` | — |
| `asp-page-handler` | `string` | `null` | — |
| `asp-protocol` | `string` | — | — |
| `asp-route` | `string` | `null` | — |
| `asp-all-route-data` | `IDictionary<string, string?>` | — | — |
| `asp-route-*` | `IDictionary<string, string?>` | — | — |
| `class` | `string` | — | Extra Tailwind utilities; merged last, so it overrides defaults. |

### `<sa-attachment-media>`

| Attribute | Type | Default | Values |
|-----------|------|---------|--------|
| `variant` | `AttachmentMediaVariant` | `Icon` | `Icon`, `Image` |
| `class` | `string` | — | Extra Tailwind utilities; merged last, so it overrides defaults. |

## Examples

*From `Pages/Attachment/_Intro.cshtml`*

```razor
<div class="flex flex-col gap-3">
    <div class="flex flex-wrap gap-3">
        <sa-attachment orientation="AttachmentOrientation.Vertical">
            <sa-attachment-media variant="AttachmentMediaVariant.Image">
                <img src="/gradients/gradient-1.jpg" alt="Sunrise over the Arashiyama bamboo grove"/>
            </sa-attachment-media>
            <sa-attachment-content>
                <sa-attachment-title>arashiyama-grove.jpg</sa-attachment-title>
                <sa-attachment-description>JPG · 1.8 MB</sa-attachment-description>
            </sa-attachment-content>
        </sa-attachment>
        <sa-attachment orientation="AttachmentOrientation.Vertical">
            <sa-attachment-media variant="AttachmentMediaVariant.Image">
                <img src="/avatars/avatar-1.jpg" alt="Priya Raman at the Fushimi Inari shrine"/>
            </sa-attachment-media>
            <sa-attachment-content>
                <sa-attachment-title>priya-at-fushimi.jpg</sa-attachment-title>
                <sa-attachment-description>JPG · 1.1 MB</sa-attachment-description>
            </sa-attachment-content>
        </sa-attachment>
        <sa-attachment orientation="AttachmentOrientation.Vertical">
            <sa-attachment-media variant="AttachmentMediaVariant.Image">
                <img src="/avatars/avatar-3.jpg" alt="Mika Tanaka in the Gion district"/>
            </sa-attachment-media>
            <sa-attachment-content>
                <sa-attachment-title>mika-at-gion.jpg</sa-attachment-title>
                <sa-attachment-description>JPG · 940 KB</sa-attachment-description>
            </sa-attachment-content>
        </sa-attachment>
    </div>
    <sa-attachment state="AttachmentState.Uploading" class="w-full">
        <sa-attachment-media>
            <sa-spinner/>
        </sa-attachment-media>
        <sa-attachment-content>
            <sa-attachment-title>hotel-invoice.pdf</sa-attachment-title>
            <sa-attachment-description>Uploading · 64%</sa-attachment-description>
        </sa-attachment-content>
        <sa-attachment-actions>
            <sa-attachment-action aria-label="Cancel uploading hotel-invoice.pdf">
                <sa-icon name="x"/>
            </sa-attachment-action>
        </sa-attachment-actions>
    </sa-attachment>
    <sa-attachment class="w-full">
        <sa-attachment-media>
            <sa-icon name="file-text"/>
        </sa-attachment-media>
        <sa-attachment-content>
            <sa-attachment-title>kyoto-itinerary.pdf</sa-attachment-title>
            <sa-attachment-description>PDF · 2.4 MB</sa-attachment-description>
        </sa-attachment-content>
        <sa-attachment-actions>
            <sa-attachment-action aria-label="Remove kyoto-itinerary.pdf">
                <sa-icon name="x"/>
            </sa-attachment-action>
        </sa-attachment-actions>
    </sa-attachment>
</div>
```

*From `Pages/Attachment/_States.cshtml`*

```razor
<div class="flex flex-col gap-3">
    <sa-attachment state="AttachmentState.Idle">
        <sa-attachment-media>
            <sa-icon name="upload"/>
        </sa-attachment-media>
        <sa-attachment-content>
            <sa-attachment-title>Attach a travel document</sa-attachment-title>
            <sa-attachment-description>PDF, JPG or PNG up to 10 MB</sa-attachment-description>
        </sa-attachment-content>
    </sa-attachment>
    <sa-attachment state="AttachmentState.Uploading">
        <sa-attachment-media>
            <sa-spinner/>
        </sa-attachment-media>
        <sa-attachment-content>
            <sa-attachment-title>flight-confirmation.pdf</sa-attachment-title>
            <sa-attachment-description>Uploading · 64%</sa-attachment-description>
        </sa-attachment-content>
        <sa-attachment-actions>
            <sa-attachment-action aria-label="Cancel uploading flight-confirmation.pdf">
                <sa-icon name="x"/>
            </sa-attachment-action>
        </sa-attachment-actions>
    </sa-attachment>
    <sa-attachment state="AttachmentState.Processing">
        <sa-attachment-media>
            <sa-icon name="id-card"/>
        </sa-attachment-media>
        <sa-attachment-content>
            <sa-attachment-title>passport-priya.jpg</sa-attachment-title>
            <sa-attachment-description>Checking the expiry date</sa-attachment-description>
        </sa-attachment-content>
    </sa-attachment>
    <sa-attachment state="AttachmentState.Error">
        <sa-attachment-media>
            <sa-icon name="circle-alert"/>
        </sa-attachment-media>
        <sa-attachment-content>
            <sa-attachment-title>hotel-invoice.pdf</sa-attachment-title>
            <sa-attachment-description>Upload failed — the file is over 10 MB</sa-attachment-description>
        </sa-attachment-content>
        <sa-attachment-actions>
            <sa-attachment-action aria-label="Retry uploading hotel-invoice.pdf">
                <sa-icon name="rotate-cw"/>
            </sa-attachment-action>
        </sa-attachment-actions>
    </sa-attachment>
    <sa-attachment state="AttachmentState.Done">
        <sa-attachment-media>
            <sa-icon name="file-text"/>
        </sa-attachment-media>
        <sa-attachment-content>
            <sa-attachment-title>kyoto-itinerary.pdf</sa-attachment-title>
            <sa-attachment-description>PDF · 2.4 MB</sa-attachment-description>
        </sa-attachment-content>
        <sa-attachment-actions>
            <sa-attachment-action aria-label="Remove kyoto-itinerary.pdf">
                <sa-icon name="x"/>
            </sa-attachment-action>
        </sa-attachment-actions>
    </sa-attachment>
</div>
```

*From `Pages/Attachment/_Image.cshtml`*

```razor
<div class="flex flex-wrap gap-3">
    <sa-attachment orientation="AttachmentOrientation.Vertical">
        <sa-attachment-media variant="AttachmentMediaVariant.Image">
            <img src="/gradients/gradient-1.jpg" alt="Sunrise over the Arashiyama bamboo grove"/>
        </sa-attachment-media>
        <sa-attachment-content>
            <sa-attachment-title>arashiyama-grove.jpg</sa-attachment-title>
            <sa-attachment-description>1.8 MB</sa-attachment-description>
        </sa-attachment-content>
        <sa-attachment-actions>
            <sa-attachment-action aria-label="Remove arashiyama-grove.jpg">
                <sa-icon name="x"/>
            </sa-attachment-action>
        </sa-attachment-actions>
    </sa-attachment>
    <sa-attachment orientation="AttachmentOrientation.Vertical">
        <sa-attachment-media variant="AttachmentMediaVariant.Image">
            <img src="/avatars/avatar-1.jpg" alt="Priya Raman at the Fushimi Inari shrine"/>
        </sa-attachment-media>
        <sa-attachment-content>
            <sa-attachment-title>priya-at-fushimi.jpg</sa-attachment-title>
            <sa-attachment-description>640 KB</sa-attachment-description>
        </sa-attachment-content>
        <sa-attachment-actions>
            <sa-attachment-action aria-label="Remove priya-at-fushimi.jpg">
                <sa-icon name="x"/>
            </sa-attachment-action>
        </sa-attachment-actions>
    </sa-attachment>
    <sa-attachment orientation="AttachmentOrientation.Vertical">
        <sa-attachment-media variant="AttachmentMediaVariant.Image">
            <img src="/avatars/avatar-2.jpg" alt="Arjun Raman in the Gion district"/>
        </sa-attachment-media>
        <sa-attachment-content>
            <sa-attachment-title>arjun-at-gion.jpg</sa-attachment-title>
            <sa-attachment-description>612 KB</sa-attachment-description>
        </sa-attachment-content>
        <sa-attachment-actions>
            <sa-attachment-action aria-label="Remove arjun-at-gion.jpg">
                <sa-icon name="x"/>
            </sa-attachment-action>
        </sa-attachment-actions>
    </sa-attachment>
</div>
```

*From `Pages/Attachment/_Group.cshtml`*

```razor
<sa-attachment-group tabindex="0" role="group" aria-label="Documents for the Kyoto trip">
    <sa-attachment orientation="AttachmentOrientation.Vertical">
        <sa-attachment-media variant="AttachmentMediaVariant.Image">
            <img src="/gradients/gradient-1.jpg" alt="Sunrise over the Arashiyama bamboo grove"/>
        </sa-attachment-media>
        <sa-attachment-content>
            <sa-attachment-title>arashiyama-grove.jpg</sa-attachment-title>
            <sa-attachment-description>1.8 MB</sa-attachment-description>
        </sa-attachment-content>
    </sa-attachment>
    <sa-attachment orientation="AttachmentOrientation.Vertical">
        <sa-attachment-media>
            <sa-icon name="file-text"/>
        </sa-attachment-media>
        <sa-attachment-content>
            <sa-attachment-title>kyoto-itinerary.pdf</sa-attachment-title>
            <sa-attachment-description>2.4 MB</sa-attachment-description>
        </sa-attachment-content>
    </sa-attachment>
    <sa-attachment orientation="AttachmentOrientation.Vertical">
        <sa-attachment-media>
            <sa-icon name="plane"/>
        </sa-attachment-media>
        <sa-attachment-content>
            <sa-attachment-title>flight-confirmation.pdf</sa-attachment-title>
            <sa-attachment-description>180 KB</sa-attachment-description>
        </sa-attachment-content>
    </sa-attachment>
    <sa-attachment orientation="AttachmentOrientation.Vertical">
        <sa-attachment-media variant="AttachmentMediaVariant.Image">
            <img src="/avatars/avatar-1.jpg" alt="Priya Raman at the Fushimi Inari shrine"/>
        </sa-attachment-media>
        <sa-attachment-content>
            <sa-attachment-title>priya-at-fushimi.jpg</sa-attachment-title>
            <sa-attachment-description>640 KB</sa-attachment-description>
        </sa-attachment-content>
    </sa-attachment>
    <sa-attachment orientation="AttachmentOrientation.Vertical">
        <sa-attachment-media>
            <sa-icon name="receipt"/>
        </sa-attachment-media>
        <sa-attachment-content>
            <sa-attachment-title>hotel-invoice.pdf</sa-attachment-title>
            <sa-attachment-description>96 KB</sa-attachment-description>
        </sa-attachment-content>
    </sa-attachment>
</sa-attachment-group>
```

*From `Pages/Attachment/_Trigger.cshtml`*

```razor
<div class="flex flex-col gap-3">
    <sa-attachment>
        <sa-attachment-media>
            <sa-icon name="file-text"/>
        </sa-attachment-media>
        <sa-attachment-content>
            <sa-attachment-title>kyoto-itinerary.pdf</sa-attachment-title>
            <sa-attachment-description>PDF · 2.4 MB</sa-attachment-description>
        </sa-attachment-content>
        <sa-attachment-actions>
            <sa-attachment-action aria-label="Remove kyoto-itinerary.pdf">
                <sa-icon name="x"/>
            </sa-attachment-action>
        </sa-attachment-actions>
        <sa-attachment-button-trigger commandfor="--attachment-preview" command="show-modal"
                                      aria-label="Preview kyoto-itinerary.pdf"/>
    </sa-attachment>
    <sa-attachment>
        <sa-attachment-media>
            <sa-icon name="plane"/>
        </sa-attachment-media>
        <sa-attachment-content>
            <sa-attachment-title>flight-confirmation.pdf</sa-attachment-title>
            <sa-attachment-description>PDF · 180 KB</sa-attachment-description>
        </sa-attachment-content>
        <sa-attachment-actions>
            <sa-attachment-action aria-label="Remove flight-confirmation.pdf">
                <sa-icon name="x"/>
            </sa-attachment-action>
        </sa-attachment-actions>
        <sa-attachment-link-trigger href="/gradients/gradient-1.jpg"
                                    aria-label="Download flight-confirmation.pdf"/>
    </sa-attachment>
</div>
<sa-dialog id="--attachment-preview">
    <sa-dialog-header>
        <sa-dialog-title>kyoto-itinerary.pdf</sa-dialog-title>
        <sa-dialog-description>12 to 19 April · 2 travellers · PDF · 2.4 MB</sa-dialog-description>
    </sa-dialog-header>
    <img src="/gradients/gradient-1.jpg" alt="First page of the Kyoto itinerary" class="w-full rounded-md"/>
    <sa-dialog-footer>
        <sa-button variant="ButtonVariant.Outline" commandfor="--attachment-preview" command="close">
            Close
        </sa-button>
    </sa-dialog-footer>
</sa-dialog>
```
