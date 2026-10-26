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

Start with what the reader sees, and put the condition after it.

- Before: "Depending on whether you have enabled the view page, the edit page or the delete action for your resource (by calling `AllowView()`, `AllowEdit()` or `AllowDelete()`), each row displays a view, edit or delete action."
- After: "Each row displays a view, edit or delete action, depending on whether you've enabled the view page, the edit page or the delete action for your resource by calling `AllowView()`, `AllowEdit()` or `AllowDelete()`."

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

Leave out pointers to things that aren't the page's subject, even to say they're elsewhere.

- Removed, on the theming page: "How form sections look isn't part of the theme. To change it, see choosing how sections look."

### Give a reason for optional things

When you suggest something optional, say why the reader might want it.

- Before: "You can also use its components in your app's own pages."
- After: "You can also use its components in your app's own pages to have a consistent look and feel."

### Callout types

Use `info` for something the reader should know, `idea` for something they might want to do, and `warn` for what breaks when a required step is missing.

- Before: "The image editor needs `StoreImage()`. Without it, StellarAdmin leaves the field off the form…", in the middle of a paragraph.
- After: the same text in a `warn` callout.

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

Keep closely related points about one feature in the same paragraph. Two short paragraphs of one sentence each look odd.

- Before: "StellarAdmin loads a panel's data the first time someone selects its tab." and "If you add only one panel, StellarAdmin displays it without tabs." as two paragraphs.
- After: "StellarAdmin loads a panel's data the first time someone selects its tab. If you add only one panel, StellarAdmin displays it without tabs."
- Before: "Each setting is a function… Text you set on one resource… overrides the default for that resource." in one paragraph.
- After: the override sentence in its own paragraph, because it's about the resource's own settings, not the function.

### Say what each package does

Give each package's job, and don't make an optional package sound required.

- Before: "To manage data in EF Core, you also need the `StellarAdmin.EntityFrameworkCore` package."
- After: "To manage data in EF Core, add the `StellarAdmin.EntityFrameworkCore` package, which queries and updates data through your `DbContext`."

### Don't repeat what has its own page

Leave requirements and setup details to the page that owns them. The introduction doesn't repeat ".NET 10 or later"; installation states it.

On a page about a feature, such as a display type, describe what the feature can do, and leave a requirement and its failure case to the page about the task. Displaying related records states that a collection needs `BadgeDisplay`, so the Badges page doesn't.

- Before: "To display a collection of related records, such as a trip's tags, call `UseDisplay<BadgeDisplay>()`. Without it, StellarAdmin leaves out the column or the field, and reports a configuration problem."
- After: "You can display a collection of related records, such as a trip's tags, using `UseDisplay<BadgeDisplay>()`."

The same goes for a statement the overview already makes. The overview says StellarAdmin never chooses `ImageDisplay`, so the Images page goes straight to the example.

- Removed: "StellarAdmin doesn't choose `ImageDisplay` for a property, so call `UseDisplay()`:"

Put a caveat the reader could miss in an `info` callout, instead of a sentence after the example.

- Before: "Items and groups with an `Order` still come first.", after the sorting example.
- After: an `info` callout: "Sorting by label only applies to the items and groups without an `Order`, which come after the ones with an `Order`."

### Introduce every code block and screenshot

Put a sentence before each code block or screenshot that says what it shows, so the reader knows what to look for before reading it.

- Added: "You can supply your own list of options by calling `UseItems()` in the editor's configuration callback."
- Added: "The example below loads the options from a service registered with dependency injection."
- Added: "Using the radio group editor with a property of type `TripDifficulty` will display the following:", before a screenshot.

Name what to look for in the code, not only "the example below".

- Added: "The example below defines `AddGuides()` in the `GuidesAdmin` class:"

When the paragraph before the code block is about something else, give the intro its own paragraph.

- Before: the recommendation to use `[Display]` and "The example below sets the label and the help text of `DepartureDate`:" in one paragraph.
- After: the recommendation, then the intro as its own paragraph.

### Introduce every table with how its entries are used

Put a sentence before each table that says where its entries go, so the reader knows how the rows relate to the call.

- Before: "`BrandingLogo` has a method for each kind of logo:"
- After: "To set the logo, set `Logo` to one of the following:"
- Added: "The parameter passed to `Add()` can be one of the following:"
- Added: "You can set the following optional properties in the `configure` callback:"

