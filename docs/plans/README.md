# Product and cross-repo plans

This index lists the live plans: proposed, open, deferred or in progress. Completed, superseded and closed plans live in [archive/](archive/) (older component plans in [oss/archive/](oss/archive/)), and each plan's status line records where it stands. Maintained architecture and execution guidance live in `docs/repos/` and `docs/development.md`.

## Live plans

- [admin-panel-backlog](admin-panel-backlog.md) (backlog): admin panel features comparable frameworks offer and StellarAdmin lacks; an item becomes a plan when picked up.
- [widgets](widgets.md) (in progress): widgets on the home page and resource pages; phases 1 to 3 implemented, open questions on index placement and reloading.
- [predefined-widgets](predefined-widgets.md) (proposed): ready-made widgets on the widget mechanism, stats first.
- [index-page-refreshing](index-page-refreshing.md) (open): selective htmx refreshes on the index page, so scopes and search stop flashing the page.
- [index-layouts](index-layouts.md) (approved, stage 5 implemented): switchable index layouts, first additional tables and custom partials (including a different data shape); board, map and others later.
- [cards-layout](cards-layout.md) (in progress): a built-in Cards layout for index pages, composed from image, title, description, badge, stats, fields and actions.
- [row-detail-expand-when](row-detail-expand-when.md) (deferred): per-row expand rules for row details, until an application needs them.
- [configured-row-details](configured-row-details.md) (proposed): row-detail panels configured from display fields instead of hand-written partials.
- [display-defaults](display-defaults.md) (proposed): app-level default displays, and display handlers in index columns.
- [grid-keyboard-navigation](grid-keyboard-navigation.md) (proposed): keyboard navigation and shortcuts in the index grid.
- [app-header-breadcrumb](app-header-breadcrumb.md) (proposed): a breadcrumb in the app header in place of the brand title.
- [unsaved-changes](unsaved-changes.md) (proposed): an unsaved changes indicator beside a form's Save button.
- [page-subtitles](page-subtitles.md) (done): a record summary under the title of the view and edit pages; implemented and documented.
- [template-folders](template-folders.md) (done): move the compact display and editor templates into `Compact/` subfolders of `DisplayTemplates/` and `EditorTemplates/`, replacing `GridDisplayTemplates/` and `GridEditorTemplates/`.
- [efcore-model-validation](efcore-model-validation.md) (proposed): form validation from the EF model's rules, not only attributes.
- [global-labels-rework](global-labels-rework.md) (proposed): global labels covering all the admin panel's text, and the docs page to match.
- [overridable-partials](overridable-partials.md) (proposed): a catalogue of overridable partial views, more of them, and better docs.
- [tailwind-admin-panel](tailwind-admin-panel.md) (proposed): an example and docs for Tailwind in custom pages and editors.
- [quick-start-project](quick-start-project.md) (proposed): a downloadable starter project for the admin panel quick start.
- [crud-screen-tag-helpers](crud-screen-tag-helpers.md) (proposed): screen-composition tag helpers for record pages, form actions and details.
- [homepage-component-embeds](homepage-component-embeds.md) (proposed): live StellarAdmin components on the website homepage.

## Maintaining these records

Update the relevant plan and its status line when work resumes. Record the current scope, actual source paths, remaining decisions and checks performed; keep historical results separate. When a plan is completed, superseded or closed, move it to `archive/`, remove its line here and update incoming links. Do not treat proposals or past session permissions as authorization to implement, commit or publish.
