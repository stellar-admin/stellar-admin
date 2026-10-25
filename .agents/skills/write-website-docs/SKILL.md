---
name: write-website-docs
description: 'Rules for writing and editing the public website docs pages (MDX under the website repo''s content/docs/). Use before drafting, rewriting or reviewing any docs page prose.'
metadata:
  internal: true
---

# Writing website docs

Rules for the public docs in the website repo's `content/docs/` (the sibling `../website` checkout, or `STELLARADMIN_WEBSITE_DIR`). The audience is developers. Docs can't leave room for confusion or ambiguity, so clarity comes before voice. Each rule comes from a change Jerrie made to a page, with the before and after.

Before handing a page over, reread every paragraph as a newcomer: what exactly would they do after reading it, and could they read it another way?

## Adding rules

New rules come from Jerrie's edits. When he rewrites a page, compare it against the version before his edit, propose one rule per change with the reader problem it fixes, and add only the rules he confirms, with their before and after. Changes he makes on feel go under Style.

## Rules

### Say it directly

State a requirement or what something does plainly, as the first thing. No lead-ins, idioms or roundabout constructions.

- Before: "StellarAdmin goes into an app you already have. Any ASP.NET Core app on .NET 10 or later will do, whether it uses MVC, Razor Pages or minimal APIs."
- After: "To install StellarAdmin, you need an ASP.NET Core app (MVC, Razor Pages or minimal APIs) running on .NET 10 or later."
- Before: "StellarAdmin gives you those pages without you writing them."
- After: "StellarAdmin lets you quickly create these admin pages."

### One instruction per sentence

Don't fold an optional step into a clause. Give it its own sentence, with its condition.

- Before: "Add `StellarAdmin`, and `StellarAdmin.EntityFrameworkCore` if your data lives in EF Core."
- After: "Add the `StellarAdmin` package. If you want to use StellarAdmin with EF Core, also add the `StellarAdmin.EntityFrameworkCore` package."

### Only mention what the reader needs at that point

Don't bring up things the reader doesn't know or care about yet. If it's worth knowing, put it in a callout that says what it is and why it matters.

- Before (in the install steps): "There's no need to install `StellarAdmin.UI` as well. It comes along with `StellarAdmin`."
- After: a callout saying `StellarAdmin` also installs StellarAdmin.UI, the Tag Helper library StellarAdmin is built with, which can also be used on its own.

Don't reassure the reader about something they weren't wondering about.

- Removed: "You don't need to set it up to use the panel."

### Give a reason for optional things

When you suggest something optional, say why the reader might want it.

- Before: "You can also use its components in your app's own pages."
- After: "You can also use its components in your app's own pages to have a consistent look and feel."

### Callout types

Use `info` for something the reader should know, and `idea` for something they might want to do.

### Say what a call does where you introduce it

Say it in the same sentence, at the reader's level. Don't name internals they don't need.

- Before: "Call `AddStellarAdmin` in `Program.cs`." followed later by "`AddStellarAdmin` registers the MVC services the panel needs, so you don't need to add them yourself."
- After: "Call `AddStellarAdmin` in `Program.cs` to register the services StellarAdmin needs."

### Show where the reader will add code later

When a code block is a starting point the reader will fill in later, put a comment where that code goes instead of leaving it empty. Use the present tense, and don't repeat the comment in the prose.

- After: `// Resources and other configuration go here`

### Use the term the reader already knows

Name the category the reader would search for.

- Before: "most apps need an admin area."
- After: "most apps need an admin area, often called an admin panel."

### Keep overview pages at the concept level

An introduction describes what you do, not the API details you'll meet later.

- Before: "You register each entity you want to manage as a **resource**, and choose the columns and fields it shows."
- After: "You register the entities you want to manage as *resources*, and configure how they are displayed."

### Introduce new terms in italics

Use italics, not bold, the first time a term is defined: *resources*.

### Name what "this" and "it" refer to

Don't open a paragraph with a bare "This", and don't use "its" where it could point at more than one thing.

- Before: "This gives you a Trips page…"
- After: "The example above gives you a Trips page…"
- Before: "a lookup that searches its records"
- After: "a lookup that searches the records of the related resource"

### Use the product's name, not a stand-in

"The panel" on its own means nothing. Write "StellarAdmin", or "the admin panel" when you mean the UI.

- Before: "The panel isn't a separate service."
- After: "StellarAdmin isn't a separate service."
- Before: "The panel's CSS and scripts are static assets"
- After: "StellarAdmin's CSS and scripts are static assets"
- Before: "the panel's home page"
- After: "the admin panel's home page"

### Don't imply a cause that isn't the point

- Before: "It runs inside your app, so it uses your app's `DbContext`…"
- After: "It runs inside your application, and uses your app's `DbContext`…"

### Use precise verbs

Prefer a verb that says what happens over "gets", "has" or "does".

- Before: "a `bool` gets a checkbox"
- After: "a `bool` is rendered as a checkbox"

### One idea per paragraph

If a sentence moves to a different topic, start a new paragraph. In the introduction, replacing a page with your own view moved out of the paragraph about editors.

### Say what each package does

Give each package's job, and don't make an optional package sound required.

- Before: "To manage data in EF Core, you also need the `StellarAdmin.EntityFrameworkCore` package."
- After: "To manage data in EF Core, add the `StellarAdmin.EntityFrameworkCore` package, which queries and updates data through your `DbContext`."

### Don't repeat what has its own page

Leave requirements and setup details to the page that owns them. The introduction doesn't repeat ".NET 10 or later"; installation states it.

## Style

Choices made on feel. Follow them, but they aren't rules to argue from.

- Vary "app" and "application" to avoid repeating a word in the same sentence: "It runs inside your application, and uses your app's `DbContext`."
- Write "the StellarAdmin.UI components", with "the".
- Write "admin panel" in lower case.
- "Run the application", not "Run the app".
- Don't restate what happens if a step is skipped once the step already says to do it. This is judgement, not a rule: if something becomes a common installation issue, call it out more.
- Avoid marketing words like "rapidly".