### Show the full nesting when a setting applies at several levels

When a setting can be made at several levels, such as every form, one form and one section, show one example from `AddStellarAdmin()` down to the deepest level. Keep only the calls that matter, and put a comment at each level. Separate one-line snippets leave the reader to work out where each line goes.

- Before: `section.Layout = …`, `edit.SectionLayout = …` and `admin.ConfigureUI(…)` as three unrelated lines.
- After: one `AddStellarAdmin()` example that sets the layout for every form, for the trip edit page and for its **Pricing** section, with "// Configures the layout for every form" and similar comments.

### Show where each setting is set

Before a table of properties, show code that sets one of them in the right callback, with the calls it's nested in. A table of names alone leaves the reader to guess where they go.

- Before: a table of `CountText`, `DuplicateText` and `RemoveLabel` with their placeholders, and no code showing where they're set.
- After: an example above the table that sets them in `labels.ListEditor(list => …)` inside `ConfigureResourceLabels()`.

### Make every example stand on its own

Readers jump straight to a section, so include the call each example goes in, even when an earlier section showed it.

- Before (in "Adding fonts"): the two `admin.AddStylesheet(...)` lines on their own.
- After: the same lines inside `builder.Services.AddStellarAdmin(admin => { … })`.

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
- Before: "you switch on the other pages it needs"
- After: "you enable the other pages it needs"
- Before: "the fewest and the most records people can select"
- After: "require a minimum or maximum number of related records to be selected"

The same goes for page titles: name the type a developer looks for, not what people see on screen.

- Before: "Yes or no, and enums"
- After: "Booleans and enums"

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

Put the link on a noun in the sentence, rather than ending with a bare "See [page]." A "see" that says why to follow the link is fine: "To change the defaults for every resource, see [global labels]."

- Before: "To change how the form's sections look, set `SectionLayout`. See [layout](/docs/admin/form-layout#choosing-how-sections-look)."
- After: "To change how the form's sections look, set `SectionLayout` to one of the [section layouts](/docs/admin/form-layout#choosing-how-sections-look)."
- Before: "Lets people sort the grid by the column. See [sorting and paging](/docs/admin/sorting-and-paging)."
- After: "Lets people [sort the grid](/docs/admin/sorting-and-paging) by the column."

### Name the exact code element

When the reader has to find something in their code, name the property, the parameter and the method, not a loose description of where it is.

- Before: "StellarAdmin passes the sort and the page to its `ListAsync()` method in the request:"
- After: "StellarAdmin passes the requested sort and page via the `Sort` and `Paging` properties of the `request` parameter when calling the `ListAsync()` method:"

### Describe a function's parameters exactly

Say how many parameters a function takes, and which type has which properties. Don't present a parameter's properties as the parameters.

- Before: "Each setting is a function that takes the resource's `SingularLabel` and `PluralLabel`, and returns the text."
- After: "Each setting is a function that returns the text. Its parameter is a `ResourceLabelContext`, with the resource's `SingularLabel` and `PluralLabel`."

### Name the specific items, not a vague group

When a statement applies to some members of a group, name them.

- Before: "Some list and dictionary texts change as people add and remove items."
- After: a table naming each text, `CountText`, `DuplicateText`, `ItemLabel`, `MoveLabel` and `RemoveLabel`, with its placeholder and what replaces it.

### Say which default is overridden

When a setting overrides a default, say which default, so the reader knows where the two are set.

- Before: "Text you set on one resource, such as `edit.Title` in `AllowEdit()`, overrides the default for that resource."
- After: "Text you specify on a specific resource's configuration, such as `edit.Title` in `AllowEdit()`, overrides the global default for that resource."

### Use one term for one thing

Use the same word for a concept throughout a page, and use the API's word. Two words make the reader wonder whether there are two concepts.

- Before: "the data source must implement sorting and pagination", on a page about `EnablePaging()`.
- After: "the data source must implement sorting and paging"

### Show the code for anything the reader implements

When the reader writes code against a contract, such as a data source's `ListAsync()`, show it in an example as well as describing it. Prose alone leaves them to work out the code.

- Before: a sentence naming `ResourceListRequest.Sort` and `ResourceListRequest.Paging`, then a link.
- After: a table of what `Sort` and `Paging` hold, and an example that sorts and pages the records.

