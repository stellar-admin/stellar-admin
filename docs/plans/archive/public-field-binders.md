# Public field binders

Status: completed (2026-11-10). Field binders are internal.

`FieldBinder` and `UseBinder()` were public, so an app could read and bind a field with its own code, such as a property without a public setter. Inside StellarAdmin, the EF Core data source uses them for many-to-many fields (`ManyToManyFieldBinder`).

The only way for an app to attach its own binder was `PostConfigure` on `ResourceOptions<TResource>`, looping over the create and edit fields. No builder method exposed it. The website docs section on field binders was removed from the form models page on 2026-11-05 (website `b91aceca`). The consumer reference (`skills/stellar-admin/references/forms.md`) and the namespace table in `skills/stellar-admin/SKILL.md` still mentioned `FieldBinder` until this work.

## Decision

Make field binders internal for now (2026-11-10). They can be made public later if a need arises; that doesn't break anyone, while taking them away would.

- Binders came with the many-to-many fields (`d914b7c`, 2026-10-20). They were made public only so the EF Core package, a separate assembly, could attach one: the many-to-many plan chose "a public binding hook instead of `InternalsVisibleTo`" ([ef-many-to-many-fields](ef-many-to-many-fields.md)). No app use case was designed.
- A binder translates between the editor's value and a property of another shape, such as the keys a checkbox group posts and a collection of tracked entities, without a separate form model. Apps get the same result with a form model whose property has the editor's shape: a core form model with a handler (`AllowEdit<TModel, THandler>`), or an EF Core form model that wraps the entity and converts in `OnLoaded` and `OnUpdating`.
- The one case without a good alternative is a third-party data source package that attaches binders as the EF Core package does. If that becomes a goal, binders return as a deliberate extension point with a builder method, not through `PostConfigure`.
- Cost: the EF Core package uses internals of `StellarAdmin`, so the two ship at matching versions. They're released together already.

## Steps

- [x] Make `FieldBinder`, `FieldBindingContext`, `FormFieldOptions.Binder` and `FormFieldOptions.UseBinder` internal, removing their XML comments ([XML documentation](../../conventions/xml-documentation.md)).
- [x] Add `InternalsVisibleTo` for `StellarAdmin.EntityFrameworkCore` in `src/StellarAdmin/StellarAdmin.csproj`.
- [x] Remove their entries from `src/StellarAdmin/PublicAPI.Unshipped.txt`.
- [x] Remove the `FieldBinder` cref from the remarks of `ResourceFieldsBuilder.Add` (`src/StellarAdmin/Resources/Builders/ResourceFieldsBuilder.cs`), keeping the note that EF Core binds many-to-many navigations without a setter.
- [x] Change the fix text of the `FieldWithoutSetter` problem (`src/StellarAdmin/Resources/Options/FieldBinderCheck.cs`) from binding with a `FieldBinder` to using a form model.
- [x] Replace the "Fields bound by code" section of `skills/stellar-admin/references/forms.md` with a short note on form models for properties without a setter or of another shape, and drop `FieldBinder` from the namespace table in `skills/stellar-admin/SKILL.md`.
- [x] Keep `ResourceFieldsBuilderTests.Add_WithoutSetterAndWithBinder_PassesOptionsValidation`; the tests already see internals.
- [x] Verify: build, the StellarAdmin and EF Core tests, and the skills check.

The website has no binder docs left (searched `content/` and `src/` on its `master`, 2026-11-10). The fields reference's read-only note named binders too; it now names EF Core many-to-many navigations.

## Verification (2026-11-10)

- Release build of `StellarAdmin.slnx`: succeeded, no new warnings.
- `dotnet test --solution StellarAdmin.slnx --configuration Release`: 2126 passed, 0 failed.
- `stellar skills check`: 67 references up to date.
- CSharpier check of the changed C# files and `git diff --check`: clean.
