# Handlers name their model once

Status: in progress (2026-10-23); step 1 committed in `7cd60d4`, step 2 done, uncommitted. Stage F of [configuration simplification](configuration-simplification.md#f-handlers-name-their-model-once), on the `configuration-simplification` branch.

## Goal

Calls that take a handler stop naming its model a second time, where that doesn't cost type checking or make the call longer.

## Where the model is named twice today

| Call | Type arguments | Is the model used apart from the handler? |
| --- | --- | --- |
| `AllowCreate<TModel, THandler>` | 2 | Yes: the create builder is `ResourceCreateBuilder<TModel>`, so `fields.Add(model => model.Email)` needs it |
| `AllowEdit<TModel, THandler>` | 2 | Yes: `ResourceEditBuilder<TModel>` |
| `AllowView<TModel, THandler>` | 2 | Yes: `ResourceViewBuilder<TModel>` |
| `EnableRowDetails<TModel, THandler>(partialName)` | 2 | No: the rows builder is `ResourceRowDetailsBuilder<TResource>`, and the model only reaches the partial |
| `panels.Add<TModel, THandler>(title, partialName)` | 2 | No: the panel builder isn't generic |

C# can't infer `TModel` from `THandler`'s interface, and can't take one type argument and infer the other (the spike for stage A confirmed this). So a create, edit or view page must name the model for its fields, and naming the handler too needs a second type argument or a second call.

## Options for create, edit and view

1. **Keep `AllowCreate<CreateUserModel, CreateUserHandler>(...)`.** The two type arguments name different things: the form's model and the code that saves it. This is the MediatR shape (`IRequestHandler<TRequest, TResponse>` names the request again).
2. **`AllowCreate<CreateUserModel>(create => create.UseHandler<CreateUserHandler>().Fields(...))`.** `UseHandler<THandler>() where THandler : class, IResourceCreateHandler<TModel>` is checked by the compiler. One type argument per call, but one more call, so the registration gets longer, not shorter.
3. **`AllowCreate<CreateUserModel>(...)` with the handler resolved as `IResourceCreateHandler<CreateUserModel>`**, registered by the app (`services.AddScoped<IResourceCreateHandler<CreateUserModel>, CreateUserHandler>()`). A missing registration would be a configuration problem that makes the page unavailable. The handler moves to `Program.cs`, so the registration no longer shows what saves the form.

Recommendation: option 1, so create, edit and view stay as they are. Jerrie chose option 1.

## Row details and panels

The model isn't used apart from the handler, so the handler alone is enough:

```csharp
// Before
index.EnableRowDetails<BookingNotes, BookingNotesHandler>("_BookingNotes");
panels.Add<BookingNotes, BookingNotesHandler>("Notes", "_BookingNotes");

// After
index.EnableRowDetails<BookingNotesHandler>("_BookingNotes");
panels.Add<BookingNotesHandler>("Notes", "_BookingNotes");
```

- The model type is read from the handler's `IResourceRowDetailHandler<TModel>` by reflection, and the loader is created for it.
- The constraint can only be `where THandler : class`, so the compiler no longer checks that the type is a handler. A type that implements no `IResourceRowDetailHandler<>`, or more than one, is a configuration problem (`UnusableRowDetailHandler`, a warning like the other row-detail problems): the rows don't expand, or the panel is left out.
- The `<TModel, THandler>` overloads are removed (the branch isn't released).

Recommendation: do this part, if the lost compile-time check is acceptable. Otherwise close stage F with no change.

## Steps

1. Core: replace `EnableRowDetails<TModel, THandler>` (both overloads) and `ResourceRowDetailPanelsBuilder.Add<TModel, THandler>` with the handler-only forms, add the problem, update `PublicAPI.Unshipped.txt`, migrate the tests and add HTTP tests for the problem.
2. Playground, consumer references (`skills/stellar-admin/references/index-pages.md`) and docs; check the website docs for the old forms.
3. Close this plan and stage F.

## Decisions (Jerrie)

1. Create, edit and view keep `<TModel, THandler>` (2026-10-23).
2. Row details and panels take only the handler, accepting the lost compile-time check (2026-10-23).

## Progress

- Step 1 (2026-10-23): `EnableRowDetails<THandler>` (both overloads) and `panels.Add<THandler>` replace the `<TModel, THandler>` forms. `HandlerRowDetailLoader.Create` reads the model from the handler's interface and registers the handler; a handler naming no model, or several, gets a stand-in loader that `RowDetailsCheck` removes and reports as `UnusableRowDetailHandler` (1067). The tests and the playground (`CategoryResourceRegistration`, `ProductResourceRegistration`) use the new forms, and two HTTP tests cover the problem. Verified: `dotnet test --solution StellarAdmin.slnx --configuration Release`, 1889/1889 passed. The playground compiles; it wasn't run.
- Step 2 (2026-10-23): the index pages reference (`skills/stellar-admin/references/index-pages.md`) shows the handler-only forms and says a handler for no model, or two, leaves its panel out. No other maintained docs, skill evals or website docs use the old forms (website checked read-only, clean). Verified: skill validator (4 skills, 92 files) and `SkillsGenerator -- --check` (no drift) pass; the playground on 5207 logs no row-detail problems, and `/stellaradmin/categories/rowdetail/8` and `/stellaradmin/products/rowdetail/7?panel=related` return 200 with their handler's model. The server is stopped, and `app.db` is unchanged.
