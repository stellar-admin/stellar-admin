# Admin panel backlog

Status: backlog (2026-10-14).

Admin panel features that comparable frameworks (Filament, Nova, Avo, ActiveAdmin, react-admin) offer and StellarAdmin does not. Recorded during the [admin panel skills](archive/admin-panel-skills.md) research so the consumer skill can name them as unsupported. None is scheduled; an item becomes a plan when it is picked up. Source: the API survey of 2026-10-14. Sidebar icons were removed from the table when they were implemented (2026-10-15, recorded in [sidebar-branding-user-menu](archive/sidebar-branding-user-menu.md)). The many-to-many follow-ups were removed when they were implemented (2026-10-20): a many-to-many field without a multi-select editor fails at startup, as does a column or display field showing a value with no text of its own (no `ToString()` override), or a collection of such values, with a built-in display other than `BadgeDisplay.UseText`. Unit tests in `ValueTextOptionsValidatorTests` cover the exemptions (badge text, custom displays, `[UIHint]`, `ToString()` overrides, scalar collections). Verified with the solution tests (1680, including new EF integration tests) and an AdminPlayground start.

| Feature | Current state |
| --- | --- |
| Custom row and page actions | Row actions are fixed to view, edit and delete. |
| Bulk actions | No row selection or bulk operations on the index. |
| Index filters | Scopes (named tabs) only; no field filters such as select, date range or ternary. |
| Form tabs and wizards | Forms have sections and groups only. |
| Dashboard and widgets | The home page shows a setup checklist in Development and links to the resources elsewhere; there is no widget or stat API. |
| File uploads | No general file upload editor. Images have `ImageEditor` and `ImageDisplay` ([image-field](archive/image-field.md)). |
| Fluent read-only fields | `FormFieldOptions.IsReadOnly` has no builder method; read-only comes only from `[ReadOnly]` or `[Editable(false)]` metadata. |
| Save hooks | No before/after create, update or delete callbacks. EF data source errors propagate. |
| Search and scope predicates for custom data sources | Only EF Core builders accept predicates; other data sources interpret `ResourceListRequest.Search` and `Scope` themselves. |
| Delete handler registration | No `AllowDelete<THandler>` to match `AllowCreate<TModel, THandler>` and `AllowEdit<TModel, THandler>`; delete needs the data source. |
| Display handlers in index columns | Throws; see [display-defaults](display-defaults.md). |
| View page display fields | The view fields builder has no `AddDetails`, unlike the form fields builder. |
| Localization | Labels are configured through `ConfigureResourceLabels` delegates, without `IStringLocalizer` integration. |
| Per-action authorization | Requirements apply per panel, resource and link; not per action. Also deferred in [dashboard-authorization](archive/dashboard-authorization.md). |
| Sidebar group order | Groups appear where their label is first registered; there is no group order setting. |
| Index content slot | `sa-index-page` has no slot between the page header and the grid, so an override adding a summary has to place it above the header. |
| Global search | The command palette searches navigation labels only, not records. |
