# Many-to-many fields on EF Core resources

Status: proposed (2026-10-18).

## Problem

EF Core resources bind only mapped scalar properties, so a many-to-many navigation such as `Guide.Languages` can't be a field. Today an application needs a form model with a key collection plus create and edit handlers that load and save the join ([relationships reference](../../skills/stellar-admin/references/relationships.md#many-to-many-with-multilookupsheeteditor)).

In the Voyager Travel session (2026-10-18), adding a Languages picker to Guides took about 130 lines: `GuideForm` duplicating the entity's fields and validation attributes, `GuideCreateHandler` and `GuideEditHandler`, and a hand-written duplicate-email check because the custom handlers replaced the built-in save. The picker itself (`MultiLookupSheetEditor`) worked well; the plumbing around it was the cost.

## Goal

A many-to-many navigation can be added as a field directly on the entity's create and edit forms, with the existing multi-select editors (`MultiLookupSheetEditor`, `CheckboxGroupEditor`, `ToggleGroupEditor`) choosing the related records, and `StellarAdmin.EntityFrameworkCore` loading and saving the relationship. No form model or handler is needed for the common case.

Something like this, though the API shape is open:

```csharp
fields.Add(guide => guide.Languages).UseEditor<MultiLookupSheetEditor>(lookup => { ... });
```

## Open questions for the implementing session

- How the field binds: the navigation itself, a key selector, or something else; how keys are read for the edit form and turned back into tracked entities on save.
- Join shapes to support: skip navigations (implicit join table) at least; whether explicit join entities and composite keys are in scope.
- How validation such as `[MinLength]` and `[MaxLength]` applies, and how it behaves with inline editing, view pages and index columns (displaying the related items).
- What happens for custom data sources and form models, which keep working as today.

Read the current binding, field and save code in `src/StellarAdmin` and `src/StellarAdmin.EntityFrameworkCore` first, then propose the public API to Jerrie before implementing. Update the relationships reference, samples, tests and website docs with the change.