This includes a delegate or a handler the reader writes. Say what it receives and what it must return, and show an implementation.

- Before: "pass `StoreImage()` a function that saves the file", with no example of the function.
- After: what the function receives and returns, and a `PhotoStore.SaveAsync()` that saves the file and returns its name.
- Before: "write a *handler* that loads it. The handler derives from `FieldEditorHandler<TEditor>`…", in prose only.
- After: an example handler, the editor that names it, and the template that reads `EditorData`.

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

In a "see" reference, the reader should recognize the page from the sidebar when they land on it. A link on a noun inside a sentence uses the noun that fits the sentence instead, such as "[sort the grid]" for the sorting and paging page.

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

### Don't repeat the section name in a page title

The sidebar lists a page's title under its section's name, so a title that repeats the section reads as a duplicate. Name what the reader does on the page. A single word, like "Delete", also reads oddly in the sidebar.

- Before: "Resources", in the Resources section
- After: "Defining resources"
- Before: "Delete"
- After: "Deleting records"

### Use one example throughout a section

When a section switches from one type to another, the reader has to map one example onto the other. Keep the same example for the whole section. If you want to showcase something the example can't, give it its own section.

- Before: `TripExtra` in the table of defaults, then `Trip` with "tours" in the URLs and the code to change them.
- After: `TripExtra` throughout, with the `"add-ons"` slug.

### Say "prefer" and why when you recommend an option

When one way of doing something is better than another, say "prefer" and give the reason in the same sentence.

- Before: "The best fix is `[Display]` on the model, because validation messages use it too."
- After: "To label a field, prefer the `[Display]` attribute on the property over setting `Title`, so that the label and the validation messages use the same name."

When the advice has no alternative to prefer, phrase it as a suggestion, with the reason.

- Before: "Set a `Title` on a field that reads through a navigation property, because…"
- After: "We suggest you set a `Title` on a field that reads through a navigation property, because…"

### Explain a problem from its cause

When you warn about a problem, say what causes it before the symptom and the fix. Without the cause, the reader can't tell whether their case is affected.

- Before: "StellarAdmin calls the factory when it displays the create page, and again when someone saves it, so a value the factory generates is generated twice."
- After: "StellarAdmin calls the factory twice: when someone opens the create page, and again when they save. A reference that the factory generates randomly therefore gets a different random value on each call."

### Don't explain what the framework already documents

Name the framework mechanism StellarAdmin uses, such as ASP.NET Core model validation, and cover only what StellarAdmin adds or changes. Point to tools that plug into the mechanism, so the reader knows they work.

- Before: "It checks data annotations, such as `[Required]`, `[StringLength]`, `[Range]` and `[EmailAddress]`, and calls `Validate()` on a model that implements `IValidatableObject`."
- After: "StellarAdmin runs ASP.NET Core model validation before it saves the record.", and a callout that validation libraries that plug into the pipeline, such as FluentValidation, work too.

### Split a page that covers two subjects

When a page covers two subjects that a reader looks for separately, make two pages. An intro that has to join them, such as "You can set the values a new record starts with, and add your own checks", is a sign.

- Before: "Defaults and validation"
- After: "Setting default values" and "Validating forms"

### Recommend the accepted practice first

When there are several ways to do something, lead with the one most developers should use. Put the alternatives and workarounds after it.

- Before: a custom data source first, and a cascade delete last.
- After: a cascade delete first, then keeping the foreign key constraint, then a custom data source for your own message.

### State what's required before what's optional

Say what the reader must do first, then what depends on their choices. Introduce a type that combines others after the reader has seen the parts.

- Before: "A data source implements one interface for each page it supports. To support every page, implement `IResourceCrudDataSource<T>`, which combines them:"
- After: "Every data source must implement `IResourceDataSource<T>`, which lists the records for the index page. For everything else you enable, the data source must also implement the matching interface:", with `IResourceCrudDataSource<T>` after the table.

### Make a statement cover everything it's about

When a statement covers several things, check that its wording fits each of them. Deleting is an action, not a page.

- Before: "Each method takes a callback that configures the page."
- After: "Each method takes a callback that configures the page or action."

When a statement applies to only one case, say what happens in the others, so the reader doesn't have to infer it.

