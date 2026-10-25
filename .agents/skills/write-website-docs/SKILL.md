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

### Introduce every code block and screenshot

Put a sentence before each code block or screenshot that says what it shows, so the reader knows what to look for before reading it.

- Added: "You can supply your own list of options by calling `UseItems()` in the editor's configuration callback."
- Added: "The example below loads the options from a service registered with dependency injection."
- Added: "Using the radio group editor with a property of type `TripDifficulty` will display the following:", before a screenshot.

Name what to look for in the code, not only "the example below".

- Added: "The example below defines `AddGuides()` in the `GuidesAdmin` class:"

### Keep examples to the section's topic

An example shows only what its section is about. Settings from other sections make the reader work out which part matters. Where the point is the result, show the result.

- Before (in the enum section): a radio group example that also set `Appearance` and `Columns`.
- After: a screenshot of the radio group the enum produces.

### Write method names with parentheses

In prose, write a method as `UseItems()`, so the reader sees it's a call. Tables of overloads keep their full signatures.

- Before: "Anything else needs `UseItems` or `UseResource`."
- After: "For anything else, you must call `UseItems()` or `UseResource()`."

### Use the precise developer term

Use the word a developer would use for the concept, not a loose everyday one.

- Before: "`UseItems` has a few other forms"
- After: "`UseItems()` has a few different overloads"
- Before: "picking from a list"
- After: "selecting from a list"

### Make headings say what the section covers

A reader scanning the page should know what a section is about from its heading alone.

- Before: "Enums"
- After: "Generating options for enums"

### Phrase table cells to match the column heading

Every cell in a column should read as a completion of its heading.

- Before (under "Use it for"): "One value from a list of any length."
- After: "Select one value from a list of any length."

### Link to the page that explains a concept

When you mention something another page explains, link to it there.

- Before: "`UseResource<TResource>()` | The records of another resource."
- After: "… using the [title and reference information](/docs/admin/titles-and-references) of that resource for the title, description and media."

### Name the exact code element

When the reader has to find something in their code, name the property, the parameter and the method, not a loose description of where it is.

- Before: "StellarAdmin passes the sort and the page to its `ListAsync()` method in the request:"
- After: "StellarAdmin passes the requested sort and page via the `Sort` and `Paging` properties of the `request` parameter when calling the `ListAsync()` method:"

### Use one term for one thing

Use the same word for a concept throughout a page, and use the API's word. Two words make the reader wonder whether there are two concepts.

- Before: "the data source must implement sorting and pagination", on a page about `EnablePaging()`.
- After: "the data source must implement sorting and paging"

### Show the code for anything the reader implements

When the reader writes code against a contract, such as a data source's `ListAsync()`, show it in an example as well as describing it. Prose alone leaves them to work out the code.

- Before: a sentence naming `ResourceListRequest.Sort` and `ResourceListRequest.Paging`, then a link.
- After: a table of what `Sort` and `Paging` hold, and an example that sorts and pages the records.

### Examples must follow the API's own advice

Readers copy examples, so an example that breaks a documented requirement spreads the bug.

- Before: a data source that sorted suppliers by name only, although `ListAsync()` says to break ties with a unique key when paging.
- After: `suppliers.OrderBy(supplier => supplier.Name).ThenBy(supplier => supplier.Code)`

### Name a group of things by what it is, not by a code pattern

A pattern like `sa-*` means nothing to a reader who hasn't met it. Where the reader may not know the pattern, such as in the admin panel docs, name the group by what it is, and link to it.

- Before: "If you also write your own pages with `sa-*` components"
- After: "If you also write your own pages with the [StellarAdmin.UI Tag Helpers](/docs/ui)"

A specific element the reader writes is code, so write it as code: the StellarAdmin.UI docs name `<sa-button>`, because that's what goes in the page.

### Make link text match the linked page's title

The reader should recognize the page from the sidebar when they land on it.

- Before: "See [sidebar](/docs/admin/sidebar) for groups and ordering."
- After: "See [sidebar and links](/docs/admin/sidebar) for groups and ordering.", the title of that page.

### Describe code with the right relationships

Say what a member extends, and which type defines it. A loose description can say something false. A method with a body is "defined"; "declared" is for a signature with no body, such as on an interface.

- Before: "`AddResources()` is defined as an extension method on the `AdminResources` class"
- After: "`AddResources()` is an extension method on `StellarAdminBuilder`, defined in the `AdminResources` class."

### Say what StellarAdmin doesn't do, when the reader must do it

When the reader must act because StellarAdmin doesn't do something they might expect, say so, and what it means for them. Describing a state they never see doesn't tell them why they act.

- Before: "A grid has no columns until you add them."
- After: "StellarAdmin doesn't add columns automatically, so the grid displays only the columns you add."

## Style

Choices made on feel. Follow them, but they aren't rules to argue from.

- Vary "app" and "application" to avoid repeating a word in the same sentence: "It runs inside your application, and uses your app's `DbContext`."
- Write "the StellarAdmin.UI components", with "the".
- Write "admin panel" in lower case.
- "Run the application", not "Run the app".
- Don't restate what happens if a step is skipped once the step already says to do it. This is judgement, not a rule: if something becomes a common installation issue, call it out more.
- Avoid marketing words like "rapidly".
- "Unless specified otherwise", not "Unless you say otherwise".
- Write "must", not "has to": "The default sort must be on a sortable column."
- Prefer "display" over "show" for what appears on screen: "Radio groups and checkbox groups display their options in a single column." "Display" needs an object or the passive ("the media is displayed"), so rephrase rather than write "the option displays". Keep "show" where it pairs with "hide" or matches an API name like `ShowMedia`.
