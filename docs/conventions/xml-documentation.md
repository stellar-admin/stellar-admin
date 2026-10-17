# XML documentation conventions

Conventions for XML doc comments in all StellarAdmin libraries. Adopted 2026-08-10, after the identity resource layer refactor forced a pass over every comment in what was then `StellarAdmin.Dashboard.Identity` (since removed). Revised 2026-10-17 to follow the [csharp-docs](../../.agents/skills/csharp-docs/SKILL.md) skill, with the exceptions below.

## The rule

An XML comment is documentation for the **consumer** of the library. It says what the class, property or method does, in plain language, and nothing else.

- **Be brief.** The summary is one sentence. One idea per sentence.
- **Never expose internals.** No order of operations, no validation timing, no "last call wins", no "the view model reads this", no design history. That context belongs in `docs/design/`, or in a regular `//` comment at the site that needs it, not on the API surface.
- **Use the vocabulary .NET developers already know**: *Returns*, *Gets*, *Sets*, *Adds*, *Removes*, *Applies*, *Initializes*. A comment that reads like a translation is worse than a plain one.

## Wording

Follow the [csharp-docs](../../.agents/skills/csharp-docs/SKILL.md) skill for wording. In short:

- **Types and methods**: start the summary with a present-tense, third-person verb ("Renders…", "Adds…", "Returns…").
- **Constructors**: "Initializes a new instance of the `<see cref="X" />` class."
- **Properties**: "Gets or sets…" for read-write, "Gets…" for read-only, and "Gets or sets a value that indicates whether…" for Booleans.
- **Parameters and return values**: a `<param>` for each parameter and a `<returns>` for each non-void method. Write a noun phrase that starts with an article and doesn't name the type ("The partial view to render."). A Boolean reads "`<see langword="true" />` to …; otherwise, `<see langword="false" />`."
- **Exceptions**: an `<exception cref>` for each exception the member throws itself, such as an argument check. State the condition directly ("`<paramref name="configure" />` is `<see langword="null" />`."). Leave out exceptions that only happen by accident, such as a null reference.
- **Overrides and interface members**: `<inheritdoc />`, unless the behaviour differs in a way the caller needs to know.
- **Keywords and code**: `<see langword>` for `null`, `true` and `false`, `<c>` for inline code and attribute names, `<see cref>` for references in a sentence and `<seealso>` for standalone references.

## Where StellarAdmin differs from the skill

- **Do not document `internal` types or members.** Leave them uncommented. If an internal type needs explanation, use `//` comments. Enum-extension classes are `internal` and undocumented, since they are plumbing, not API.
- **Add `<value>` only when it adds something** the summary doesn't say, such as the default, what `true` means, the units or the format. Leave it out when it would repeat the summary.
- **Default values go in `<value>`**, as a separate sentence: "The default is `<see langword="false" />`." A property with a default always gets a `<value>`.
- **Behaviour notes go in `<remarks>`**, such as "Ignored when bound with `asp-for`" or "Requires `stellar-admin.js`."
- **No `<example>` blocks.** Examples live in the website docs, the DocsSamples and the consumer skills.

## Formatting

Write `<summary>`, `<remarks>`, `<value>` and multi-line `<exception>` text as multiline blocks, even for one short sentence. Put the opening and closing tags on their own lines and indent the text four spaces after `/// `. A short `<param>` or `<returns>` can stay on one line. Brevity describes the wording, not a single-line XML layout.

## Examples

```csharp
/// <summary>
///     Gets or sets a value that indicates whether the choice is disabled.
/// </summary>
/// <value>
///     <see langword="true" /> if the choice is disabled and can't be selected; otherwise,
///     <see langword="false" />. The default is <see langword="false" />.
/// </value>
```

```csharp
// Too long: the parameters are folded into the summary.
/// <summary>
///     Lets rows expand to show a partial view, with a model the loader returns for a resource
///     key when the row expands, or with the row's resource when the loader is null.
/// </summary>

// Correct.
/// <summary>
///     Shows a partial view when a row is expanded.
/// </summary>
/// <param name="partialName">The partial view to render.</param>
/// <param name="loader">
///     A delegate that loads the partial's model from the row's key, or <see langword="null" />
///     to use the row itself as the model.
/// </param>
```

```csharp
// Too long: the reader does not need the fall-back logic.
/// <summary>
///     Gets the field the user list is sorted by, or <see langword="null" /> when it is
///     unsorted, including when the requested sort field did not match a sortable column.
/// </summary>

// Correct.
/// <summary>
///     Gets the field the list is sorted by, or <see langword="null" /> when the list is unsorted.
/// </summary>
```

If the trimmed detail is load-bearing for maintainers (the fall-back behavior above is), keep it as a `//` comment inside the member, next to the code that implements it, where it cannot drift out of sight of an edit.
