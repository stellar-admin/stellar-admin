# Admin panel backlog

Status: backlog (2026-10-14).

Admin panel features that comparable frameworks (Filament, Nova, Avo, ActiveAdmin, react-admin) offer and StellarAdmin does not. Recorded during the [admin panel skills](admin-panel-skills.md) research so the consumer skill can name them as unsupported. None is scheduled; an item becomes a plan when it is picked up. Source: the API survey of 2026-10-14.

| Feature | Current state |
| --- | --- |
| Custom row and page actions | Row actions are fixed to view, edit and delete. |
| Bulk actions | No row selection or bulk operations on the index. |
| Index filters | Scopes (named tabs) only; no field filters such as select, date range or ternary. |
| Sidebar icons | Resource items and custom links have no icon. Also noted as unscheduled in [custom-sidebar-links](archive/custom-sidebar-links.md). |
| Form tabs and wizards | Forms have sections and groups only. |
| Dashboard and widgets | The home page is a bare landing page with no widget or stat API. |
| File and image uploads | No upload editor or display. |
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
