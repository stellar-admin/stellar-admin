# Field editor catalog

Status: active, 2026-10-01. The spike is committed on the `field-editor-catalog` branch. Phase 1 is implemented and awaits review.

## Goal

Ship a full set of built-in form field editors that `UseEditor<T>` can select, similar to the Filament form field catalog. Editors have friendly names such as `TextInputEditor`, `SelectEditor` and `RadioGroupEditor`, and each accepts editor-specific settings. The MVC editor templates named after data types (`String`, `Int32`, `Currency`, `Boolean`, …) stay, because metadata-based template resolution and per-type app overrides are important. Each piece of editor markup exists once. The data-type templates forward to a friendly editor instead of repeating its markup.

This supersedes item 6, "Richer typed EditorOptions", in [generic-resources-follow-ups](generic-resources-follow-ups.md).

## Starting point

Before this work, `Areas/StellarAdmin/Views/Shared/EditorTemplates/` held 31 templates. 24 of them rendered the same `<sa-input>` line and differed only in `type`, `step` and `asp-format`. Every template repeated the same setup block for the field properties, read-only state and class names. `EnumRadioGroup` and `EnumRadioChoiceCards` duplicated about 30 lines of enum option building, and `Enum` built its items a third way. `UseEditor` could select only `SelectListEditorOptions` and `CheckboxGroupEditorOptions`. `RadioEditorOptions` only configured classes, and the radio templates were selected with `UIHint`.

The numeric data-type templates cannot simply be deleted. MVC tries each candidate name in turn, looking for a view first and then its own built-in template of that name. MVC has built-in templates for `Int32`, `Decimal` and the other numeric types, so without our file it renders its plain input and never falls back to `String`.

## Decisions

### Naming: the configuration class is the editor

The split between a plain configuration object and a request-time, DI-activated component stays. The configuration class takes the friendly `…Editor` name, because that is what users select and read. The runtime half becomes a handler, following ASP.NET Core authorization, where `IAuthorizationRequirement` is data and `AuthorizationHandler<TRequirement>` is the behavior.

```csharp
fields.Add(p => p.CategoryId).UseEditor<SelectEditor>(select => select.UseItems<CategoryItems>());
fields.Add(p => p.Price).UseEditor<TextInputEditor>(input => input.Prefix = "$");

public sealed class SelectEditor : ChoiceEditor, IFieldEditor<SelectEditorHandler>;

public sealed class SelectEditorHandler(SelectEditor editor, IServiceProvider services)
    : ChoiceEditorHandler<SelectEditor>(editor, services);
```

| Before | After |
| --- | --- |
| `EditorOptions` | `FieldEditor` |
| `IFieldEditorOptions` / `IFieldEditorOptions<TEditor>` | `IFieldEditor` / `IFieldEditor<THandler>` |
| `IFieldEditor` / `IFieldEditor<TOptions>` | `IFieldEditorHandler` / `IFieldEditorHandler<TEditor>` |
| `ChoiceItemsEditorOptions` | `ChoiceEditor` |
| `ChoiceItemsEditor<TOptions>` | `ChoiceEditorHandler<TEditor>` |
| `SelectListEditorOptions` / `SelectListEditor` | `SelectEditor` / `SelectEditorHandler` |
| `CheckboxGroupEditorOptions` / `CheckboxGroupEditor` | `CheckboxGroupEditor` / `CheckboxGroupEditorHandler` |
| `RadioEditorOptions` | `RadioGroupEditor`, selectable with `UseEditor` |
| `FormFieldOptions.Editor` / `FormFieldProperties.Editor` typed `EditorOptions` | typed `FieldEditor` |

Most editors load no request data. A `FieldEditorHandler<TEditor>` base class supplies a no-op `PrepareAsync`, so those handlers only declare their template name. Handlers must accept their editor in the constructor, because the controller passes it to `ActivatorUtilities.CreateInstance`.