- Before: "The filter, for an EF Core resource. Leave it out for a scope that displays every record."
- After: "For an EF Core resource, the filter. Leave it out for a scope that displays every record. A custom data source takes no filter, because it filters the records itself."

### Put braces in MDX prose in inline code

MDX runs `{…}` in prose as JavaScript, so a placeholder like `{singular}` crashes the page with a `ReferenceError`. Put it in inline code, where it's text.

- Before: patterns like "Create {singular}"
- After: patterns like `Create {singular}`

### Write on-screen labels in bold

When you name a label the reader sees on screen, such as a button, a tab or a menu item, write it in bold, capitalized as it appears. Use quotation marks for literal strings, such as a default value, and italics only to introduce a term.

- Before: "the All and Pending scopes", "the Create button"
- After: "the **All** and **Pending** scopes", "the **Create** button"
- Unchanged: "`CreateLabel` defaults to "Create"", because that's a string value.

### Write keystrokes with `<kbd>`

Put each key in a `<kbd>` element, named as it's printed on the keyboard, and join a combination with "+" and no spaces. Write the Mac key as "Command", and the Escape key as "Esc". Use "press" for keys.

- Before: "Pressing Enter in an input, or Ctrl+Enter (Cmd+Enter on a Mac) in any field, saves the row. Pressing Escape cancels."
- After: "Press `<kbd>Enter</kbd>` in an input, or `<kbd>Ctrl</kbd>+<kbd>Enter</kbd>` (`<kbd>Command</kbd>+<kbd>Enter</kbd>` on a Mac) in any field, to save the row. Press `<kbd>Esc</kbd>` to cancel."

### Don't use "display" as a verb next to a display

A *display*, such as `NumberDisplay`, is what renders a value, the counterpart of an editor, as in ASP.NET's `DisplayFor()` and `EditorFor()`. When a sentence names a display, use "render" or "show" for the verb, so the reader doesn't have to work out which "display" is the noun. Call it a "display type" the first time a page mentions it.

- Before: "`UseDisplay<TDisplay>()` | Displays the value with a different display."
- After: "`UseDisplay<TDisplay>()` | Renders the value with a different display type."

### Open a section with the problem it solves

Say what situation the reader is in before what to do about it, so they know whether the section applies to them.

- Before: "When the details need data that the row's record doesn't have, load the data when someone expands the row, so that the index page doesn't load it for every row."
- After: "Your details may need data that isn't part of the row's record, such as related records or data from another service. Instead of loading this data for every row on the index page, StellarAdmin can load it only when someone expands a row."

### Document each builder's settings where it's covered

Don't refer the reader to another section's table for the settings a builder shares. Give the builder its own example and table, so the reader sees what's available without assembling it.

- Before: "The `UseInlineRow()` callback of `AllowEdit()` takes the same `DimOtherRows` and `AfterSave` settings."
- After: a "Configuring the edited row" section with its own example and a table of `DimOtherRows` and `AfterSave`.

### Show yes/no comparisons as a table with ticks and crosses

When the reader needs to know whether each item in a list supports something, put the items in a table, one row each, and use the `<Yes />` and `<No />` MDX components for the answer. They render a tick or a cross, with the word for screen readers.

- Before: "These editors work in a cell: …" and "These editors don't work in a cell: …"
- After: a table with the editor, its method, and `<Yes />` or `<No />` under "Works in a cell".

### Don't describe a call the example right below makes obvious

When the example that follows plainly shows a call, don't spell it out in the prose before it as well. Keep the sentence where the call isn't obvious from the example.

- Before: "To add the page, call `AllowView()` on the resource. In its callback, call `Fields()` to add the page's fields. The example below adds a view page for bookings…"
- After: "To add the page, call `AllowView()` on the resource. The example below adds a view page for bookings…"

### State the condition, not "then"

Don't make the reader work out what "then" follows from. Name the condition.

- Before: "The index page then displays a view button, with an eye icon, on each row."
- After: "When a view page is enabled, the index page displays a view button on each row."

Leave out visual details the UI makes obvious, such as the icon on a button.

### Describe configuration as what's enabled, not what people can do

"People can" reads as a permission, which the reader may take to mean authorization. When a behavior depends on configuration, say what's enabled.

