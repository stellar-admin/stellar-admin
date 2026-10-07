# Dashboard form grid

Status: in progress, 2026-10-07. Phase 1 done; phases 2 and 3 not started.

## Goal

A developer can set the column count of any form scope (the form root, a section, a group) and the column span of any item (a field, a section, a group) in Dashboard create and edit forms, with values that vary per breakpoint. This replaces `AddRow`, whose rows only give one equal column per child, with no wrapping and no spans.

## Decisions

- Every form scope is a grid. `Columns` defaults to 1, so a form without layout calls renders as today.
- Tiers are `Default`, `Small` (≥30rem), `Medium` (≥40rem) and `Large` (≥56rem). Widths may be tuned during implementation. Tiers are container queries on the grid itself, not viewport media queries, so the Create sheet and a narrow section collapse correctly.
- Unset tiers inherit from the next smaller tier. A plain `Columns(n)` means 1 column below Medium and `n` from Medium. A plain `ColumnSpan(n)` applies at every tier.
- Spans clamp to the parent grid's column count at each tier, resolved on the server. `*Full` spans the whole grid at that tier.
- No `grid-auto-flow: dense`, because it breaks tab order. No `ColumnStart` in this slice.
- `AddRow` is removed, not deprecated; `AddGroup(group => group.Columns(n))` covers it. There is no separate `AddGrid`.
- A group is a container with no title or border. It gives part of the form its own column count, or stacks several fields in one cell of the parent grid. A section does the same with a title and the section's layout.
- A section can override the form's section layout with `section.Layout`. Precedence: the Create sheet's forced `Stacked`, then the section, then the form's `SectionLayout`, then the app-wide default in the Dashboard forms options.
- The grid stays internal to the Dashboard. `<sa-form-row>` in the TagHelpers package is unchanged; the work can move into the Tag Helpers later if needed.
- The [options builder conventions](../conventions/options-builders.md) gain a rule: layout values that vary per breakpoint use fluent methods with a tier builder; plain scalars stay properties.

## Public API

```csharp
edit.Fields(fields =>
{
    fields.Columns(2);

    fields.AddSection("Profile", section =>
    {
        section.Layout = FormSectionLayout.Split;
        section.ColumnSpanFull();
        section.Columns(columns => columns.Small(2).Large(3));
        section.Add(m => m.FirstName);
        section.Add(m => m.LastName);
        section.Add(m => m.Initials);
        section.Add(m => m.Bio).ColumnSpanFull();
    });

    fields.AddSection("Account", section =>
    {
        section.Add(m => m.Email);
        section.AddGroup(group =>
        {
            group.Columns(2);
            group.Add(m => m.Password);
            group.Add(m => m.PasswordConfirmation);
        });
    });
});
```

- `ResourceFieldsBuilder<TModel>`: adds `Columns(int)` and `Columns(Action<GridColumnsBuilder>)`, both chainable. Removes `AddRow()` and `AddRow(configure)`. `AddGroup()` returns the new `ResourceGroupBuilder<TModel>`.
- `ResourceSectionBuilder<TModel>` and the new `ResourceGroupBuilder<TModel>`: add `ColumnSpan(int)`, `ColumnSpan(Action<ColumnSpanBuilder>)` and `ColumnSpanFull()`.
- `ResourceSectionBuilder<TModel>`: adds a setter-only `FormSectionLayout? Layout`, defaulting to `null` (use the form's layout).
- `ResourceFieldBuilder`: adds the same three span methods, chainable like `UseEditor`.
- `GridColumnsBuilder`: `Default(n)`, `Small(n)`, `Medium(n)`, `Large(n)`.
- `ColumnSpanBuilder`: the same four tiers plus `DefaultFull()`, `SmallFull()`, `MediumFull()`, `LargeFull()`.
- Counts and spans below 1 throw. Upper limit for columns: 12.

## Rendering

- `FormSectionOptions` gains `Layout`. `_FormSection.cshtml` uses it unless the Create sheet forces `Stacked`.
- `FormRowOptions` and `_FormRow.cshtml` are removed. `FormContainerOptions` gains the resolved columns per tier; `FormItemOptions` gains the requested span per tier.
- The server resolves inheritance and clamping and writes inline CSS variables on each grid and cell: `--sa-cols`, `--sa-cols-sm/md/lg`, `--sa-span`, `--sa-span-sm/md/lg`.
- The CSS lives in the Dashboard's `Client/css/client.css` under `@container form-grid`. Span variables are registered with `@property { inherits: false }` so they do not leak into nested grids.
- `_FormItems.cshtml`, `_FormSection.cshtml` and `_FormGroup.cshtml` render the grid wrapper and cell variables.
- Keep `<sa-field-group>` inside each section and group. It provides the `@container/field-group` that horizontal fields use for their breakpoint, so the grid wrapper must not replace it. Phase 2 checks a horizontal field inside a multi-column grid.

## Phases

Stop for review after each phase.

1. **Model and builders.** The options changes, tier builders, resolution (inheritance and clamping), `section.Layout`, and TUnit tests for the builders and resolution. The convention amendment. `AddRow` stays, so the playground and integration tests keep building and passing.
2. **Rendering and `AddRow` removal.** Partials, inline variables, container-query CSS and per-section layout. Then remove `AddRow`, `FormRowOptions` and `_FormRow.cshtml`, moving the integration tests and the DashboardPlayground `Users` and `Customers` registrations to `AddGroup(group => group.Columns(n))`. Integration tests for the grid markup and section layout precedence. A browser check of a form at wide and narrow widths and in the Create sheet.
3. **Samples and docs.** Add a playground form that shows spans, per-tier counts and mixed section layouts. Update the `stellar-admin-dashboard` consumer reference (`setup.md`) and the development guide where it describes form layout.

## Verification

Phase 1 (2026-10-07):

- Added `GridColumnsBuilder`, `ColumnSpanBuilder`, `ResourceGroupBuilder<TModel>`, `Columns` on the fields scope, span methods on fields, sections and groups, and `section.Layout` (`FormSectionOptions.Layout`). Column counts and spans are stored internally on the options (`FormGridColumnDefinitions`, `FormColumnSpanDefinitions`) and resolved by `FormGridColumnDefinitions.Resolve()` and `FormColumnSpanDefinitions.Resolve(parent)`; they are not public on the options yet. The unit test project has `InternalsVisibleTo`.
- TUnit tests: resolution (inheritance, plain count, full span, clamping) in `tests/StellarAdmin.Dashboard.Tests/Resources/Options/`, and out-of-range rejection through the builders in `ResourceFieldsBuilderTests`.
- `dotnet build StellarAdmin.slnx` succeeded; `dotnet test --solution StellarAdmin.slnx --no-build` passed 782 of 782. Nothing renders the grid yet; that is phase 2.
