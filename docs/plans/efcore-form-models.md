# Custom create and edit models on EF Core resources

Status: proposed, being refined (2026-11-01).

Let an EF Core resource use a form model with extra fields, and run code when it saves, without giving up the EF data source. Today one extra field (a reason for the change, a password confirmation) means `AllowEdit<TModel, THandler>`, and the handler then loads, maps and saves by hand, losing what `EfCoreResourceDataSource` does: copying only the configured fields onto the tracked entity, many-to-many fields, the includes for lookups and display fields, and the concurrency and refused-delete messages. Covers create and edit saves from the "Save hooks" row of the [admin panel backlog](admin-panel-backlog.md); delete and list events are out of scope.

Motivating cases: an internal app that asks for a reason on every change and writes it to a separate audit table, and the playground's users, which save through `UserManager`.

## Decisions (Jerrie)

1. Events live in StellarAdmin.EntityFrameworkCore only. A custom data source is the developer's own code and can use its own models (2026-11-01).
2. Events can be set as delegates as well as an events class (2026-11-01).
3. A wrapped form model names its entity property with an explicit argument (`model => model.Product`), not by convention (2026-11-01).
4. Events sit only on the create and edit builders, typed to the form's model. There are no resource-level events (query, delete, or saves from any form) (2026-11-01).
5. The core create and edit builders are unsealed with a protected constructor, so the EF builders derive from them and add `Events` and `UseEvents` (2026-11-01).
6. Fields under a wrapped model's entity are validated by attributes, as today: on the entity, or through `IValidatableObject` on the form model for properties that can't be annotated (2026-11-01). Reading rules from the EF model is out of scope: [Validation from the EF model](efcore-model-validation.md).
7. The data source runs the `-ing` events and the save in one transaction, so an event that saves more than once (such as `UserManager`) doesn't open its own (2026-11-01).
8. The after-save events (`Created`, `Updated`) have no `Fail` or `Handled`: the record is already saved (2026-11-01).
9. `IResourceCreateHandler` and `IResourceEditHandler` stay; they remain useful for custom data sources. Retire them later only if they turn out not to be needed (2026-11-01).
10. Inline create and edit rows support wrapped models: columns match wrapped fields without the entity prefix, and a required form-only field with no column is a startup configuration problem, like `InlineRowCheck`'s required-property check (2026-11-01).
11. `UseFactory` stays in core. The EF create events add `OnInitialized`, which runs after the factory (or `new TModel()`) and before the form is shown, for defaults that need services or async code. It runs wherever a create form is shown (the create page, a new inline row, the create sheet from a lookup, the next form after Save and add another), but not for the model used to load a field's items. Values not on the form, such as `CreatedBy`, belong in `OnCreating` (2026-11-01).
12. Plain `AllowCreate`/`AllowEdit` on an EF resource get the same events, with the entity as the model (`context.Model` and `context.Entity` are the same object), so an audit row or `CreatedBy` doesn't need a wrapper model (2026-11-01).
13. `Fail` takes the field as an expression on the form's model (`context.Fail(model => model.User.Email, "...")`), plus `Fail(message)` for an error on the whole form (2026-11-01).
14. An exception in an after-save event (`OnCreated`, `OnUpdated`) is logged, and the page continues as saved (2026-11-01).

## Proposal

- **Wrapped form models**: a create or edit form binds a model that holds the entity in a property, plus fields that only exist on the form. The EF data source loads the entity into the property and copies the configured fields under it onto the tracked entity, as it does today; the other fields reach the events. Nothing is mapped by hand.
- **Form events** on the create and edit builders, typed to the form's model and the entity: create has `OnInitialized`, `OnCreating`, `OnCreated`; edit has `OnLoaded`, `OnUpdating`, `OnUpdated`.
- The `-ing` events run after the form's values are on the tracked entity, inside a transaction the data source opens, before `SaveChanges`; anything they add to the context is saved with the record. They can reject the save with an error on the form, or take over the save.

Core changes only by unsealing the create and edit builders. The EF overloads set the existing `CreateHandler`, `EditLoader` and `EditHandler` delegates on `ResourceOptions<TResource>`.

## Proposed API

### Builders

