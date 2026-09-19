# C# file organization

Member ordering for C# types in all StellarAdmin repos. Applies to new code and to files being substantially edited — do **not** mass-reorder untouched files just to comply (keep diffs reviewable). CSharpier handles formatting only and never reorders members, so there is no tooling conflict.

## Language features

Prefer modern C# features supported by the repository's pinned SDK when they simplify code. Use `field` for property backing storage, target-typed `new` when the type is clear, and primary constructors where they preserve the intended API and accessibility. Keep the SDK's stable language default rather than setting `LangVersion` to `latest` or enabling preview features.

## Section order within a type

1. **Constants**
2. **Fields** — placement matches existing code: private backing fields at the top
3. **Properties**
4. **Constructors** — fewest parameters to most
5. **Methods** — public first, then protected and private **as a single group**
6. **Nested types**

## Ordering within a section

- **Access:** public members before non-public. For methods, protected + private form one trailing group; the same public-first rule applies to constants, fields, and properties.
- **Static:** statics do not get their own section — within a section (and access group), static members come before instance members.
- **Alphabetical** within each resulting group.
- **Overloads** (same name): sorted by parameter list length, ascending. Constructors follow the same idea (fewest → most parameters).

## Example skeleton

```csharp
public class Example
{
    private const string DefaultName = "…";          // consts

    private readonly IService _service;              // fields

    public string Name { get; set; }                 // properties: public, alphabetical
    internal bool IsDirty { get; set; }              // then non-public

    public Example() { }                             // ctors: fewest → most params
    public Example(IService service) { }

    public void Refresh() { }                        // public methods, alphabetical
    public void Save() { }
    public void Save(bool force) { }                 // overloads: by param count

    protected virtual void OnSaved() { }             // protected + private: one group,
    private static string Normalize(string s) { }    // static before instance,
    private void Validate() { }                      // alphabetical
}
```

## Method formatting

Always use braces for `if` and `else` bodies, including a single statement. Do not rely on CSharpier to add them.

Group statements that perform one logical step together, and separate those groups with a blank line. This applies to all methods, constructors, configuration lambdas, and top-level code, including tests. Keep related statements together; do not insert a blank line between every statement. Separate a completed conditional block from the next independent section or conditional, and separate the final return from the work that precedes it. Review this grouping explicitly before handing off code; CSharpier cannot decide where logical sections belong.

For example, resource registration has separate groups for argument validation, duplicate-registration checks, options construction/configuration/registration, sidebar registration, controller registration, authorization configuration, and the final return. Keep the statements within each group together:

```csharp
var options = new EfCoreResourceOptions<TContext, TEntity>(name);
configure(new EfCoreResourceBuilder<TContext, TEntity>(options));
builder.Services.AddSingleton(options);

builder.Services.AddSingleton<ISidebarItemsProvider>(
    new ResourceSidebarProvider(name, () => options.IndexPage.EffectiveTitle)
);

var controller = typeof(EfCoreResourceController<TContext, TEntity>);
builder
    .AddApplicationPart(typeof(StellarAdminDashboardBuilderExtensions).Assembly)
    .AddController(controller, name);
```

In controllers, separate validation, entity creation/loading, binding, persistence, and response construction. In tests, follow the [unit testing conventions](unit-testing.md): one behavior per test method, with explicit `// Arrange`, `// Act`, and `// Assert` sections separated by blank lines. In Tag Helper `ProcessAsync` methods, separate resolved configuration/context setup, output element name/mode, attribute configuration, styling helpers, child-content rendering, and the final return. Use `CarouselTagHelper` as a concrete reference for Tag Helper spacing.

```csharp
var effectiveOrientation = Orientation ?? CarouselOrientation.Horizontal;

output.TagName = "div";
output.TagMode = TagMode.StartTagAndEndTag;

output.Attributes.SetAttribute("data-slot", "carousel");
if (!output.Attributes.ContainsName("role"))
{
    output.Attributes.SetAttribute("role", "region");
}

output.Attributes.SetAttribute("class", JoinCssClasses("sa-carousel", output.GetUserSuppliedClass()));

return Task.CompletedTask;
```
