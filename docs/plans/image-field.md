# Image display and editor

Status: proposed (2026-10-16). UX agreed in the prototype `sandbox/html/image-field.html`, which records every decision with its options.

## Goal

Show and edit an image stored as a single string property. The stored value can be a full URL, a file name or any other reference: a per-field resolver turns it into a URL, and a per-field storage callback saves an uploaded file and returns the value to store. The field works on its one property only; it is never combined with another field.

## Decisions (Jerrie, 2026-10-16)

1. One display, `ImageDisplay`, for the grid and the view page, and one editor, `ImageEditor`, for forms and inline grid rows.
2. The resolver is per field: `ResolveUrl` on both `ImageDisplay` and `ImageEditor`, with a synchronous overload and an asynchronous one that receives the services. It returns only a URL. Without it, the stored value is used as the URL.
3. Nothing beyond the image is ever shown: no file name, size or link. The lightbox only shows a larger image with a close button; the user opens the original with the browser's "Open image in new tab".
4. Uploading is the app's job: `StoreImage` receives the file and returns the value to store. The file is sent when picked (prototype question 12), with progress; the form posts only the returned value.
5. StellarAdmin never deletes images, neither discarded uploads nor replaced or removed ones. Cleaning up unreferenced files is the app's job, and the docs say so.
6. `MaxFileSize` (default 5 MB) and `AcceptedTypes` (default JPEG, PNG, WebP, GIF and AVIF; no SVG) are configurable, and the server checks both again.
7. The grid and view-page settings (prototype questions 1, 2, 5 and 7):
   - `AspectRatio` (`ImageAspectRatio`: `Square`, `Landscape` 4:3, `Widescreen` 16:9, `Portrait` 3:4, `Original`). Unset means `Square` in the grid and `Original` on the view page.
   - `Fit` (`ImageFit`: `Cover`, `Contain`), default `Cover`.
   - `Size` (`ImageDisplaySize`), grid height and view-page maximum height: `ExtraSmall` 24px / 8rem, `Small` 32px / 10rem (grid default), `Medium` 40px / 15rem (view-page default), `Large` 64px / 20rem.
   - `Interaction` (`ImageDisplayInteraction`: `Lightbox`, `Preview` on hover, `None`), default `Lightbox`.
8. Not configurable (questions 3, 4, 6, 8, 9 and 11): theme-rounded corners capped below a circle; an empty value shows a placeholder tile in the grid and a "No image" placeholder on the view page; an image that fails to load shows the broken-image tile; nothing under the picture on the view page; in an editing grid row the thumbnail is the button, × on its corner removes, and the row's Cancel discards (no Save or Cancel of its own); the dropzone's replace and remove are always visible.
9. The editor's layout is configurable: `Layout` (`ImageEditorLayout`: `Dropzone`, `Compact`, `Attachment`), default `Dropzone`. Inline grid rows always use the thumbnail button.

## API

```csharp
columns.Add(d => d.Photo, column => column.UseDisplay<ImageDisplay>(image =>
{
    image.ResolveUrl(value => $"https://cdn.voyager.travel/photos/{value}");
    image.AspectRatio = ImageAspectRatio.Landscape;
    image.Fit = ImageFit.Contain;
    image.Size = ImageDisplaySize.Large;
    image.Interaction = ImageDisplayInteraction.None;
}));

fields.Add(d => d.Photo, field => field.UseEditor<ImageEditor>(editor =>
{
    editor.Layout = ImageEditorLayout.Compact;
    editor.ResolveUrl(async (value, services, ct) =>
        await services.GetRequiredService<IPhotoStore>().GetUrlAsync(value, ct));
    editor.StoreImage(async (file, services, ct) =>
        await services.GetRequiredService<IPhotoStore>().SaveAsync(file, ct));
    editor.MaxFileSize = 10 * 1024 * 1024;
    editor.AcceptedTypes = ["image/jpeg", "image/png", "image/webp"];
}));
```

- `ResolveUrl(Func<string, string?>)` and `ResolveUrl(Func<string, IServiceProvider, CancellationToken, Task<string?>>)`.
- `StoreImage(Func<IFormFile, IServiceProvider, CancellationToken, Task<string>>)`. An `ImageEditor` without `StoreImage` fails when the resource options are validated.

## Stages

1. **StellarAdmin.UI components.** From the prototype: the image thumbnail and figure (`sa-image`), the shared lightbox and hover preview, and the upload control (`sa-image-upload` with a `sel-image-upload` web component) in its three layouts and the grid-cell thumbnail button. The control posts the file to an upload URL it is given, shows progress, checks size and type in the browser, and keeps the returned value in a hidden input; replace, remove and undo are staged until the form saves. CSS in `Client/css/components/`, tag helper tests, ComponentPlayground pages and browser checks across the theme fixtures, light and dark.
   Done (2026-10-16, not committed). `sa-image` (`variant` `Thumbnail` or `Figure`, `size`, `aspect-ratio`, `fit`, `interaction`, `src`, `alt`) and `sa-image-upload` (`asp-for` or `name`/`value`, `src`, `upload-url`, `layout` `Dropzone`/`Compact`/`Attachment`/`Cell`, `accept`, `max-file-size`, `disabled`, `form`), with the UI enums `ImageVariant`, `ImageSize`, `ImageAspectRatio`, `ImageFit`, `ImageInteraction` and `ImageUploadLayout`. Four semantic icon roles appended: `Image`, `ImageError`, `Upload`, `Remove`. CSS in `image.css` and `image-upload.css`; `js/image.ts` handles the loaded and broken states, the lightbox (a template written with the first lightbox image of a response) and the hover preview; `sel-image-upload` uploads with `XMLHttpRequest` for progress, sends the form's antiforgery token, stages replace and remove with undo, and blocks the form's submit while uploading. The attachment layout is built on `sa-attachment`. ComponentPlayground pages Image and Image Upload (a temporary-folder upload endpoint with a pause, and a failing one). Verified: the UI tag helper tests (552 passing, 11 new); headless Chromium on ComponentPlayground: 30 upload checks (replace, progress, undo, remove, cancel, upload into empty, type and size rejection, server error, attachment, compact, cell, submit blocked while uploading) and 8 image checks (broken state, load states, one lightbox template, preview, lightbox open and sized to the image), all passing; screenshots in the default theme, Soft dark and Ops dark. Not done: the Ledger fixture, mobile widths, the contrast script, and website docs and consumer references (stage 4).
2. **`ImageDisplay`.** The display, its enums and `ResolveUrl`, with `Displays/Image.cshtml` in `DisplayTemplates` and `GridDisplayTemplates`, built on stage 1's components. The async resolver runs once per row in a grid; it is meant for computing URLs, not for queries.
3. **`ImageEditor` and uploads.** The editor, its enum, `StoreImage`, `MaxFileSize` and `AcceptedTypes`, with `Editors/Image.cshtml` in `EditorTemplates` and `GridEditorTemplates`. An upload endpoint on the resource controller, with the resource's create or edit authorization and antiforgery, checks the file and calls `StoreImage`, and returns the stored value and its resolved URL. It raises the request body limit to `MaxFileSize` for that request only. Integration tests for the endpoint (authorization, size and type rejection, the stored value posted with the form).
4. **Samples and docs.** A photo field in AdminPlayground (display, editor, inline row), the stellar-admin consumer skill reference, and the website docs.

Stages stop for review at each boundary.