- Before: "if people can [delete](/docs/admin/delete) records, a **Delete** button."
- After: "if [deleting is enabled](/docs/admin/delete), a **Delete** button."

### Keep code lines short enough to read without scrolling

A long line scrolls sideways in the code block, and the reader loses the end of it. When a call with a callback doesn't fit on one line, put the callback's body in a block.

- Before: `fields.Add(trip => trip.Summary).UseTextareaEditor(textarea => textarea.ClassNames.Control = "trip-summary");`
- After:

  ```cs
  fields.Add(trip => trip.Summary).UseTextareaEditor(textarea =>
  {
      textarea.ClassNames.Control = "trip-summary";
  });
  ```

### Say which part of the UI a call configures

When a call configures part of an editor or a page, name that part. A word like "keys" or "values" could mean the data instead.

- Before: "call `Key()` and `Value()` to configure the keys and the values."
- After: "call `Key()` and `Value()` to configure the key and the value columns of the editor."

### Put setup outside StellarAdmin in a callout

When the reader must do something outside StellarAdmin for a feature to work, such as configuring EF Core, put it in an `idea` callout so it stands out from how StellarAdmin behaves. A note that asks nothing of the reader stays in the prose.

- Before: "EF Core doesn't map a dictionary property on its own. Store the dictionary in a JSON column, or give the property a value converter.", as a paragraph.
- After: the same text in an `idea` callout.
- Unchanged: "EF Core maps a list of strings or numbers as a primitive collection, so the property needs no extra configuration."

### Start with the required call on its own

Show the smallest example that works first. Then add each optional call in its own step, with the condition for using it, so the reader can tell what they need.

- Before: the first image editor example set both `StoreImage()` and `ResolveUrl()`.
- After: the first example sets only `StoreImage()`, and `ResolveUrl()` follows with its own example.

### Say when an optional call is needed, not when to leave it out

Give the condition under which the reader needs the call. A condition for leaving it out makes them reverse it.

- Before: "If the property stores the image's URL, leave `ResolveUrl()` out."
- After: "If the property value isn't a URL the browser can load, specify a `ResolveUrl()` function that turns the property value into one."

Don't explain what happens when the reader leaves out a call they'd have no reason to make.

- Removed: "When you call the editor's method without `UseItems()`, the editor lists the records of the related resource:"

### Keep a page to its own subject

Content about another feature belongs on that feature's page. Where the two must be configured together, keep one sentence that says so and links to the other page.

- Before: an `IconDisplay` example and a "Displaying the icon's name" section on the icon editor page.
- After: both moved to the displays page.
- Kept, on the image editor page: "To display the image in the grid or on the view page, use the `ImageDisplay` display type, with the same `ResolveUrl()` function."

### Don't explain something by comparing it with another page

The reader may not have read the other page, so a comparison with it confuses rather than explains.

- Removed, on the rich text editor page: "The rich text editor has no `ResolveUrl()`, because the URL is stored in the HTML."

### Present an option StellarAdmin doesn't choose as available, with how to use it

When StellarAdmin never picks an option by default, say that the option exists and name the call that uses it. "Never chooses" makes it sound like a limitation, and doesn't tell the reader what to call.

- Before: "StellarAdmin never chooses `ImageDisplay` or `IconDisplay`, so you must choose them yourself."
- After: "StellarAdmin also supports `ImageDisplay` and `IconDisplay`, which must be specified explicitly using `UseDisplay()`."

### State a requirement before what happens if it's not met

Say what the reader must do first, addressed to them, then what StellarAdmin does when they don't.

- Before: "An editor that selects one value, such as a select, can't save several records. If the field uses one, StellarAdmin leaves the field off the form…"
- After: "The editor you configure with a many-to-many navigation must support selecting multiple records. If you specify an editor that only supports selecting a single record, such as a select, StellarAdmin leaves the field off the form…"

### Describe limitations as what's supported

Say what StellarAdmin supports and what isn't supported. StellarAdmin doesn't edit records, people do, so it "allows editing".

- Before: "StellarAdmin edits a many-to-many navigation… It can't edit these, and leaves the field off the form:"
- After: "StellarAdmin allows editing a many-to-many navigation… The following is not supported:"

### Describe what the reader's code receives, not how StellarAdmin builds it

When a delegate or a handler receives a value, say what the value contains. How StellarAdmin creates it is an internal detail.

