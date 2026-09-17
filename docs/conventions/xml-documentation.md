# XML documentation conventions

Conventions for XML doc comments in all StellarAdmin libraries. Adopted 2026-08-10, after the identity resource layer refactor forced a pass over every comment in what is now `StellarAdmin.Dashboard.Identity`.

## The rule

An XML comment is documentation for the **consumer** of the library. It says what the class, property or method does — and nothing else.

- **Be brief.** One sentence for most members. Say what it does and stop.
- **Never expose internals.** No order of operations, no validation timing, no "last call wins", no "the view model reads this", no design history. That context belongs in `docs/design/`, or in a regular `//` comment at the site that needs it — not on the API surface.
- **Use the vocabulary .NET developers already know**: *Returns*, *Gets*, *Sets*, *Adds*, *Removes*, *Throws*, *Applies*, *Transforms*. Properties are noun phrases ("The page title."). Do not substitute unfamiliar verbs for familiar ones — a comment that reads like a translation is worse than a plain one.

## Formatting

Write `<summary>` and `<remarks>` as multiline blocks, even for one short sentence. Put the opening and closing tags on their own lines and indent the text four spaces after `/// `. Brevity describes the wording, not a single-line XML layout. Apply this to public types, properties, methods, and enum members. Internal types remain undocumented.

## Supporting rules

- **Do not document `internal` types** — leave them uncommented. If an internal type needs explanation, use `//` comments.
- **Default values go in `<remarks>`, not in the summary.**
  ```csharp
  /// <summary>
  ///     The item label.
  /// </summary>
  /// <remarks>
  ///     Defaults to the title of the page the item links to.
  /// </remarks>
  ```
- **Enum-extension classes are `internal` and undocumented** — they are plumbing, not API.

## Examples

```csharp
// Too long — explains when a failure happens and what the library does not do.
/// <summary>
///     Applies the search term the user entered to the users query. The query is not
///     validated at configuration time; one the query provider cannot translate fails
///     when the users index page renders with a search term applied.
/// </summary>

// Correct.
/// <summary>
///     Applies the search term to the query.
/// </summary>
```

```csharp
// Too long — the reader does not need the fall-back logic.
/// <summary>
///     The field the user list is sorted by, or <c>null</c> when it is unsorted —
///     including when the requested sort field did not match a sortable column.
/// </summary>

// Correct.
/// <summary>
///     The field the list is sorted by, or <c>null</c> when the list is unsorted.
/// </summary>
```

If the trimmed detail is load-bearing for maintainers (the fall-back behavior above is), keep it as a `//` comment inside the member — next to the code that implements it, where it cannot drift out of sight of an edit.
