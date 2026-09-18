# Markdown conventions

Conventions for Markdown and MDX across all StellarAdmin repos — the docs site content, README files, design docs, agent skills, and the conventions docs themselves. Adopted 2026-08-24.

## Use separate sentences

Do not join independent clauses with semicolons in prose. Jerrie prefers separate sentences with full stops. This applies to documentation and user-facing explanations, not semicolons required by code syntax.

## Do not hard-wrap prose

Write one line per paragraph. Do not break prose at a column limit.

```markdown
<!-- correct -->
The assets are served from the package as static web assets. The CSS ships as one bundle per theme. Link exactly one, because switching themes is switching the `<link>`.

<!-- avoid -->
The assets are served from the package as static web assets. The CSS ships as
one bundle per theme. Link exactly one, because switching themes is switching
the `<link>`.
```

The same rule applies to a list item or blockquote that runs long: keep it on one line rather than continuing it on an indented next line.

**Why:** readers soft-wrap in their own editor at whatever width suits them, so a hard-wrapped file imposes one author's width on everyone. Hard wrapping also makes editing more expensive than it looks — inserting a word mid-paragraph reflows every following line in that paragraph, so the "cleaner diffs" argument mostly evaporates for anything larger than a single-word swap, while every edit risks leaving the paragraph ragged.

It also keeps hand-written files consistent with generated ones. Generators emit a paragraph as a single line and will never wrap, so hard-wrapping by hand splits a repo into two styles that can never converge.

## What this does not apply to

- **Fenced code blocks.** Code wraps where the code wants to wrap.
- **Tables.** One row per line, as usual.
- **YAML frontmatter.** It is YAML, not prose. A folded block scalar (`description: >-`) is the idiomatic way to write a long value there and produces the same string either way, so leave those wrapped.
- **Deliberate line breaks.** Two trailing spaces or a `<br>` where a real break is intended.

## Applying it

Follow this in new and substantially edited Markdown. Do not reflow untouched files as a drive-by — a whole-file rewrap buries the real change in the diff. Sweeping an existing file is fine as its own commit.