- Before: "The `destination` that the filter receives isn't the record people are editing. When StellarAdmin loads the options, it creates a new `Destination` with only the values…"
- After: "The `destination` instance passed to the filter contains only the values…"

### Say what a call is for, and let the example show its arguments

When an example follows, say what the call achieves. Listing its type parameters and delegates in prose repeats the code.

- Before: "call `UseText()` with the type of the records, and a function that returns the text of each badge"
- After: "call `UseText()` to specify the text displayed inside the badge"

### Say where something appears

When StellarAdmin opens or displays something, say where, so the reader can picture it.

- Before: "When people click the image, StellarAdmin opens a larger version."
- After: "When people click the image, StellarAdmin opens a larger version in a popup."

### Don't assume the reader used a related feature

A reader can arrive at a page without using the feature next to it, such as an image display without the image editor. Don't phrase an instruction as reusing something they may not have.

- Removed: "Use the same function you passed to the image editor"

### Qualify a noun when the page has two of the same kind

When a page involves two things of the same kind, such as a field and the field it depends on, a bare noun could mean either. Use the qualified term.

- Before: "Changing the text of a disabled field", "a field is disabled"
- After: "Changing the text of a disabled dependent field", "a dependent field is disabled"

### Describe what's on screen, not the person

When something represents a person, such as the signed-in user, say what's displayed. "Displays them" reads as if the person were on screen.

- Before: "When someone is signed in, StellarAdmin displays them at the bottom of the sidebar, with their avatar, name and email address. Clicking them opens the *user menu*…"
- After: "When someone is signed in, the bottom of the sidebar displays their avatar, name and email address. Clicking these opens the *user menu*…"

### Give the instruction, not StellarAdmin's reasoning

Say what the reader does. Don't explain it with what StellarAdmin can't know. Say what StellarAdmin doesn't do only when the reader would expect it to (see "Say what StellarAdmin doesn't do, when the reader must do it").

- Before: "StellarAdmin doesn't know which of your environments needs one, so call it only in those environments."
- After: "Call it only in the environments that should display an indicator."

## Style

Choices made on feel. Follow them, but they aren't rules to argue from.

- Vary "app" and "application" to avoid repeating a word in the same sentence: "It runs inside your application, and uses your app's `DbContext`."
- Write "the StellarAdmin.UI components", with "the".
- Write "admin panel" in lower case.
- "Run the application", not "Run the app".
- Don't restate what happens if a step is skipped once the step already says to do it. This is judgement, not a rule: if something becomes a common installation issue, call it out more.
- Avoid marketing words like "rapidly".
- Use American spelling: "color", "recognize", "behavior".
- "Unless specified otherwise", not "Unless you say otherwise".
- Write "must", not "has to": "The default sort must be on a sortable column."
- When the paragraph before an example is long, put "The example below…" in its own paragraph.
- Prefer "display" over "show" for what appears on screen: "Radio groups and checkbox groups display their options in a single column." "Display" needs an object or the passive ("the media is displayed"), so rephrase rather than write "the option displays". Keep "show" where it pairs with "hide" or matches an API name like `ShowMedia`, and use "render" or "show" in a sentence that names a display type.
- Prefer naming the things over a vague stand-in like "what to return", but go by what reads best: "StellarAdmin passes the requested search, scope, sort and page, and the keys of the records to return, via the properties of the `request` parameter", not "StellarAdmin passes what to return via the properties of the `request` parameter".
- Write "rendered using" a partial view, not "rendered from": "The details are rendered using a partial view that you write."
- Describe what people see in plain words: "An inline grid of icons", not "A tile for each icon".
- Drop words that repeat what's already said: "StellarAdmin loads the related record", not "StellarAdmin loads the related record with the record".
- Write "such as", not "e.g.".
- Keep page titles short. A title doesn't need to name everything on the page: "Links and emails" covers phone numbers too.
- Describe a dependency with "depending on", and "use" a custom display rather than "write" one: "To vary a badge's variant depending on its value, use a custom display", not "To choose each badge's variant from its value, write a custom display."
- Name the kind of element along with its label: "The **Log out** item posts to…", not "**Log out** posts to…".
- Write "for all resources", not "for every resource": "To change the default text for all resources, call `ConfigureResourceLabels()`."
