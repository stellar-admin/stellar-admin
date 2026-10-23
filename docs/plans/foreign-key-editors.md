# Foreign-key editors

Status: approved (2026-10-23), in progress. Stage C of [configuration simplification](configuration-simplification.md), on the `configuration-simplification` branch. Builds on [resource references](archive/resource-references.md).

## Goal

On an EF Core entity form, a foreign key or many-to-many field whose related type is a registered resource needs no editor call. It gets a lookup sheet, or a multi-lookup sheet for a many-to-many navigation, with its items from the related resource. Explicit configuration is only for a different editor or other settings.

## Decisions (Jerrie, 2026-10-23)

1. The default is always a sheet editor (`LookupSheetEditor` or `MultiLookupSheetEditor`), because they search and page, so they suit lists of any size, and choosing them costs no row-count query. A small list that suits a select names `UseEditor<SelectEditor>()`. A default chosen by the related resource's `Reference(...)` can come later if small lists turn out to be common.
2. A foreign key whose related type isn't a registered resource keeps its property's default editor (a number input), and reports Info. It's often deliberate, so it isn't a warning.
3. A many-to-many field whose related type isn't a registered resource is left out of the form with a warning, as today. Approved with the plan.
4. The Products sample's edit form shows the default; the create form keeps its configured sheet. Approved with the plan.

## API

No new public API. A field with no editor now gets one:

```csharp
// Before
fields.Add(product => product.CategoryId).UseEditor<LookupSheetEditor>();
fields.Add(product => product.Tags).UseEditor<MultiLookupSheetEditor>();

// After: a lookup sheet and a multi-lookup sheet, with items from the Category and Tag resources
fields.Add(product => product.CategoryId);
fields.Add(product => product.Tags);

// Unchanged: another editor, or the sheet's settings
fields.Add(product => product.CategoryId).UseEditor<SelectEditor>();
fields.Add(product => product.CategoryId).UseEditor<LookupSheetEditor>(lookup => lookup.EnableCreate());
```

- `FormFieldOptions.InferResource(Type)` keeps its signature. Its remarks change: a field with no editor also gets a sheet editor for the resource, when the resource is registered.

## Behaviour

- **Where it applies:** fields on EF Core entity forms (create, edit and their inline rows) with no editor, a single-property foreign key or a many-to-many navigation, and a related type with a registered resource. Form models and custom data sources aren't affected; they name their related resource with `UseResource<T>()` as today.
- **Which editor:** a single value gets `LookupSheetEditor`; a collection gets `MultiLookupSheetEditor`. Both use their defaults: no create button, the default sheet title and layout.
- **Label:** the field is labelled with the related resource's singular label, as fields with a configured lookup are since Stage B.
- **Unregistered foreign key:** the field keeps the number input, and reports Info naming the fix: register a resource for the related type, or configure an editor with `UseItems`.
- **Unregistered many-to-many:** the field has no default editor to fall back to, so it's left out of the form as it is today, with a warning. The fix also names registering a resource for the related type.
- **Unavailable related resource:** a resource that's registered but unavailable at render (an EF Core mapping problem, say) gives the same result as a configured lookup since Stage B: no items, and that resource's problem.

## Implementation

- **Where the editor is chosen.** The EF Core PostConfigure can't tell whether a type has a resource, because registrations are internal to core. Core's options factory can: `ResourceOptionsFactory.Check` already receives the registered resource types and runs after every PostConfigure. So the EF Core package marks the field, and core picks the editor.
- **EF Core side.** `InferResource` stores the related type on a field that has no editor (an internal property). `InferReferencedResources` runs before `UseManyToManyBinders`, and the `SingleValueManyToManyEditor` check accepts a field with no editor and an inferred resource.
- **Core side.** A new check before `EditorCheck` gives each field with an inferred resource and no editor its sheet editor when the type is registered. Otherwise it reports Info and leaves the field alone, or removes a field a binder binds (the many-to-many case) with a warning, as `EditorCheck` does for binder fields.
- **Inline rows.** The grid has a lookup sheet template but no multi-lookup one. Step 1 checks what an inline row does with a many-to-many field today; a field the grid can't edit keeps today's behaviour.

## Steps

1. EF Core marking, the core check, and the configuration problems, with tests.
2. Samples and consumer references.
3. Verification and this plan's record.

Checkpoint with Jerrie after step 1.

## Progress

- **Step 1 (2026-10-23, not committed).** `InferResource` stores the related resource on a field with no editor (internal `FormFieldOptions.InferredResource`). The new `InferredEditorCheck` runs before `EditorCheck` and gives such a field a `LookupSheetEditor` or `MultiLookupSheetEditor` with its handler, when the resource is registered. Otherwise it reports `InferredEditorUnavailable` (event 1066): Info for a field that keeps its default editor, and a warning for a binder field it leaves out. The many-to-many field of an inline row is also left out with a warning, since the grid has no multi-lookup editor. The EF Core PostConfigure runs `InferReferencedResources` before `UseManyToManyBinders`, which now accepts a many-to-many field with no editor. Tests: foreign key and many-to-many fields with no editor (rendering, the label, saving), without a resource (number input with Info, many-to-many left out with a warning), and an inline edit row. `Edit_WithSingleValueEditor_LogsProblem` lost its no-editor case. Release: 147 EF Core integration tests, 664 admin panel integration tests and 308 admin panel unit tests pass.

## Samples

- **Products:** the edit form's Category becomes `fields.Add(product => product.CategoryId)`, to show the default. The create form keeps `UseCategories` (sheet title, search placeholder and New), and both Tags fields keep their editors.

## Tests

- EF Core integration tests (`tests/StellarAdmin.EntityFrameworkCore.IntegrationTests/Resources`): a foreign key and a many-to-many field with no editor render a lookup sheet and a multi-lookup sheet with the related resource's items, on create, edit and an inline row, and save. An unregistered foreign key renders the number input and reports Info. An unregistered many-to-many field is left out with a warning. An explicit editor wins.
- `ResourceManyToManyTests` expects the `SingleValueManyToManyEditor` problem for a many-to-many field with no editor; that case changes. Any test that expects a number input for a foreign key with no editor changes too.

## Documentation

- Consumer references: `skills/stellar-admin/references/relationships.md` and `editors.md` show foreign keys without an editor call, and when to name one. Regenerate with `util/SkillsGenerator`.
- Website docs: batched into the configuration simplification follow-ups.

## Open

- Built-in array and dictionary editors, for quickly entering a list of items or key-value pairs, get their own plan later (Jerrie, 2026-10-23). An array editor could then be the default for a many-to-many field whose related type isn't a resource.

- There's no way to keep a number input for a foreign key to a registered resource, since no built-in editor renders one. Not needed unless someone asks.
