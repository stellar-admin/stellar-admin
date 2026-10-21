# Configuration problems

A mistake in the admin panel's configuration never stops the app. The panel works around it, logs it, and in Development lists it on every admin page.

## Contents

- [What the panel does](#what-the-panel-does)
- [Finding problems](#finding-problems)
- [Diagnostics settings](#diagnostics-settings)
- [Reporting problems from an integration](#reporting-problems-from-an-integration)

## What the panel does

The panel works around each mistake in the narrowest way it can, and says what it did. For example:

- A default sort on a column that isn't sortable: the list uses the data source's order.
- A `PageSize` missing from `PageSizes`: it is added to them. This is info rather than a warning, since the page size still applies.
- `AllowCreate` with a data source that can't create: there's no New button, and the create page returns 404.
- A field without a public setter: the field is left out of its form.

When nothing narrower works, such as an EF Core entity that isn't mapped in its context or a slug that isn't valid or that another resource uses, the resource is unavailable: it is hidden from the sidebar, the home page and the command palette, and its pages return 404. In Development they show why instead. Other resources and the host app keep working.

Only a null argument still throws, at the call that passed it.

## Finding problems

- Each problem has a level. An error makes a whole resource unavailable. A warning means something that was configured doesn't happen. Info means it happens, and the panel adjusted something next to it.
- In Development, a pill at the bottom of each admin page counts the problems of each level. It opens a sheet that shows the lines of the app's code that configured them, such as the `fields.Add` call of a field that is left out, with each problem under its line: what you notice, why, what happens instead, and how to fix it. A problem with no line, such as one from a build without debug symbols, is listed under its resource after the files.
- Every environment logs each problem once in the `StellarAdmin.Configuration` category, at its level (error, warning or information), with the full path and line of the code that configured it. Most problems are logged at startup. A few can only be found when a page renders, such as a lookup's `EnableCreate` for a resource without a create form; those are logged the first time, and the field is left out or the lookup has no New button.
- Each kind of problem has its own log event ID, so log filters and alerts can match it.
- A problem with no line of its own, such as a blank label set on the resource, names the `AddResource` call. The line comes from the app's debug symbols, so a build without them shows none.

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
                $"The `{field.FieldName}` field isn't on the form because `Booking.{field.FieldName}` can't be saved.",
                $"Make `Booking.{field.FieldName}` saveable, or remove the field.",
                field
            );
        }
    }
});
```

- `ReportProblem` reports a warning. It takes the message (what you notice, why, and what the panel does instead) and then the fix. Backticks mark code in both. Pass the field (any `FormItemOptions`) or the `DataGridColumnOptions` column the problem is about, and the problem names the line that added it; without one, it names the resource's `AddResource` call.
- `ReportInfo` takes the same arguments, for an adjustment that keeps what was configured.
- `ReportUnavailable(eventId, message, fix)` reports an error that makes the whole resource unavailable, for a problem with no narrower fallback.
- `ResourceOptions<T>`, `ResourceFormOptions.RemoveField` and `DataGridColumnOptions.DisableSorting` are in `StellarAdmin.Resources.Options`.
- Use event IDs of your own. StellarAdmin uses 1000 to 1099.
