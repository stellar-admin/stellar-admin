# Computed index columns

Status: implemented (2026-10-20) for index columns. View page and form display fields still require a property path; they are a follow-up.

## Problem

`columns.Add(product => string.Join(",", product.Tags.Select(t => t.Name)))` was accepted at startup, and `DataGridColumnOptions.FieldName` was documented as null for such expressions, but `<sa-data-grid-column field-for>` accepted only a property chain, so the index page threw when it rendered. On an EF Core resource the cell would also have been empty, because includes came only from property paths.

## Decisions (2026-10-20)

- The resource API is unchanged: `columns.Add` takes any expression.
- A computed column without a `Title` fails at startup.
- A computed column sorts only with `Sortable(selector)`; `Sortable()` alone fails at startup.
- Its sort key is its title in PascalCase (`Tag names` becomes `TagNames`), exposed as the new public `DataGridColumnOptions.SortField` (the `FieldName` of a property column). Sort links, the controller, `ResourceSort.Field` and the EF data source use it. A computed column whose key matches another sortable column's fails at startup.
- Columns only for now; view page and form display fields are a follow-up.

## How it works

- StellarAdmin.UI: `field-for` accepts any expression. A property chain behaves as before. Any other expression is compiled once per column when the grid collects its columns (`DataGridComputedValue`), and called for each row. Property reads and instance method calls on null give the member type's default instead of throwing; static and extension methods aren't guarded. Templates get the metadata of the expression's return type. `template` no longer requires a property for a computed column.
- StellarAdmin: `ComputedColumnOptionsValidator` enforces the title, sorting and key rules.
- StellarAdmin.EntityFrameworkCore: `EfCorePropertyPaths.FindDisplayIncludePaths` collects every property path a column's expression reads from its parameter and includes what each needs, as for a property column. Navigations read through a nested lambda's parameter, such as `t.Category` inside `Select(t => ...)`, aren't included.

## Verification (2026-10-20)

- `DataGridColumnTagHelperTests`: computed values for each row, null safety with empty text, template with the computed value.
- `ResourceColumnDisplayTests`: computed columns render with titles and a display; null safety; missing title fails validation. `ResourceSortingTests`: sorting by the title-based key, `Sortable()` without selector and a clashing key fail validation.
- EF `ResourceColumnTests`: computed columns load a skip navigation (`Tags`) and a reference (`Category`) they read.
- Suites pass: StellarAdmin.Tests 256, StellarAdmin.IntegrationTests 597, StellarAdmin.EntityFrameworkCore.IntegrationTests 115, StellarAdmin.UI.Tests 684, StellarAdmin.UI.IntegrationTests 19. `util/SkillsGenerator --check` reports no drift.
- Website: a "Computed columns" section on the data grid page; `pnpm lint`, `pnpm types:check` and `pnpm build` pass. Not checked in a browser.
