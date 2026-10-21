# Configuration problems

A mistake in the admin panel's configuration never stops the app. The panel works around it, logs it, and in Development lists it on every admin page.

## Contents

- [What the panel does](#what-the-panel-does)
- [Finding problems](#finding-problems)
- [Diagnostics settings](#diagnostics-settings)
- [Reporting problems from an integration](#reporting-problems-from-an-integration)

## What the panel does

Each problem has one of four effects, from the narrowest to the widest:

| Effect | Meaning | Examples |
| --- | --- | --- |
| Ignored | A setting falls back to its default. | A default sort on a column that isn't sortable: the list uses the data source's order. A `KeepForNext` that names no create field. |
| Adjusted | A setting is corrected. | A `PageSize` missing from `PageSizes` is added to them. A column span of 13 becomes 12. |
| Turned off | One feature is unavailable. | `AllowCreate` with a data source that can't create: no New button, and the create page returns 404. A field without a public setter is left out of its form. |
| Resource off | The whole resource is unavailable. | An EF Core entity that isn't mapped in its context, or a slug that isn't valid or that another resource uses. |

A turned-off resource leaves the sidebar, the home page and the command palette, and its pages return 404. In Development they show why instead. Other resources and the host app keep working.

Only a null argument still throws, at the call that passed it.

## Finding problems

- In Development, a "Configuration problems" pill at the bottom of each admin page shows how many there are. It opens a sheet that lists them by resource, with what is wrong, how to fix it and what the panel did.
- Every environment logs each problem once, as a warning in the `StellarAdmin.Configuration` category. Most problems are logged at startup. A few can only be found when a page renders, such as a lookup's `EnableCreate` for a resource without a create form; those are logged the first time, and the field is left out or the lookup has no New button.
- Each kind of problem has its own log event ID, so log filters and alerts can match it.

A page that looks incomplete (a missing field, column, button or sidebar item) usually has a problem listed in the pill.

## Diagnostics settings

```csharp
builder.Services.AddStellarAdmin(admin =>
{
    admin.ConfigureDiagnostics(diagnostics =>
    {
        diagnostics.ShowIndicator = false;   // default true; the pill only ever shows in Development
        diagnostics.ThrowOnProblems = builder.Environment.IsEnvironment("Testing");   // default false
    });
});
```

- `ShowIndicator = false` hides the pill and its sheet. Problems are still logged.
- `ThrowOnProblems = true` makes startup throw an `InvalidOperationException` that lists every problem, in every environment. Set it from a condition, such as the environment, for tests or CI that should fail on a configuration mistake. Problems found while a page renders are only logged.

## Reporting problems from an integration

A package or app code that configures resources in a post-configuration callback reports what it worked around with `ReportProblem`, after repairing the options:

```csharp
services.AddOptions<ResourceOptions<Booking>>().PostConfigure(options =>
{
    foreach (var field in options.Create?.Fields ?? [])
    {
        if (!CanSave(field))
        {
            options.Create!.RemoveField(field);
            options.ReportProblem(
                new EventId(5001, "UnsavableField"),
                ConfigurationProblemEffect.TurnedOff,
                $"Booking.{field.FieldName} can't be saved.",
                "The field is left out."
            );
        }
    }
});
```

- `ConfigurationProblemEffect` is in `StellarAdmin.Diagnostics`; `ResourceOptions<T>`, `ResourceFormOptions.RemoveField` and `DataGridColumnOptions.DisableSorting` are in `StellarAdmin.Resources.Options`.
- `ConfigurationProblemEffect.ResourceOff` turns the resource off.
- Use event IDs of your own. StellarAdmin uses 1000 to 1099.