```csharp
// EfCoreResourceBuilder<TEntity>, beside the core AllowCreate<TModel, THandler> and AllowEdit<TModel, THandler>
public EfCoreResourceBuilder<TEntity> AllowCreate<TModel>(
    Expression<Func<TModel, TEntity>> entity, Action<EfCoreResourceCreateBuilder<TEntity, TModel>> configure);
public EfCoreResourceBuilder<TEntity> AllowEdit<TModel>(
    Expression<Func<TModel, TEntity>> entity, Action<EfCoreResourceEditBuilder<TEntity, TModel>> configure);
// Plain AllowCreate()/AllowEdit() return EfCoreResourceCreateBuilder<TEntity, TEntity> and EfCoreResourceEditBuilder<TEntity, TEntity>

public sealed class EfCoreResourceEditBuilder<TEntity, TModel> : ResourceEditBuilder<TModel>
{
    public EfCoreResourceEditBuilder<TEntity, TModel> Events(Action<EfCoreEditEvents<TEntity, TModel>> configure);
    public EfCoreResourceEditBuilder<TEntity, TModel> UseEvents<TEvents>() where TEvents : EfCoreEditEvents<TEntity, TModel>;
}
// EfCoreResourceCreateBuilder<TEntity, TModel> : ResourceCreateBuilder<TModel>, the same with EfCoreCreateEvents<TEntity, TModel>
```

### Events types

Shaped like ASP.NET Core's `JwtBearerEvents`: delegate properties, and virtual methods that call them, so the same type serves inline delegates and a subclass resolved from DI (registered scoped if it isn't already).

```csharp
public class EfCoreEditEvents<TEntity, TModel> where TEntity : class
{
    public Func<EfCoreLoadedContext<TEntity, TModel>, CancellationToken, Task> OnLoaded { get; set; }
    public Func<EfCoreSaveContext<TEntity, TModel>, CancellationToken, Task> OnUpdating { get; set; }
    public Func<EfCoreSavedContext<TEntity, TModel>, CancellationToken, Task> OnUpdated { get; set; }

    public virtual Task LoadedAsync(EfCoreLoadedContext<TEntity, TModel> context, CancellationToken cancellationToken) =>
        OnLoaded(context, cancellationToken);
    // ... one virtual method per event
}
// EfCoreCreateEvents<TEntity, TModel>: OnInitialized, OnCreating, OnCreated

public sealed class EfCoreSaveContext<TEntity, TModel> where TEntity : class
{
    public DbContext Db { get; }
    public TEntity Entity { get; }
    public EntityEntry<TEntity> Entry { get; }   // original and current values, for audits
    public TModel Model { get; }
    public IServiceProvider Services { get; }

    public void Fail(string message);                                                // reject with a form error; the transaction is rolled back
    public void Fail<TValue>(Expression<Func<TModel, TValue>> field, string message); // reject with an error on a field
    public void Handled();                                  // the event saved the record itself; skip SaveChanges
}
```

## Examples

### A reason for every change

```csharp
public sealed class EditProductModel
{
    public Product Product { get; set; } = null!;

    [Required, StringLength(500)]
    public string Reason { get; set; } = "";
}

resource.AllowEdit<EditProductModel>(model => model.Product, edit =>
{
    edit.Fields(fields =>
    {
        fields.Add(model => model.Product.Name);
        fields.Add(model => model.Product.Price);
        fields.Add(model => model.Product.Details.Sku);
        fields.Add(model => model.Product.CategoryId);
        fields.Add(model => model.Product.Tags).UseMultiLookupSheetEditor(UseTags);
        fields.Add(model => model.Reason);
    });

    edit.Events(events =>
        events.OnUpdating = (context, _) =>
        {
            context.Db.Add(new ProductAudit
            {
                ProductId = context.Entity.Id,
                Reason = context.Model.Reason,
                ChangedAt = DateTimeOffset.UtcNow,
            });

            return Task.CompletedTask;
        }
    );
});
```