Smaller names settled in Phase 1: `ISelectListItemsProvider` is removed and `SelectEditor.UseItems<TProvider>()` accepts any `IChoiceItemsProvider`. `IChoiceItemsProvider` and `EditorClassNames` keep their names. `RadioEditorClassNames` becomes `RadioGroupEditorClassNames`. In the EF Core package, `SelectListEditorOptionsExtensions` becomes `SelectEditorExtensions`, `CheckboxGroupEditorOptionsExtensions` becomes `CheckboxGroupEditorExtensions` and `EfCoreSelectListItemsBuilder` becomes `EfCoreSelectItemsBuilder`. `IFieldEditor.HandlerType` replaces `IFieldEditorOptions.EditorType`.

Convenience methods such as `fields.Add(...).UseSelect(...)` are wanted later as sugar over `UseEditor<T>`. They are not part of this plan.

### Two tiers of templates

Friendly editors live in `Areas/StellarAdmin/Views/Shared/EditorTemplates/Editors/`. Their handlers return template names such as `Editors/TextInput`. The `/` means a friendly name can never collide with an application type name during MVC's type-name lookup. `Html.Editor` still finds these templates, so the `UseEditor` path is unchanged. Applications override them at the same path, and `UIHint("Editors/…")` works for editors that need no settings or request data.

Data-type templates stay in `EditorTemplates/` and each forwards with `@await Html.PartialAsync("EditorTemplates/Editors/<Name>", Model, ViewData)`. They must not call `Html.Editor` again, because MVC's `TemplateInfo.Visited` guard renders nothing when the model object is the same instance, for example a string.

Friendly editor templates inherit a shared base view, `FieldEditorView<TEditor>`, that exposes the field properties, typed editor, read-only state and title. This removes the repeated setup block.

Friendly editors infer defaults from `ModelMetadata` in C#, not in the templates. Explicit settings win over inference. For example, `TextInputEditor` on a `decimal` property renders a number input with `step="any"` unless the editor sets another type. This lets many data-type templates be identical forwards.

### Catalog

| Editor | Component | Notable settings |
| --- | --- | --- |
| `TextInputEditor` | `sa-input`, `sa-input-group` | Type (Text, Email, Tel, Url, Password, Number), Placeholder, Prefix, Suffix, Min, Max, Step |
| `TextareaEditor` | `sa-textarea` | Rows |
| `DateInputEditor`, `DateTimeInputEditor`, `TimeInputEditor` | `sa-input`, sharing a base | Min, Max, Step |
| `CheckboxEditor` | `sa-input type=checkbox` | — |
| `ToggleEditor` | `sa-switch` | — |
| `SelectEditor` | `sa-select` | Items, empty option text |
| `RadioGroupEditor` | `sa-radio-group` and fields | Items, `Appearance = Default \| Cards` |
| `CheckboxGroupEditor` | `sa-checkbox-group` | Items |
| `ToggleButtonsEditor` | `sa-segmented-control` or `sa-toggle-group` | Items |
| `SliderEditor` | `sa-slider` | Min, Max, Step |
| `OneTimeCodeEditor` | `sa-input-otp` | Length |

Choice editors share `ChoiceEditor` item sources. An explicit `UseItems(...)` wins. An enum property supplies its members, with display names and descriptions, through one C# helper. A nullable property adds an empty choice.

Out of scope until the base exists: file upload, rich text, Markdown, tags input, key/value, color picker and repeater. None has an `sa-*` component yet.

### Data-type template mapping

