# Typed editors and lookup sources

Status: completed (2026-10-23). Stages A and D of [configuration simplification](../configuration-simplification.md#a-fields-keep-their-types-each-editor-gets-its-own-method), committed in `92e1638`, `5703182` and the commit that closes this plan, on the `configuration-simplification` branch, which isn't merged yet.

## Goal

Every editor call reads without type arguments, apart from naming an entity, a resource or a source class once. Each built-in editor gets its own method on the field builder, and the editors that take items know the form's type, so loaders, filters and source arguments infer their parameters. Items for records that aren't a resource come from a source class that owns their value, title and media.

## API

```csharp
// Before
fields.Add(trip => trip.CityId)
    .DependsOn(trip => trip.CountryId)
    .UseEditor<SelectEditor>(select => select.UseItems(
        (Trip trip, IServiceProvider services, CancellationToken cancellationToken) => ...));
fields.Add(trip => trip.CityId)
    .UseEditor<SelectEditor>(select => select.UseResource<City>(items => items.Where<Trip>((city, trip) => ...)));
fields.Add(trip => trip.ArrivalAirport)
    .UseEditor<LookupSheetEditor>(lookup => lookup.UseItems<TripAirportSource, TripAirport, string, int, Trip>(
        airport => airport.Code, airport => airport.Name, trip => trip.CountryId!.Value,
        items => items.UseCode(airport => airport.Code)));
fields.Add(form => form.CountryId)
    .UseEditor<SelectEditor>(select => select.UseItems<Country, int>(country => country.Id, country => country.Name));
fields.Add(tour => tour.Nights).UseEditor<SliderEditor>(slider => slider.Step = 1);

// After
fields.Add(trip => trip.CityId)
    .DependsOn(trip => trip.CountryId)
    .UseSelectEditor(select => select.UseItems((trip, services, cancellationToken) => ...));
fields.Add(trip => trip.CityId)
    .UseSelectEditor(select => select.UseResource<City>(items => items.Where((city, trip) => ...)));
fields.Add(trip => trip.ArrivalAirport)
    .UseLookupSheetEditor(lookup => lookup.UseSource<TripAirportSource>(trip => trip.CountryId!.Value));
fields.Add(form => form.CountryId)
    .UseSelectEditor(select => select.UseItems<Country>(country => country.Id, country => country.Name));
fields.Add(tour => tour.Nights).UseSliderEditor(slider => slider.Step = 1);

// The source owns its value, title and presentation
public sealed class TripAirportSource : LookupSource<TripAirport, string, int>
{
    public override string GetValue(TripAirport airport) => airport.Code;
    public override string GetTitle(TripAirport airport) => airport.Name;
    public override string? GetCode(TripAirport airport) => airport.Code;
    public override Task<IReadOnlyCollection<TripAirport>> FindAsync(IReadOnlyCollection<string> values, CancellationToken cancellationToken) => ...;
    public override Task<LookupPage<TripAirport>> SearchAsync(int countryId, LookupQuery query, CancellationToken cancellationToken) => ...;
}
```

### New

- **Editor methods on `ResourceFieldBuilder<TResource>`**, one per built-in editor, each with a configure overload and one without. Each returns the field builder, so `DependsOn` and the editor chain in any order.
  - Each is named after its editor class (decision 4).
  - With items, passing an editor typed by the form: `UseSelectEditor`, `UseRadioGroupEditor`, `UseSegmentedControlEditor`, `UseCheckboxGroupEditor`, `UseToggleGroupEditor`, `UseLookupSheetEditor` and `UseMultiLookupSheetEditor`.
  - Without items, passing today's editor: `UseCheckboxEditor`, `UseToggleEditor`, `UseSliderEditor`, `UseTextInputEditor`, `UseTextareaEditor`, `UseDateInputEditor`, `UseDateTimeInputEditor`, `UseTimeInputEditor`, `UseOneTimeCodeEditor`, `UseRichTextEditor`, `UseIconEditor` and `UseImageEditor`.
- **Form-typed editors:** `SelectEditor<TModel>`, `RadioGroupEditor<TModel>`, `SegmentedControlEditor<TModel>`, `CheckboxGroupEditor<TModel>`, `ToggleGroupEditor<TModel>`, `LookupSheetEditor<TModel>` and `MultiLookupSheetEditor<TModel>`, each derived from today's editor. They add:
  - `UseItems(Func<TModel, IServiceProvider, CancellationToken, Task<IReadOnlyList<ChoiceItem>>>)` on the choice editors, replacing `UseItems<TModel>`;
  - `UseResource<TResource>(Action<ResourceItemsBuilder<TModel, TResource>>)`, whose builder's `Where((record, form) => ...)` takes no type argument;
  - `UseSource<TSource>()` and `UseSource<TSource>(Expression<Func<TModel, object?>> argument)` on the two lookup sheets.
- **`LookupSource<TItem, TValue>` and `LookupSource<TItem, TValue, TArgument>`** in `StellarAdmin.Resources.Editors`, on a non-generic `LookupSource` base that `UseSource` constrains to: abstract `GetValue`, `GetTitle`, `FindAsync` and `SearchAsync`, and virtual `GetDescription`, `GetAvatar`, `GetCode`, `GetIcon` and `GetImage`. The media is the one media method the source overrides, read from its type, so the sheet knows its layout before items load.
- **`ChoiceItems`**, an abstract class integrations derive from to supply choices with a value type (`ChoiceEditor.UseItems(ChoiceItems)`), replacing the EF Core package's use of `UseItems<TModel>`. `LookupItems` gains a virtual `ValueType` for the same check.

### Changed

- The non-generic `SelectEditor`, `RadioGroupEditor`, `SegmentedControlEditor`, `CheckboxGroupEditor`, `ToggleGroupEditor`, `LookupSheetEditor` and `MultiLookupSheetEditor` get internal constructors and are no longer sealed, so only the form-typed editors derive from them. (Abstract classes, as planned, broke the editor templates, which create a default editor for a field without one; `FieldEditorView<TEditor>` drops its `new()` constraint and creates it through the internal constructor.) They keep their settings and the items that don't need the form (`UseItems(IEnumerable<ChoiceItem>)`, `UseItems<TProvider>()`, `UseItems(LookupItems)`), and handlers, views and `is` checks keep using them. `UseEditor<SelectEditor>` no longer compiles; `UseSelectEditor` replaces it.
- EF Core `UseItems<TEntity, TValue>` (six overloads) becomes `UseItems<TEntity>(Expression<Func<TEntity, object?>> value, Expression<Func<TEntity, string>> title, ...)`. The value's type is read from the expression and checked against the field when the options resolve (decision 2).
- `EfCoreChoiceItemsBuilder<TEntity, TValue>` and `EfCoreLookupItemsBuilder<TEntity, TValue>` lose `TValue`. Their `Where<TModel>` and `ReferenceFrom<TModel>` keep the form's type argument (decision 2).
- `ResourceItemsBuilder<TResource>` becomes `ResourceItemsBuilder<TModel, TResource>`, and `Where<TModel>` becomes `Where`.

### Removed

- `ChoiceEditor.UseItems<TModel>(loader)`, now on the form-typed editors.
- `UseItems<TSource, TEntity, TValue>` and `UseItems<TSource, TEntity, TValue, TArgument, TModel>` on both lookup sheets, two overloads each, replaced by `UseSource`.
- `ILookupSource<TEntity, TValue>` and `ILookupSource<TEntity, TValue, TArgument>`, replaced by the base classes (decision 3).

No obsolete overloads are kept: the package is a preview and none of this has shipped (`PublicAPI.Shipped.txt` is empty).

### Unchanged

- `UseEditor<TEditor>` stays for custom editors. It keeps compiling for the built-in editors without items, but the docs teach the methods.
- `DependsOn`, `UseItems<TProvider>()` (`IChoiceItemsProvider`), `UseResource<T>()` without a filter, and `EnableCreate`.
- Displays (decision 5): `UseDisplay<TDisplay>` and `BadgeDisplay.UseText<TItem>` already take one type argument.

## Why the form's type and not the property's

The stage A sketch typed the field builder by its property too (`ResourceFieldBuilder<TResource, TProperty>`), so item values would be checked at compile time. A spike against the pinned SDK (C# 14) showed what each shape can do:

- **Partial type arguments:** C# can't infer some type arguments and take others explicitly, and that includes C# 14 extension blocks. An EF Core extension on `SelectEditor<TModel, TProperty>` with `UseItems<TEntity>` can't be called as `UseItems<Country>(...)`. Only instance methods on a generic class can, and the EF Core package can't add those to core types. EF Core items therefore extend the non-generic editor, and with a one-argument call the value's type can't be a type parameter.
- **Collections:** the element type of `List<int>` can be found through a covariant interface (`IFieldBuilder<TModel, out TProperty>`), but:
  - a many-to-many navigation (`ICollection<Tag>`) takes key values, not `Tag`;
  - `ToggleGroupEditor` edits both single values and collections, and a scalar overload always beats the collection one;
  - `string` matches as a collection of `char`.
- **The form type alone** removes every type argument in the examples above except the entity, resource or source name. The value type is checked when options resolve, as every other mismatch already is.

The spike compiled each "After" call above: `DependsOn` chaining, inferred loader parameters, `Where((city, trip) => ...)`, `UseSource<T>(trip => trip.CountryId!.Value)` with the argument's type readable from the expression, and the one-argument EF Core `UseItems<Category>` on an `int?` field and on a `List<int>` field.

## Behaviour

- **Value checks.** When options resolve, a value selector or source whose value type isn't the field's type is a configuration problem. Nullable and underlying types match (`int` items for an `int?` field), and so do element types for multiple values. Such a field keeps its default editor, as `MismatchedHandler` does today, and the problem reuses `UnsuitableEditor`. A field a binder binds (a many-to-many navigation) isn't checked, since it holds entities and binds the items' keys.
- **Source arguments.** The argument expression's type (with the boxing conversion removed) must be the source's `TArgument` or its nullable form, and is checked the same way; a null value at search time throws, since `DependsOn` keeps the sheet from searching while a parent is empty. A source with an argument used without one, or a source without an argument given one, is a problem too.
- **Sources** are resolved from DI per request, or created with the request's services when the app didn't register them (`ActivatorUtilities`). Editor configuration runs when options resolve, after the service collection is built, so `UseSource` can't register them as `UseDataSource` does.
- **Media.** A source overriding more than one media method is a configuration problem, and the field keeps its default editor.
- **Inferred editors** (stage C) create the form-typed lookup sheets for the resource's form type.

## Decisions (Jerrie, 2026-10-23)

Jerrie accepted 1, 2, 3 and 5 as recommended, and changed 4.

1. **Form type only.** Editors carry the form's type, not the property's, and item values are checked when options resolve rather than at compile time (see above). The alternative was the property-typed builder, with compile-time value checks for scalar fields and the collection limits above.
2. **EF Core items for entities that aren't resources.** (a) They stay an EF Core extension: `UseItems<Country>(...)` takes one type argument, and `Where<Trip>` and `ReferenceFrom<Booking>` on its builder keep the form's type. (b) Core gets an "entity items" concept that EF Core implements, so `Where((city, trip) => ...)` drops `<Trip>` here as well. That adds a core API that does nothing without an integration package. Chosen: (a). No playground resource uses these items any more (B and C replaced them), so only tests and one consumer reference example keep `Where<Trip>`.
3. **`ILookupSource` interfaces.** Removed in favour of the base classes, so a lookup sheet has one way to take a source.
4. **Method names.** `Use` plus the editor's class name: `UseSelectEditor`, `UseLookupSheetEditor`, `UseIconEditor`. Jerrie's change to the proposed `UseSelect`: the method matches the class it creates, has no exceptions, and can't collide with the items builders' `UseIcon`, `UseImage` or `UseResource`.
5. **Displays stay as they are.** Columns and display fields keep `UseDisplay<TDisplay>`. Per-display methods (`UseBadgeDisplay`, `UseNumberDisplay`) would be for consistency only, with no type argument removed; they are left for a later stage, if wanted.

## Steps

Each step is committed separately, after Jerrie's go-ahead.

1. **Core editors and sources.** The editor methods, form-typed editors, `ResourceItemsBuilder<TModel, TResource>`, `LookupSource` base classes and `UseSource`, value and argument checks, inferred editors, removal of the replaced members, and the EF Core `UseItems<TEntity>` change. Public API files, the integration tests (about 230 `UseEditor` call sites, 20 EF Core and 18 source `UseItems` calls) and new tests for the checks and sources. The playground is moved only as far as the build needs.
2. **Playground, consumer references and docs.**
   - Playground: about 230 `UseEditor` call sites, and the Trips, gallery and form layout sources. The gallery's airport lookups use `UseResource<GalleryAirport>()`, because `GalleryAirport` is a resource and its separate lookup source repeats its data source.
   - Consumer references: `skills/stellar-admin` (SKILL.md and the editors, relationships, forms and inline editing references, 28 `UseEditor` examples) and `skills/stellar-admin-setup`. The `build/skill-evals` task checks that name `UseEditor<LookupSheetEditor>` or `UseItems<Customer, int>`.
   - Maintained docs: `docs/development.md`.
   - A smoke run of the playground on port 5207.
3. **Close the plan.** Record verification, archive it, and update the stage table and the plan index.

## Tests

- Each editor method sets its editor, and chains with `DependsOn` in either order.
- A form-typed loader receives the form, and `UseResource` with `Where((record, form) => ...)` filters by it, on forms, create sheets and inline rows (the existing dependent fields tests, migrated).
- `UseSource` with and without an argument, on both lookup sheets: search, find by value, description and media, and a source the app didn't register.
- Problems: a value selector or source of the wrong value type, an argument of the wrong type, an argument missing or extra. Each keeps the field's default editor and names the fix.
- EF Core `UseItems<TEntity>` on an `int`, `int?` and `List<int>` field, and the existing EF items tests with the new signatures.

## Progress

- **Step 1 (2026-10-23, uncommitted):** done as above, with the deviations recorded in API and Behaviour. The whole solution has to build, so the playground was migrated in this step: the `UseEditor` calls were rewritten mechanically, typed loader parameters dropped, and the four sources (`GalleryAirportLookupSource`, `GalleryGuideLookupSource`, `LayoutDestinationLookupSource`, `TripAirportSource`) became `LookupSource` classes. The field editor gallery shows each lookup layout with a different description and media, so it now has a small source subclass per combination; presentation can no longer vary per field for one source.
  - Tests: the integration tests were migrated the same way, and the category test source has a subclass per description and media. EF Core integration tests create the form-typed editors through `Infrastructure/TestEditors.cs`. New: `EditorCheckTests` (items and sources of another value type, nullable and element types, source arguments missing, extra or of another type, two media methods), `SourceLookupItemsTests` (shape and argument type read from the source and the expression, a missing argument value) and `ResourceConfigurationTests.ItemsOfAnotherValueType_KeepDefaultEditorAndLogProblem`. The editor methods, form-typed loaders, `Where((record, form) => ...)` and `UseSource` are covered by the migrated HTTP tests.
  - Verified: `dotnet test --solution StellarAdmin.slnx -c Release`, 1887 passed, 0 failed. A full build shows no new warnings.
  - Committed in `92e1638`.
- **Step 2 (2026-10-23):** committed in `5703182`.
  - Consumer references: `skills/stellar-admin` (SKILL.md, and the editors, relationships, forms and inline editing references) teach the `Use…Editor` methods, `UseEditor<TEditor>` for custom editors only, form-typed loaders and `Where((record, form) => ...)`, EF `UseItems<TEntity>` with its value check, and `LookupSource` classes with `UseSource`. `skills/stellar-admin-setup` troubleshooting names lookup sources. SKILL.md and `docs/development.md` also still named the stage E `UseItems<TContext, TEntity, TValue>`; both are fixed.
  - `build/skill-evals/admin-panel/tasks.md`: the checks name `UseLookupSheetEditor`, `UseRadioGroupEditor` and `UseSliderEditor`, and the customer lookup takes its items from the Customers resource.
  - Playground: gallery labels and descriptions name the new calls. The gallery's airport lookups keep their sources instead of moving to `UseResource<GalleryAirport>()` as planned: the gallery shows each layout with a different description and media, which one resource's `Reference` settings can't give per field.
  - Verified: the playground builds; `npm run check --prefix build/skill-validation` validated 4 skills and 92 Markdown files; `dotnet run --project util/SkillsGenerator -- --check` reports no drift. Smoke run on port 5207 (stopped afterwards; `app.db` unchanged): the home page, Trips, the Trips edit page, Products and its edit page, Categories, Destinations and all 16 field editor gallery create pages return 200; Users redirects to sign-in. Lookup searches return the source's titles, descriptions, codes, images and avatars on the lookup and multi-lookup galleries, and the Trips airport lookup returns Osaka's airports for Japan and Santiago for Chile from the form's country. The startup problems are only the deliberately misconfigured Excursions ones.
- **Step 3 (2026-10-23):** this plan is archived, the stage table marks A and D done, and the plan index lists it as retired.

## Open

- Choice editors (select, radio group) could take a `LookupSource` too, loading every item through `SearchAsync`. Not part of this plan; `IChoiceItemsProvider` stays.
- Stage F (handlers name their model once) and G (resource classes) are separate plans.
