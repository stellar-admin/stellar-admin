---
component: Image
tags: [sa-image, sa-image-upload]
generated: true
---

# Image

Displays an image as a thumbnail or a figure, with a placeholder when there is none and a broken-image tile when it can't be loaded.

<!-- structure:begin -->
## Composition and behavior

`<sa-image>` is a thumbnail by default (`Small`, `Square`), or a figure with `variant="ImageVariant.Figure"` (`Medium`, `Original`), as on a record's details page. Without `src` it shows a placeholder; an image that fails to load shows a broken-image tile, which the script sets from the image's `error` event. With a `src`, a click opens the image larger in a dialog (`Lightbox`); `Preview` shows it larger on hover instead, and `None` does neither. Always give `alt`: it names the lightbox button too.

`<sa-image-upload>` uploads a picked or dropped file straight away, with progress, and keeps the value the server returns in a hidden input named by `asp-for` or `name`. The form posts only that value. Replace and remove are staged until the form saves, with Undo; the control blocks the form's submit while an upload is running. Code that saves without a form submit (htmx, fetch) should check the element's `uploading` property first.

The upload endpoint you give as `upload-url` receives a multipart POST with the file in the field `file`, and the form's antiforgery token both as the `__RequestVerificationToken` field and the `RequestVerificationToken` header (outside a form, the nearest token around the control). It answers with JSON: `{ "value": "...", "url": "..." }` on success, where `value` is what the form saves; or `{ "error": "..." }` with an error status, which the control shows. The browser checks `accept` and `max-file-size` (default JPEG, PNG, WebP, GIF and AVIF, up to 5 MB) before uploading; check both again on the server. `src` is the current image's URL when `value` is not a URL itself. Use `layout="ImageUploadLayout.Cell"` in an editing table row, with an `aria-label`, since it renders no label.
<!-- structure:end -->

## Tags

| Tag | Description |
|-----|-------------|
| `<sa-image>` | Displays an image as a thumbnail or a figure, with a placeholder when there is none and a broken-image tile when it can't be loaded. |
| `<sa-image-upload>` | An image picker that uploads the picked file straight away and keeps the value the upload returns in a hidden input, which is what the form posts. |

## Attributes

### `<sa-image>`

| Attribute | Type | Default | Values |
|-----------|------|---------|--------|
| `alt` | `string` | — | — |
| `aspect-ratio` | `ImageAspectRatio` | `Square` | `Square`, `Landscape`, `Widescreen`, `Portrait`, `Original` |
| `fit` | `ImageFit` | `Cover` | `Cover`, `Contain` |
| `interaction` | `ImageInteraction` | `Lightbox` | `Lightbox`, `Preview`, `None` |
| `size` | `ImageSize` | `Small` | `ExtraSmall`, `Small`, `Medium`, `Large` |
| `src` | `string` | — | — |
| `variant` | `ImageVariant` | `Thumbnail` | `Thumbnail`, `Figure` |
| `class` | `string` | — | Extra Tailwind utilities; merged last, so it overrides defaults. |

> In Razor, enum values are written fully-qualified, e.g. `variant="ButtonVariant.Outline"`.

### `<sa-image-upload>`

| Attribute | Type | Default | Values |
|-----------|------|---------|--------|
| `accept` | `string` | — | — |
| `disabled` | `bool` | `false` | `true`, `false` |
| `form` | `string` | — | — |
| `layout` | `ImageUploadLayout` | `Dropzone` | `Dropzone`, `Compact`, `Attachment`, `Cell` |
| `max-file-size` | `long` | — | — |
| `src` | `string` | — | — |
| `upload-url` | `string` | — | — |
| `value` | `string` | — | — |
| `column-span` | `string` | — | — |
| `column-span-lg` | `string` | — | — |
| `column-span-md` | `string` | — | — |
| `column-span-sm` | `string` | — | — |
| `column-start` | `string` | `auto` | — |
| `column-start-lg` | `string` | — | — |
| `column-start-md` | `string` | — | — |
| `column-start-sm` | `string` | — | — |
| `description` | `string` | — | — |
| `error` | `string` | — | — |
| `asp-for` | `ModelExpression` | — | — |
| `label` | `string` | — | — |
| `name` | `string` | — | — |
| `wrap-in-field` | `bool` | — | `true`, `false` |
| `class` | `string` | — | Extra Tailwind utilities; merged last, so it overrides defaults. |

