# Many-to-many fields on EF Core resources

Status: implemented (2026-10-20); index and view display of the related items is deferred.

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

## Decisions (2026-10-20)

- The navigation itself is the field: `fields.Add(guide => guide.Languages).UseEditor<MultiLookupSheetEditor>(...)`. The editor's `UseItems` value must be the related entity's primary key.
- Core gains a public binding hook instead of `InternalsVisibleTo`: `FieldBinder` (`GetValue`, `BindAsync`), `FieldBindingContext` (`Field`, `Model`, `Values`, `Services`, `AddError`) and `FormFieldOptions.Binder`/`UseBinder`. Binders are shared instances that get services from the context.
- `ResourceFieldsBuilder.Add` no longer requires a setter; `FieldBinderOptionsValidator` fails startup for a create or edit field with neither a public setter nor a binder.
- The EF package attaches an internal `ManyToManyFieldBinder` to fields that select a skip navigation (a join table without an entity type). Explicit join entities and related entities with composite keys still fail at startup.
- Supported on create, edit and inline rows, and with save and add another's kept fields. Checkbox group and toggle group work as well as the multi-lookup sheet.
- Index columns and view display fields of the related items are deferred. An index column on the navigation currently renders empty, because the index query includes only references.

## How it works

- Edit pages include the navigation. The binder's `GetValue` returns the related keys, which editor handlers receive and templates render with the property's metadata, so a template sees `List<TKey>` while the label and validation attributes come from the navigation.
- On post, MVC binds the other fields, then each binder runs with the posted values. The many-to-many binder loads only the posted keys missing from the collection, in one `IN` query through the request's context, so they are tracked; unknown keys become field errors. The navigation's validation attributes (`[MinLength]`, `[MaxLength]`) then validate the bound collection.
- The EF data source's update loads the stored entity's collection and adds and removes related entities by key; create needs no change, because the added related entities are already tracked.
- The multi-lookup endpoints convert posted keys to the binder value's element type instead of the property's.

## Verification (2026-10-20)

- `ResourceManyToManyTests` (11 tests): edit rendering with checkbox group and toggle group (checked, not disabled), edit save, clearing, unknown key error, `[MaxLength]` error, create linking existing entities, multi-lookup sheet values and titles, `multilookupselection` conversion, multi-lookup save, inline edit row save.
- `ResourceFieldsBuilderTests`: missing setter fails options validation; a binder passes it.
- Full suites pass: StellarAdmin.Tests 256, StellarAdmin.IntegrationTests 589, StellarAdmin.EntityFrameworkCore.IntegrationTests 108, StellarAdmin.UI.Tests 681, StellarAdmin.UI.IntegrationTests 19. `util/SkillsGenerator --check` reports no drift.
- Not checked in a browser.

## Outstanding

- A sample in the admin playground.
- Index and view display of the related items.