| Data-type templates | Friendly editor |
| --- | --- |
| `String`, `Guid`, `Byte`, `SByte`, `Int16`, `UInt16`, `Int32`, `UInt32`, `Int64`, `UInt64`, `Single`, `Double`, `Decimal`, `Currency`, `EmailAddress`, `PhoneNumber`, `Url`, `Password` | `TextInputEditor` |
| `Date`, `DateOnly` | `DateInputEditor` |
| `DateTime`, `DateTimeOffset` | `DateTimeInputEditor` (keep today's text input for `DateTimeOffset` unless a better format is agreed) |
| `Time`, `TimeOnly` | `TimeInputEditor` |
| `MultilineText` | `TextareaEditor` |
| `Boolean` | `CheckboxEditor`, or `SelectEditor` with Yes, No and Not set when nullable |
| `Enum` | `SelectEditor`, or `TextInputEditor` for flags enums |

The current `EnumRadioGroup`, `EnumRadioChoiceCards`, `SelectListEditor` and `CheckboxGroupEditor` templates are decided one by one in Phase 5. They were probably test examples, so each may be removed or kept as a forward.

## Phases

Each phase stops for review before the next starts.

1. **Rename the editor API.** Apply the naming table to the existing types with no behavior change. Add `FieldEditorHandler<TEditor>`. Update tests, fixtures, the samples and the consumer skills reference that mention the old names.
2. **Base view and `TextInputEditor`.** Add `FieldEditorView<TEditor>`, the C# metadata inference and `Editors/TextInput`. Forward every text-like data-type template to it. Replace the spike template, fixture and tests with real ones.
3. **Choice editors.** Add `SelectEditor`, `RadioGroupEditor` with both appearances, `CheckboxGroupEditor` and `ToggleButtonsEditor` under `Editors/`, with the shared enum item helper. Forward `Enum` and nullable `Boolean`.
4. **Remaining editors.** Add `TextareaEditor`, `CheckboxEditor`, `ToggleEditor`, the date and time editors, `SliderEditor` and `OneTimeCodeEditor`. Forward the remaining data-type templates.
5. **Legacy templates and documentation.** Decide on each legacy template. Update the consumer skills reference, samples and any website documentation affected.

Every phase runs the Dashboard HTTP integration tests, the EF Core HTTP integration tests and the Dashboard unit tests, and records the results here.

## Phase 1, 2026-10-01

Applied the naming table with no behavior change. `FieldEditorHandler<TEditor>` exposes the editor as a protected `Editor` property, supplies a no-op `PrepareAsync`, and is the base of `ChoiceEditorHandler<TEditor>`. The select and checkbox group handlers still return the existing `SelectListEditor` and `CheckboxGroupEditor` template names, because the templates move in Phase 3. `RadioGroupEditor` is renamed but not yet selectable with `UseEditor`, because it has no handler or template until Phase 3. After review, the editor classes, `IFieldEditor`, `EditorClassNames` and `RadioGroupEditorClassNames` moved from `Resources/Options` into `Resources/Editors` and the `StellarAdmin.Dashboard.Resources.Editors` namespace, next to their handlers. `FormFieldOptions` stays in `Resources.Options`. Updated the tests, fixtures, DashboardPlayground registrations, the Dashboard skills setup reference and `docs/development.md`.

Verification: the solution builds in Release. Dashboard HTTP integration tests passed 201 of 201, EF Core HTTP integration tests passed 59 of 59 and Dashboard unit tests passed 37 of 37. CSharpier reports the changed C# files as formatted.

## Spike, 2026-10-01

The spike checked the mechanics before any design work. It added `EditorTemplates/Editors/TextInput.cshtml` with metadata inference written inline and a temporary `data-spike-editor` marker. `String.cshtml` and `Decimal.cshtml` became single-line `PartialAsync` forwards. A test fixture editor returned the template name `Editors/TextInput`, and `EditorTemplateSpikeTests` has four HTTP tests.

Results:

- Forwarding through `PartialAsync` with the same `Model` and `ViewData` keeps the field name and id, the `[Display]` label, `data-val-required`, `data-val-range`, the inferred number type and field errors after an invalid post.
- `Html.Editor(field, "Editors/TextInput")` resolves the subfolder template.
- A temporary application file at `Areas/StellarAdmin/Views/Shared/EditorTemplates/Editors/TextInput.cshtml` overrode the library template on both the data-type path and the `UseEditor` path. The temporary file was then removed.
- A nested `@Html.Editor("", "Editors/TextInput")` inside `String.cshtml` rendered nothing for `Name`, with no error. This confirms the `Visited` guard. The file was restored afterwards.
- An editor class without a constructor that accepts its settings object fails with "A suitable constructor … could not be located".

Verification: `dotnet run --project tests/StellarAdmin.Dashboard.IntegrationTests --configuration Release -p:AllowMissingPrunePackageData=true` passed 197 of 197 before the spike and 201 of 201 with the spike in place. The EF Core integration tests and Dashboard unit tests were not run for the spike.