## Examples

*From `Pages/Image/_Intro.cshtml`*

```razor
<sa-stack>
    <sa-item variant="ItemVariant.Outline" size="ItemSize.Small">
        <sa-item-media>
            <sa-image src="/cities/kyoto.jpg" alt="Kiyomizu-dera above red autumn foliage in Kyoto" size="ImageSize.Medium" />
        </sa-item-media>
        <sa-item-content>
            <sa-item-title>Kyoto</sa-item-title>
            <sa-item-description>Autumn temples, 6 nights</sa-item-description>
        </sa-item-content>
    </sa-item>
    <sa-item variant="ItemVariant.Outline" size="ItemSize.Small">
        <sa-item-media>
            <sa-image src="/cities/lisbon.jpg" alt="Terracotta rooftops and historic churches in Lisbon" size="ImageSize.Medium" />
        </sa-item-media>
        <sa-item-content>
            <sa-item-title>Lisbon</sa-item-title>
            <sa-item-description>Tram 28 and the old town, 4 nights</sa-item-description>
        </sa-item-content>
    </sa-item>
</sa-stack>
```

*From `Pages/Image/_Figure.cshtml`*

```razor
<sa-details>
    <sa-display-field label="Destination">Lisbon, Portugal</sa-display-field>
    <sa-display-field label="Cover photo">
        <sa-image src="/cities/lisbon.jpg" alt="Terracotta rooftops and historic churches in Lisbon" variant="ImageVariant.Figure" />
    </sa-display-field>
</sa-details>
```

*From `Pages/Image/_States.cshtml`*

```razor
<div class="flex flex-wrap items-start gap-6">
    <sa-image size="ImageSize.Large" />
    <sa-image src="/cities/valparaiso.jpg" alt="Colourful houses on the hills of Valparaíso" size="ImageSize.Large" />
    <sa-image variant="ImageVariant.Figure" size="ImageSize.ExtraSmall" aspect-ratio="ImageAspectRatio.Landscape" />
    <sa-image src="/cities/valparaiso.jpg" alt="Colourful houses on the hills of Valparaíso" variant="ImageVariant.Figure" size="ImageSize.ExtraSmall" aspect-ratio="ImageAspectRatio.Landscape" />
</div>
```

*From `Pages/ImageUpload/_Intro.cshtml`*

```razor
<sa-image-upload name="CoverPhoto" value="kyoto.jpg" src="/cities/kyoto.jpg" upload-url="/ImageUpload?handler=Upload"
                 label="Cover photo" description="Shown at the top of the tour page." />
```

*From `Pages/ImageUpload/_Cell.cshtml`*

```razor
<sa-table>
    <sa-table-header>
        <sa-table-row>
            <sa-table-head>Photo</sa-table-head>
            <sa-table-head>Destination</sa-table-head>
        </sa-table-row>
    </sa-table-header>
    <sa-table-body>
        <sa-table-row>
            <sa-table-cell>
                <sa-image-upload name="rows[1].Photo" value="cape-town.jpg" src="/cities/cape-town.jpg" upload-url="/ImageUpload?handler=Upload"
                                 layout="ImageUploadLayout.Cell" aria-label="Photo of Cape Town" />
            </sa-table-cell>
            <sa-table-cell>Cape Town</sa-table-cell>
        </sa-table-row>
        <sa-table-row>
            <sa-table-cell>
                <sa-image-upload name="rows[2].Photo" upload-url="/ImageUpload?handler=Upload"
                                 layout="ImageUploadLayout.Cell" aria-label="Photo of Reykjavík" />
            </sa-table-cell>
            <sa-table-cell>Reykjavík</sa-table-cell>
        </sa-table-row>
    </sa-table-body>
</sa-table>
```