On save: `[Required]` rejects an empty reason; the data source copies the five product fields onto the tracked product (`Reason` isn't on the entity); `OnUpdating` adds the audit row; one `SaveChanges` saves both.

### Users through UserManager, with a password confirmation

`ApplicationUser` is in `ApplicationDbContext`, so users become an EF resource and `CreateUserHandler` goes away.

```csharp
public sealed class CreateUserModel
{
    public ApplicationUser User { get; set; } = new() { FirstName = "", LastName = "" };

    [Required, DataType(DataType.Password)]
    public string Password { get; set; } = "";

    [Required, Compare(nameof(Password)), DataType(DataType.Password)]
    [Display(Name = "Confirm password")]
    public string PasswordConfirmation { get; set; } = "";

    [Display(Name = "Roles")]
    public string[] RoleIds { get; set; } = [];
}

resource.AllowCreate<CreateUserModel>(model => model.User, create =>
{
    create.Fields(fields =>
    {
        fields.AddSection("Account", section =>
        {
            section.Add(model => model.User.Email);
            section.Add(model => model.User.EmailConfirmed);
        });
        fields.AddSection("Profile", section =>
        {
            section.AddGroup(group =>
            {
                group.Columns(2);
                group.Add(model => model.User.FirstName);
                group.Add(model => model.User.LastName);
            });
            section.Add(model => model.User.DepartmentId, field =>
            {
                field.Title = "Department";
                field.UseSelectEditor(options => options.UseResource<Department>());
            });
        });
        fields.AddSection("Roles", section =>
            section.Add(model => model.RoleIds, field =>
                field.UseCheckboxGroupEditor(options => options.UseResource<ApplicationRole>())));
        fields.AddSection("Password", section =>
            section.AddGroup(group =>
            {
                group.Columns(2);
                group.Add(model => model.Password);
                group.Add(model => model.PasswordConfirmation);
            }));
    });

    create.Events(events =>
        events.OnCreating = async (context, cancellationToken) =>
        {
            var users = context.Services.GetRequiredService<UserManager<ApplicationUser>>();
            var user = context.Entity;
            user.UserName = user.Email;

            var result = await users.CreateAsync(user, context.Model.Password);
            if (result.Succeeded && context.Model.RoleIds.Length > 0)
            {
                var roleNames = await context.Db.Set<ApplicationRole>()
                    .Where(role => context.Model.RoleIds.Contains(role.Id))
                    .Select(role => role.Name!)
                    .ToListAsync(cancellationToken);
                result = await users.AddToRolesAsync(user, roleNames);
            }

            if (!result.Succeeded)
            {
                foreach (var error in result.Errors)
                {
                    context.Fail(error.Description);
                }

                return;
            }

            context.Handled();
        }
    );
});
```

`[Compare]` checks the confirmation before the event runs. The data source's transaction covers both `UserManager` saves; `Fail` rolls back a user created before `AddToRolesAsync` failed.

### Editing users and their roles

`RoleIds` is form-only: roles are saved through `UserManager`, not a navigation. `OnLoaded` fills it, and `OnUpdating` saves the user and syncs the roles through `UserManager`, which normalizes the user name and email. `UserManager`'s store uses the same scoped `ApplicationDbContext`, so `context.Entity` is the user it updates.

```csharp
public sealed class EditUserModel
{
    public ApplicationUser User { get; set; } = null!;

    [Display(Name = "Roles")]
    public string[] RoleIds { get; set; } = [];
}

resource.AllowEdit<EditUserModel>(model => model.User, edit =>
{
    edit.Fields(fields =>
    {
        fields.Add(model => model.User.Email);
        fields.Add(model => model.User.FirstName);
        fields.Add(model => model.User.LastName);
        fields.Add(model => model.RoleIds, field =>
            field.UseCheckboxGroupEditor(options => options.UseResource<ApplicationRole>()));
    });

    edit.Events(events =>
    {
        events.OnLoaded = async (context, cancellationToken) =>
            context.Model.RoleIds = await context.Db.Set<IdentityUserRole<string>>()
                .Where(role => role.UserId == context.Entity.Id)
                .Select(role => role.RoleId)
                .ToArrayAsync(cancellationToken);

        events.OnUpdating = async (context, cancellationToken) =>
        {
            var users = context.Services.GetRequiredService<UserManager<ApplicationUser>>();
            var user = context.Entity;
            user.UserName = user.Email;

            var result = await users.UpdateAsync(user);
            if (result.Succeeded)
            {
                var roles = await context.Db.Set<ApplicationRole>().ToListAsync(cancellationToken);
                var current = await users.GetRolesAsync(user);
                var selected = roles.Where(role => context.Model.RoleIds.Contains(role.Id)).Select(role => role.Name!).ToList();
                result = await users.RemoveFromRolesAsync(user, current.Except(selected));
                if (result.Succeeded)
                {
                    result = await users.AddToRolesAsync(user, selected.Except(current));
                }
            }

            if (!result.Succeeded)
            {
                foreach (var error in result.Errors)
                {
                    context.Fail(error.Description);
                }

                return;
            }

            context.Handled();
        };
    });
});
```

### Recording who created a record, on a plain form

No wrapper model: `context.Model` and `context.Entity` are both the `Article`. `CreatedBy` and `CreatedAt` aren't on the form.

```csharp
resource.AllowCreate(create =>
{
    create.Fields(fields =>
    {
        fields.Add(article => article.Title);
        fields.Add(article => article.Body);
    });

    create.Events(events =>
        events.OnCreating = (context, _) =>
        {
            var user = context.Services.GetRequiredService<IHttpContextAccessor>().HttpContext!.User;
            context.Entity.CreatedBy = user.Identity!.Name!;
            context.Entity.CreatedAt = DateTimeOffset.UtcNow;

            return Task.CompletedTask;
        }
    );
});
```

### Defaults that need services

`UseFactory` can't reach services; `OnInitialized` can. The user sees the default and can change it.

```csharp
resource.AllowCreate(create =>
{
    create.Fields(...);
    create.Events(events =>
        events.OnInitialized = async (context, cancellationToken) =>
            context.Entity.TimeZoneId = await context.Db.Set<Setting>()
                .Select(setting => setting.DefaultTimeZoneId)
                .FirstAsync(cancellationToken)
    );
});
```

### Blocking a save

`Entry` has the stored values, so a rule can compare old and new. The error shows on the field, and nothing is saved.

```csharp
resource.AllowEdit(edit =>
{
    edit.Fields(...);
    edit.Events(events =>
        events.OnUpdating = (context, _) =>
        {
            var original = context.Entry.Property(product => product.Price).OriginalValue;
            if (context.Entity.Price < original / 2)
            {
                context.Fail(product => product.Price, "The price can't drop by more than half in one change.");
            }

            return Task.CompletedTask;
        }
    );
});
```

### A side effect after saving, chosen on the form

A form-only checkbox decides whether to send a welcome email. `OnCreated` runs after the commit, so the email never goes out for a user that wasn't saved.

```csharp
public sealed class CreateUserModel
{
    public ApplicationUser User { get; set; } = new() { FirstName = "", LastName = "" };

    [Display(Name = "Send welcome email")]
    public bool SendWelcomeEmail { get; set; } = true;

    // Password, PasswordConfirmation, RoleIds as in the create example
}

create.Events(events =>
{
    events.OnCreating = ...; // as in the create example
    events.OnCreated = async (context, cancellationToken) =>
    {
        if (context.Model.SendWelcomeEmail)
        {
            var email = context.Services.GetRequiredService<IWelcomeEmail>();
            await email.SendAsync(context.Entity, cancellationToken);
        }
    };
});
```

## Open questions

None at the moment.

## Left to the implementation

The implementing agent decides these, then records each choice in this plan:

- The steps and their order, the tests, and how much of the playground (such as the users), docs, consumer skills reference and `PublicAPI.Unshipped.txt` to update.
- The members of the contexts for `OnInitialized`, `OnLoaded` and the after-save events.
- On edit, whether `context.Model`'s entity property and `context.Entity` are the same object in `OnUpdating` (the submitted form binds a fresh, untracked entity whose fields are copied onto the tracked one). Users of the API see this choice.
- Whether `OnLoaded` runs again when a save fails validation and the form is shown again.
- The order of `OnInitialized` and the values `KeepForNext` carries over after Save and add another.
- What happens when an event calls `Handled()` and the save throws a concurrency or database error.

## Implementation notes

- Plain `AllowCreate`/`AllowEdit` on `EfCoreResourceBuilder<TEntity>` must return the EF builders. Overriding the parameterless forms with a covariant return works, but a second `AllowEdit(Action<EfCoreResourceEditBuilder<TEntity, TEntity>>)` beside the core `AllowEdit(Action<ResourceEditBuilder<TEntity>>)` is ambiguous for `edit => ...`; `ResourceBuilderBase` needs restructuring, such as type parameters for its create and edit builders like the existing `TBuilder`.

- `UpdateAsync` already walks nested property paths when copying fields, so `model => model.Product.Name` mostly works; the field-to-entity property lookup, the include paths for lookups and display fields, and the default labels need to strip the entity's prefix.
- Labels for entity fields come from the property name or attributes on the entity; a form model's `[Display]` no longer applies to them, so `DepartmentId` needs `field.Title`.
- `IValidatableObject` errors for a wrapped field (`User.Email`) must show under that field; check the key once wrapped fields are named.
- The transaction must go through the context's execution strategy (`Database.CreateExecutionStrategy()`), since a retrying strategy refuses user-started transactions, and must not start one when the context already has a current transaction.
- Events run inside `EfCoreResourceDataSource`; the wrapped-model overloads call internal data source methods that take the model and the entity selector.
