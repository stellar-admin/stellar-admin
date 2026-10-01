# Field editor catalog

Status: active, 2026-10-01. The spike, Phase 1 and Phase 2 are committed on the `field-editor-catalog` branch. Phase 2b, the editor gallery, is implemented and awaits review.

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
2b. **Editor gallery.** Add the visual harness described in [Editor gallery](#editor-gallery) to DashboardPlayground, with the text input scenarios. Review the prefix and suffix input group in a browser.
3. **Choice editors.** Add `SelectEditor`, `RadioGroupEditor` with both appearances, `CheckboxGroupEditor` and `ToggleButtonsEditor` under `Editors/`, with the shared enum item helper. Forward `Enum` and nullable `Boolean`.
4. **Remaining editors.** Add `TextareaEditor`, `CheckboxEditor`, `ToggleEditor`, the date and time editors, `SliderEditor` and `OneTimeCodeEditor`. Forward the remaining data-type templates.
5. **Legacy templates and documentation.** Decide on each legacy template. Update the consumer skills reference, samples and any website documentation affected.

From Phase 3 on, each phase adds its editors' gallery resource and checks it in a browser. Every phase runs the Dashboard HTTP integration tests, the EF Core HTTP integration tests and the Dashboard unit tests, and records the results here.

## Editor gallery

The HTTP tests guard structure: names, ids, `aria-describedby`, validation attributes and inferred input types. They cannot show layout, spacing or theme styling, and the number of editor scenarios is too large to review by hand without a fixed set of pages. The gallery is that fixed set.

### Scenario axes

Dashboard editors are always model-bound, because every template renders with `asp-for`. Unbound tag helper use belongs to DocsSamples. The gallery varies four axes:

- Property metadata: type, nullability, `[DataType]`, `[Display(Name, Description)]`, `[Required]` and `[Range]`. These decide which template MVC resolves and what the editor infers.
- Editor selection: no `UseEditor`, so the data-type template forwards, or `UseEditor<T>` with explicit settings.
- Field configuration: a `Title` override and read-only.
- State: empty on the create page, with values on the edit page, and with errors after an invalid post.

### Shape

DashboardPlayground gets a "Field editors" sidebar group with one in-memory resource per friendly editor, starting with "Text input". Each resource's model has one property per scenario, and the property's display name describes it, for example `decimal? · [Range] · Prefix $`. Adding a scenario is one property and, when needed, one field registration. Read-only twins sit next to a few properties so both states are visible together.

Each resource uses the real pipeline: resource registration, editor handler preparation, MVC template resolution, the data-type forwards and the configured theme. A page that calls `Html.Editor` with hand-built `FormFieldProperties` would bypass the parts most likely to break, so the gallery does not use one.

- The create page shows empty values, placeholders and inferred types.
- The edit page shows formatted existing values from the in-memory data source.
- Saves go through real model validation, and a valid save succeeds without storing anything. A "Gallery options" section at the top of every gallery form has two bound checkboxes. "Reject every field" makes a valid save fail with an error on every field, to show each editor's error state at once. "Skip client validation" turns off the browser's constraint checks for the next submit, so invalid values such as a malformed email reach server validation. Gallery records are never changed.

Browser checks run on port 5206 and the server is stopped afterwards. Port 5205 is Jerrie's. DashboardPlayground no longer calls `dashboard.RequireAuthorization()`, so the gallery opens without signing in. The Users and Roles resources keep their own authorization policies.

### Pixel comparison, not yet decided

`util/visual-regression/vrt.mjs` compares base and head screenshots, which would turn "the forwards still render the same" into a zero pixel diff for Phases 3 to 5. It cannot capture the gallery yet. It discovers pages from `docs/DocsSamples/Pages`, sets the theme with a `?theme=` query parameter, and DashboardPlayground sets its theme in `ConfigureTheme`. Using it needs an explicit page list option, and either one capture per theme or a theme switch the tool can drive. This is a separate decision after Phase 2b and is not part of it.

## Phase 2b, 2026-10-01

Added the gallery in `sandbox/DashboardPlayground/Resources/FieldEditors`. `FieldEditorGalleryDataSource<TRecord>` serves one sample record, from the record type's static `CreateSample()` declared by `IFieldEditorGalleryRecord<TSelf>`. Saves pass through real model validation and succeed without storing anything. The shared `FieldEditorGalleryRecord` base class adds `Id` and the two gallery options, which the registration places in a "Gallery options" section ahead of each gallery's scenarios. With `RejectEveryField` on, the data source rejects a valid save with a summary message and one error per scenario property. `wwwroot/js/field-editor-gallery.js`, linked with `AddScript`, sets the form's `noValidate` from `SkipClientValidation` when a submit button is clicked. `StyledCode` gained `[StringLength(6, MinimumLength = 6)]` so an input group can show a real error. `FieldEditorGalleryRegistration.AddFieldEditorGallery()` registers each gallery resource under the "Field editors" sidebar group with create and edit sharing one field configuration. `TextInputGallery` at `/stellaradmin/text-input` has 27 scenarios in three sections: data-type templates without `UseEditor`, field configuration (`Title` override and `[Editable(false)]`, including a read-only prefix), and `TextInputEditor` settings (placeholder, explicit type, min/max/step, prefix, suffix, both with description and validation, and `ClassNames.Control`). Removed `dashboard.RequireAuthorization()` from the playground.

The browser review found one defect. The grouped layout rendered the description before the error, while `sa-input` uses `FieldLayout.Stacked`, which renders the error first. `Editors/TextInput.cshtml` now renders the error first. A read-only input inside an input group keeps the group's normal background, while a plain read-only input is shaded. This is input group styling and was left as is.

Verification: the solution and DashboardPlayground build in Release. Dashboard HTTP integration tests passed 212 of 212, EF Core HTTP integration tests passed 59 of 59 and Dashboard unit tests passed 37 of 37. CSharpier reports the gallery files as formatted. Headless Chromium captured the create page, the edit page and the edit page after a rejected save at 1280 pixels wide in the Parallax theme, against a scratch copy of `app.db` on port 5206. The server was stopped afterwards. Other themes and narrow widths were not checked. After the gallery options were added, a second headless run checked five paths on the edit and create pages. The browser blocked an invalid email when client validation was on. With it skipped, the server returned the real `[EmailAddress]`, `[Phone]`, `[Range]` and `[StringLength]` messages, including inside the prefix input group, and the checkbox stayed checked. An unchanged save redirected to the index. Reject every field produced 27 field errors plus the summary. A blank create returned the `[Required]` and `[Range]` errors. The DashboardPlayground was started with `ASPNETCORE_ENVIRONMENT=Development` so library assets resolve from `bin`.

Field description, added during the Phase 2b review: `ResourceFieldBuilder.Description` sets a field's help text next to `Title`, as a field setting rather than an editor setting, so it also applies without `UseEditor`. It flows through `FormFieldOptions.Description` and `FormFieldProperties.Description` to `FieldEditorView.Description`, which is null when the property's `[Display(Description)]` should be used. `TextInput.cshtml` passes it to `sa-input` and, in the prefix and suffix layout, to `sa-field-description` and the `aria-describedby` id. The older data-type templates (Boolean, Date, Enum, MultilineText and the rest) ignore it until they move onto `FieldEditorView` in Phases 4 and 5. Two gallery scenarios cover a field description and one that replaces an attribute description inside a prefix group. Two integration tests cover the plain and the prefix layouts. Dashboard HTTP integration tests passed 214 of 214, EF Core HTTP integration tests 59 of 59 and Dashboard unit tests 37 of 37. The rendered gallery edit page showed both field descriptions, no attribute description, and the description id in `aria-describedby`. That check read the HTML only and took no screenshot.

## Phase 2, 2026-10-01

Added `TextInputEditor`, `TextInputEditorHandler` and the `TextInputType` enum (Text, Email, Tel, Url, Password, Number) in `Resources/Editors`. The editor's settings are `Type`, `Placeholder`, `Prefix`, `Suffix`, `Min`, `Max` and `Step`. `Min`, `Max` and `Step` are `decimal?` and render with the invariant culture. An internal `ResolveAttributes(ModelMetadata)` method on the editor does the inference in C#. An explicit `Type` wins. Otherwise the `EmailAddress`, `PhoneNumber`, `Url` and `Password` data types map to their input types, numeric properties become `number`, and everything else is `text`. A number input gets the explicit `Step`, or `1` for integer types and `any` for `decimal`, `double` and `float`. The type is always emitted, because an empty `type` attribute on `sa-input` is copied as `type=""` by the framework input helper.

`FieldEditorView<TEditor>` in `Areas/StellarAdmin` is a public `RazorPage<object?>` base for friendly editor templates. It exposes `Field`, `Editor`, `EditorData`, `IsReadOnly` and `Title`. A field without a selected editor gets a new `TEditor` carrying the field's class names. Any other editor type fails rendering with an `InvalidOperationException`.

`Editors/TextInput.cshtml` renders `sa-input` without a prefix or suffix. With either, it composes `sa-field`, `sa-field-label`, `sa-input-group` with `sa-input-group-input` and the add-ons, `sa-field-description` and `sa-field-error`, because the input group has no field wrapper. In that layout the template sets `aria-describedby` to `<id>-description` and `<id>-error` itself, and `ClassNames.Control` styles the input group. All 18 text-like data-type templates from the mapping table are now one-line forwards. The spike marker, `SpikeTextInputEditor` and `EditorTemplateSpikeTests` are removed. `TextInputEditorTests` replaces them, with a `TextInputFieldsModel` fixture covering currency, string with description, email, `Guid`, nullable `long`, phone, `int`, `double` and URL properties.

Verification: the solution builds in Release. Dashboard HTTP integration tests passed 212 of 212, EF Core HTTP integration tests passed 59 of 59 and Dashboard unit tests passed 37 of 37. CSharpier reports the changed C# files as formatted. No browser check was run, so the input group's appearance in a form is unverified.

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
