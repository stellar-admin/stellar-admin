# Default field labels

Status: completed (2026-11-12). From task `h89z`.

## Goal

Every field of a form, view page or display field shows one name, its label, in the label itself and in every message about it: validation messages and model-binding messages. The label is the field's `Title`, else `[Display(Name)]`, else the related resource's label for a foreign key, else the property name split like index headers ("Departure Date"). This applies at every depth of a field path, and it doesn't change the metadata of the app's own models.

## Why not a metadata provider

MVC builds a property's metadata once per type and caches it for the whole app. Labels are per field, so a type-wide name can't carry `Title` or a foreign key's label. Two fields on the same property of the same type (`Billing.Street`, `Shipping.Street`) can have different titles. A provider scoped to some types would also depend on which page first built a type's metadata.

## Approach: metadata per form, per field path

StellarAdmin binds and validates each form with a tree of metadata of its own. The root wraps the form model's metadata, and each property wraps the framework's metadata for that property, with `DisplayName` set to the field's label for its path. Everything else is passed through to the framework's metadata, so validators, binders and the binder cache work as before. The framework's metadata cache isn't touched.

Spike (2026-11-12, throwaway unit test with `AddMvcCore().AddDataAnnotations()` and a real `ControllerBase.TryUpdateModelAsync`; deleted afterwards). It set the controller's `MetadataProvider` to return the wrapped root for the form model's type, and its `ObjectValidator` to call `ObjectModelValidator.Validate(…, model, root)`. Results:

- `[Required]` with a `Title`: "The Lead passenger field is required."
- Conversion failure, split default: "The value 'abc' is not valid for Departure Date."
- `[Range]`: "The field Passenger Count must be between 1 and 10."
- Foreign key label: "The value 'x' is not valid for Category."
- Same nested property under two paths: "The Billing street field is required." and "The Shipping street field is required."
- Non-nullable reference type (the required rule MVC adds itself): "The Reference Code field is required."
- The framework's metadata for the same property still had no display name ("DepartureDate").

Model-binding messages without a name ("The value '' is invalid.") stay as MVC writes them.

## Public API

- `StellarAdmin.UI.DisplayNames.FromPropertyName(string name)`, which returns `"Departure Date"` for `"DepartureDate"`. It's the former `SplitPascalCase` of `DataGridColumnTagHelper`, which calls it now.

## What was built (2026-11-12)

- `FormMetadata` (internal, `Areas/StellarAdmin/FormMetadata.cs`) derives from `DefaultModelMetadata` instead of forwarding every member. Each node is a new `DefaultModelMetadata` built from the framework's binding, display and validation metadata, attributes, accessors and identity (read through the protected `Identity` property), and overrides only `DisplayName` and `Properties`.
- Only the properties on a configured field's path get a node; the others, and everything below a field, are the framework's metadata. Wrapping every property overflowed the stack in MVC's binder factory on EF Core entities whose navigations lead back to their own type, since every path was a new node.
- A form's root is built once per form (keyed by its field list), so the binder factory, which caches by root, adds one entry per form rather than one per request.
- `ResourceController.TryBindConfiguredFieldsAsync` binds through `TryUpdateFormModelAsync`, which hands MVC the form's root for that call and validates with it. Kept-field and dependency binding clear their errors, so they keep the framework's metadata.
- Rendering passes labels explicitly: form and grid editors get `FieldLabels.Get` as their `Title` (it was `null` when the metadata named the field); display fields and view pages get `FieldLabels.Get(displayField, metadata)`; image displays use the field's or column's label as alt text.
- Collections without a `Title` or `[Display(Name)]` now read as their split property name ("Category Ids").

## Verification (2026-11-12)

- Unit: `FormMetadataTests` (node answers like the framework's metadata apart from `DisplayName`, which fails if a future version adds a detail the copy misses; same property under two paths; properties off field paths stay the framework's; framework metadata unnamed), `DisplayNamesTests`. StellarAdmin.Tests 358/358, StellarAdmin.UI.Tests 815/815.
- Integration: new `FieldLabelTests` (labels for `Title`, split default, resource label, nested paths; messages for required, conversion, range, implicit required, foreign key and nested paths; display field label). StellarAdmin.IntegrationTests 759/759 after updating 9 tests that asserted raw names, EF Core 170/170, UI 19/19, ScenarioTesting.Tests 23/23.
- `skills check`: 67 references up to date.

## Steps

- [x] `DisplayNames.FromPropertyName` in StellarAdmin.UI, with unit tests and an entry in `PublicAPI.Unshipped.txt`. The grid header default uses it.
- [x] `FieldLabels.Get` falls back to the split name instead of the metadata's name.
- [x] Internal `FormMetadata`, a `DefaultModelMetadata` subclass (no forwarding wrapper needed), with the guard test.
- [x] Forms bind and validate with the form's root in `TryBindConfiguredFieldsAsync`. `CollectionEditorValidator` and `ValidateBoundField` already used `FieldLabels.Get`.
- [x] Rendering: labels passed explicitly to editors, display fields and image alt text.
- [x] View pages and display fields: `Title`, else `[Display(Name)]`, else the split name.
- [x] Integration tests added and updated.
- [x] Consumer skills: `SKILL.md`, `references/forms.md`, `references/columns.md`, `references/relationships.md`.
- [x] Website text: `fields.mdx` (label default; the advice to prefer `[Display]` because `Title` didn't reach messages is replaced), `quick-start.mdx` callout.
- [x] Docs screenshots: regenerated all 121 specs. 15 images changed, all from animation timing or date-dependent data; none shows a changed label, because the docs samples use `[Display(Name)]` or one-word properties. The regenerated images were restored, so the website keeps its committed screenshots.

## Risks

- `FormMetadata` copies the framework's metadata details; if a future ASP.NET Core version adds one, the guard test fails.
- An app that replaces MVC's `IObjectModelValidator` with one that isn't an `ObjectModelValidator` keeps the framework's names in validation messages.
- Messages that a validation library writes itself, such as FluentValidation's, use that library's names (`.WithName(...)`, else its own split of the property name).
