---
component: RichTextEditor
tags: [sa-rich-text-editor]
generated: true
---

# RichTextEditor

Renders an editor for formatted text, such as headings, lists, links, tables and images, whose value is HTML.

<!-- structure:begin -->
## When to use

Use it for long text that staff format: a guide's bio, a tour's itinerary, an email template. Use a [textarea](textarea.md) for plain text.

## Value, scripts and sanitizing

- The value is HTML in a child `<textarea>`, which the form posts under the field's name and which shows, as plain HTML text, until the editor's script has loaded. Lists post as `ul`/`ol`, and an empty editor posts an empty value. A value without tags opens as paragraphs.
- The editor renders the `<script type="module">` that loads its own file, `rich-text-editor.js` (about 120 KB compressed), inside its element. Pages without an editor never load it, several editors load it once, and an editor in an htmx response loads it when swapped in. `stellar-admin.js` must still be on the page, for the toolbar's menus and tooltips.
- **Sanitize the posted HTML on the server** before you store or show it: the browser can post anything. Allow only the markup the editor writes (`p`, `h1`–`h3`, `strong`, `em`, `u`, `s`, `code`, `pre`, `blockquote`, `ul`, `ol`, `li`, `a[href]`, `img[src width height]`, `table`, `tbody`, `tr`, `th`, `td[colspan rowspan data-colwidth]`, `hr`, `br`), for example with the HtmlSanitizer package. The admin panel does this for its `RichTextEditor`.
- Show saved HTML in an element with the `sa-rich-text` class, which formats it as the editor does: `<div class="sa-rich-text">@Html.Raw(sanitizedHtml)</div>`.

## Images

`upload-url` turns on images: staff paste, drop or insert them, and each one is posted to the URL as the multipart field `file`, with the form's antiforgery token in the `RequestVerificationToken` header and the form field. The endpoint checks the file again and answers `{ "url": "..." }`, or `{ "error": "..." }` with an error status, which the editor shows under the text. `accept` and `max-file-size` are checked in the browser first. Without `upload-url` there is no image button and pasted image files are ignored.
<!-- structure:end -->

## Attributes

| Attribute | Type | Default | Values |
|-----------|------|---------|--------|
| `accept` | `string` | — | — |
| `disabled` | `bool` | `false` | `true`, `false` |
| `max-file-size` | `long` | `5242880` | — |
| `placeholder` | `string` | — | — |
| `readonly` | `bool` | `false` | `true`, `false` |
| `rows` | `int` | `8` | — |
| `upload-url` | `string` | `null` | — |
| `value` | `string` | — | — |
| `class-names` | `RichTextEditorClassNames` | — | — |
| `column-span` | `string` | `1` | — |
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

## Example

*From `Pages/RichTextEditor/_Intro.cshtml`*

```razor
<sa-rich-text-editor name="Bio" label="Guide bio" description="Shown on the guide's public profile."
                     placeholder="Tell travellers about this guide…"
                     value="<h2>Meet Amara</h2><p>Amara has led <strong>small-group walking tours</strong> through Cape Town for twelve years. She grew up in Bo-Kaap and knows every <em>spice merchant</em> by name.</p><ul><li><p>Table Mountain sunrise hikes</p></li><li><p>Bo-Kaap food walks</p></li></ul>" />
```
