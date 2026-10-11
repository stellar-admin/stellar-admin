---
name: stellar-admin-theme
description: >-
  Create or change a StellarAdmin theme: turn a description ("warm, editorial,
  dense, green accent"), brand colours or a reference look into a theme file of
  CSS variables for StellarAdmin.TagHelpers or StellarAdmin.Dashboard, link it,
  load its fonts, and give a theme builder link to preview and tune it. Use when
  the user wants a new look, a preset, brand colours, different density,
  rounder or sharper corners, flatter or deeper surfaces, or other fonts in a
  StellarAdmin app. Does not cover component markup; use the Tag Helpers skill
  for that.
metadata:
  author: StellarAdmin
---

# Creating StellarAdmin themes

StellarAdmin's stylesheet, `stellar-admin.css`, carries every component and the default theme. Any other look is a short CSS file of **theme variables** (`--sa-accent`, `--sa-radius`, `--sa-density`, …) that the app owns and links after the stylesheet. Every colour, size and shadow derives from the variables, so a theme changes all components at once, in light and dark mode, without component CSS.

## Version compatibility

This skill follows StellarAdmin's default branch. Theme files of CSS variables need a StellarAdmin.TagHelpers package that ships `stellar-admin.css`. If the app's package instead ships one bundle per theme (`stellar-admin.observatory.css`, `stellar-admin.shadcn.nova.css`, …), it predates theme variables: tell the user, and don't write a theme file for it. The installed package lists its variables in `stellar-admin.theme-variables.json` (served at `/_content/StellarAdmin.TagHelpers/stellar-admin.theme-variables.json`); don't use a variable that file doesn't list.

## Workflow

1. **Find what the app has.** Check the package version, the layout's stylesheet links (or, in a Dashboard app, `AddStylesheet` calls in `Program.cs`), and any existing theme file. Changing an existing theme means editing its variables, not starting over.
2. **Choose the variables.** Read the [theme variable reference](references/theme-variables.md). Start from the default, or from the worked example closest to the request, and set only the variables the request calls for; a theme of four to ten variables is normal. Use its "From a description to variables" table for adjectives. Brand colours can be any CSS colour syntax, hex included. If the request is vague, make reasonable choices and say what you chose rather than asking about every variable.
3. **Write the theme file**, for example `wwwroot/css/theme.css`, with a header comment that describes the look and carries a theme builder link (the reference shows how to build it):

   ```css
   /* StellarAdmin theme: warm, editorial, dense, green accent. Link it after stellar-admin.css.
      Builder: https://www.stellaradmin.com/theme-builder#v1:accent=oklch(0.52%200.13%20150)&… */
   @layer sa.theme {
     :root {
       --sa-accent: oklch(0.52 0.13 150);
       --sa-neutral-hue: 80;
     }
   }
   ```

   Variables go on `:root` inside `@layer sa.theme`. Don't set `--sa-color-*` or other derived tokens, don't write `.dark` blocks (use the `-dark` variables when dark mode should differ), and don't write component rules for a look a variable covers.
4. **Link it after `stellar-admin.css`.** In a Tag Helpers app's layout:

   ```razor
   <link rel="stylesheet" href="/_content/StellarAdmin.TagHelpers/stellar-admin.css" asp-append-version="true"/>
   <link rel="stylesheet" href="~/css/theme.css" asp-append-version="true"/>
   ```

   In a Dashboard app, register it in `Program.cs`; the Dashboard layout links `stellar-admin.css` first, then registered stylesheets in order:

   ```csharp
   builder.Services.AddStellarAdmin().AddDashboard(dashboard =>
   {
       dashboard.AddStylesheet("~/css/theme.css");
   });
   ```

5. **Load its fonts.** The stylesheet never downloads fonts; each font variable is a stack that falls back to native fonts. For a web font, link it from the layout before the stylesheets (or `dashboard.AddStylesheet("https://fonts.googleapis.com/…")`):

   ```html
   <link rel="stylesheet" href="https://fonts.googleapis.com/css2?family=Fraunces:wght@400..700&amp;display=swap"/>
   ```

   A builder download may begin with `@import url("https://fonts.googleapis.com/…")` instead. That works only as the first statement of a file that is linked; in a file that is itself `@import`ed, browsers drop it silently.
6. **Hand over** the theme builder link, so the user can see the theme on real components, tune it, and download the result to replace the file.

Text contrast is the token formulas' job: text on the accent switches between white and near-black automatically, and accent fills and accent text are adjusted to pass 4.5:1. Don't check contrast, unless the theme forces a text colour with `--sa-on-accent`.

To return to the default theme, remove the theme file's link. To offer several themes, keep one file per theme and switch which one is linked.

## Part of a page

The same variables re-derive everything inside the element they are set on. `data-sa-density="compact"` or `"comfortable"` changes a section's density; `data-sa-scope` marks an element that sets its own variables, such as a branded panel:

```razor
<section data-sa-scope style="--sa-accent: oklch(0.6 0.15 150); --sa-radius: 9999px;">
    <sa-button>Book now</sa-button>
</section>
```

Dark mode is the `dark` class on an ancestor, usually `<html>`; a theme needs nothing extra for it.

## Online tools

- Theme builder: <https://www.stellaradmin.com/theme-builder>
- Theming documentation: <https://www.stellaradmin.com/docs/tag-helpers/theming>
