---
component: RichTextEditor
tags: [sa-rich-text-editor]
generated: true
---

# RichTextEditor

Renders an editor for formatted text, such as headings, lists, links, tables and images, whose value is HTML.

## Attributes

| Attribute | Type | Default | Values |
|-----------|------|---------|--------|
| `accept` | `string` | — | — |
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
