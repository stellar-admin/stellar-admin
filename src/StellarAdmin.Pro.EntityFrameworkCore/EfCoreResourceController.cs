using System.ComponentModel;
using System.Globalization;
using System.Linq.Expressions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewEngines;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.Extensions.Logging;
using StellarAdmin.Pro.Areas.StellarAdmin;
using StellarAdmin.Pro.Areas.StellarAdmin.ViewModels;
using StellarAdmin.Pro.Resources.Controllers;
using StellarAdmin.Pro.Resources.Infrastructure.Query;
using StellarAdmin.Pro.Resources.Options;

namespace StellarAdmin.Pro.EntityFrameworkCore;

[Area("StellarAdmin")]
internal sealed class EfCoreResourceController<TContext, TEntity>(
    TContext db,
    EfCoreResourceOptions<TContext, TEntity> options,
    ICompositeViewEngine views,
    ILogger<EfCoreResourceController<TContext, TEntity>> logger
) : ResourceControllerBase<TEntity>
    where TContext : DbContext
    where TEntity : class
{
    public async Task<IActionResult> Create(CancellationToken cancellationToken)
    {
        ValidateResource();

        return Page(
            "Create",
            await BuildReferenceForm(
                options.CreatePage,
                options.CreatePage.CreateInstance(),
                cancellationToken
            )
        );
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [ActionName("Create")]
    public async Task<IActionResult> CreatePost(CancellationToken cancellationToken)
    {
        ValidateResource();

        var entity = options.CreatePage.CreateInstance();

        await BindFormFieldsAsync(entity, options.CreatePage);

        var choices = await LoadChoices(options.CreatePage, cancellationToken);
        ValidateSelections(entity, options.CreatePage, choices);

        if (ModelState.IsValid)
        {
            db.Set<TEntity>().Add(entity);

            if (await Save(cancellationToken))
            {
                return RedirectToAction(nameof(Index));
            }
        }

        return Page(
            "Create",
            await BuildReferenceForm(
                options.CreatePage,
                entity,
                cancellationToken,
                choices: choices
            )
        );
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(string id, CancellationToken cancellationToken)
    {
        var entity = await Find(id, cancellationToken);
        if (entity is null)
        {
            return NotFound();
        }

        db.Set<TEntity>().Remove(entity);

        if (!await Save(cancellationToken))
        {
            TempData[TempDataKeys.ErrorMessage] =
                "The record could not be deleted. It may have changed or be in use.";
        }

        return RedirectBackOrToIndex();
    }

    public async Task<IActionResult> Edit(string id, CancellationToken cancellationToken)
    {
        var entity = await Find(id, cancellationToken, options.EditPage);

        return entity is null
            ? NotFound()
            : Page(
                "Edit",
                await BuildReferenceForm(
                    options.EditPage,
                    entity,
                    cancellationToken,
                    BuildFormDeleteDialogViewModel(options.Delete, entity, id)
                )
            );
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [ActionName("Edit")]
    public async Task<IActionResult> EditPost(string id, CancellationToken cancellationToken)
    {
        var entity = await Find(id, cancellationToken, options.EditPage);
        if (entity is null)
        {
            return NotFound();
        }

        await BindFormFieldsAsync(entity, options.EditPage);

        var choices = await LoadChoices(options.EditPage, cancellationToken);
        ValidateSelections(entity, options.EditPage, choices);

        if (ModelState.IsValid && await Save(cancellationToken))
        {
            return RedirectToAction(nameof(Index));
        }

        return Page(
            "Edit",
            await BuildReferenceForm(
                options.EditPage,
                entity,
                cancellationToken,
                BuildFormDeleteDialogViewModel(options.Delete, entity, id),
                choices
            )
        );
    }

    public async Task<IActionResult> Index(
        [FromQuery] IndexPageRequest request,
        CancellationToken cancellationToken
    )
    {
        var key = ValidateResource();

        var parameter = Expression.Parameter(typeof(TEntity), "entity");
        var member = Expression.Property(parameter, key.PropertyInfo!);
        var selector = Expression.Lambda(member, parameter);

        var query = db.Set<TEntity>()
            .AsNoTracking()
            .OrderByField(selector, StellarAdmin.TagHelpers.DataGridSortDirection.Ascending);

        foreach (
            var reference in options.References.Where(reference =>
                options.IndexPage.Columns.Any(column => column.FieldName == reference.FieldName)
            )
        )
        {
            query = reference.Include(query);
        }

        var result = await IndexPageQuery.ExecuteAsync(
            options.IndexPage,
            query,
            request,
            q => q.CountAsync(cancellationToken),
            q => q.ToListAsync(cancellationToken)
        );

        return Page(
            "Index",
            new ResourceIndexPageViewModel<TEntity>(
                result,
                options.IndexPage,
                options.Delete,
                entity =>
                    Convert.ToString(
                        key.PropertyInfo!.GetValue(entity),
                        CultureInfo.InvariantCulture
                    )!
            )
        );
    }

    private async Task<ResourceFormPageViewModel> BuildReferenceForm(
        FormPageOptions<TEntity> page,
        TEntity entity,
        CancellationToken cancellationToken,
        ResourceFormDeleteDialogViewModel? delete = null,
        Dictionary<string, IReadOnlyList<SelectListItem>>? choices = null
    )
    {
        choices ??= await LoadChoices(page, cancellationToken);

        foreach (
            var reference in options.References.Where(reference =>
                choices.ContainsKey(reference.FieldName)
            )
        )
        {
            var value = reference.CurrentValue(entity);
            var items = choices[reference.FieldName];
            if (
                !string.IsNullOrEmpty(value)
                && reference.CurrentLabel(db.Model, entity) is { } label
            )
            {
                var current = items.FirstOrDefault(item => item.Value == value);
                if (current is not null)
                {
                    current.Text = label;
                }
                else
                {
                    choices[reference.FieldName] =
                    [
                        .. items,
                        new SelectListItem(label, value) { Disabled = true },
                    ];
                }
            }
        }

        var model = BuildFormPageViewModel(page, entity, delete);
        model.ReferenceChoices = choices;

        return model;
    }

    private async Task<TEntity?> Find(
        string id,
        CancellationToken cancellationToken,
        FormPageOptions<TEntity>? page = null
    )
    {
        var key = ValidateResource();

        object? value;
        try
        {
            value = TypeDescriptor.GetConverter(key.ClrType).ConvertFromInvariantString(id);
        }
        catch (Exception ex)
            when (ex
                    is ArgumentException
                        or FormatException
                        or NotSupportedException
                        or OverflowException
            )
        {
            return null;
        }

        if (value is null)
        {
            return null;
        }

        IQueryable<TEntity> query = db.Set<TEntity>();
        if (page is not null)
        {
            foreach (
                var reference in options.References.Where(reference =>
                    page.Fields.Any(field => field.FieldName == reference.FieldName)
                )
            )
            {
                query = reference.Include(query);
            }
        }

        var parameter = Expression.Parameter(typeof(TEntity), "entity");
        var predicate = Expression.Lambda<Func<TEntity, bool>>(
            Expression.Equal(
                Expression.Property(parameter, key.PropertyInfo!),
                Expression.Constant(value, key.ClrType)
            ),
            parameter
        );

        return await query.SingleOrDefaultAsync(predicate, cancellationToken);
    }

    private async Task<Dictionary<string, IReadOnlyList<SelectListItem>>> LoadChoices(
        FormPageOptions<TEntity> page,
        CancellationToken cancellationToken
    )
    {
        var choices = new Dictionary<string, IReadOnlyList<SelectListItem>>(StringComparer.Ordinal);
        foreach (
            var reference in options.References.Where(reference =>
                page.Fields.Any(field => field.FieldName == reference.FieldName)
            )
        )
        {
            choices.Add(reference.FieldName, await reference.LoadChoices(db, cancellationToken));
        }

        return choices;
    }

    private ViewResult Page(string action, object model)
    {
        var name = views.FindView(ControllerContext, action, true).Success
            ? action
            : "EfCoreResource" + action;

        return View(name, model);
    }

    private async Task<bool> Save(CancellationToken cancellationToken)
    {
        try
        {
            await db.SaveChangesAsync(cancellationToken);

            return true;
        }
        catch (DbUpdateException ex)
        {
            logger.LogWarning(ex, "Could not save resource {EntityType}", typeof(TEntity).Name);

            ModelState.AddModelError(
                string.Empty,
                "The record could not be saved. It may have changed or conflict with existing data."
            );

            return false;
        }
    }

    private IProperty ValidateResource()
    {
        var entity =
            db.Model.FindEntityType(typeof(TEntity))
            ?? throw new InvalidOperationException(
                $"{typeof(TEntity).Name} is not mapped in {typeof(TContext).Name}."
            );

        var keys = entity.FindPrimaryKey()?.Properties;
        if (
            keys is not { Count: 1 }
            || keys[0].PropertyInfo is null
            || !(
                keys[0].ClrType == typeof(int)
                || keys[0].ClrType == typeof(long)
                || keys[0].ClrType == typeof(Guid)
                || keys[0].ClrType == typeof(string)
            )
        )
        {
            throw new InvalidOperationException(
                "EF resources require one CLR primary-key property of type int, long, Guid, or string."
            );
        }

        foreach (
            var field in options
                .CreatePage.Fields.Concat(options.EditPage.Fields)
                .Where(f => !f.IsReadOnly)
        )
        {
            var property = entity.FindProperty(field.FieldName);
            if (
                property is null
                || property.IsPrimaryKey()
                || property.ValueGenerated != ValueGenerated.Never
                || property.IsConcurrencyToken
            )
            {
                throw new InvalidOperationException(
                    $"Writable field '{field.FieldName}' must be a mapped scalar property that is not a key, generated value, or concurrency token."
                );
            }
        }

        foreach (var reference in options.References)
        {
            reference.Validate(db.Model);
        }

        return keys[0];
    }

    private void ValidateSelections(
        TEntity entity,
        FormPageOptions<TEntity> page,
        IReadOnlyDictionary<string, IReadOnlyList<SelectListItem>> choices
    )
    {
        foreach (
            var reference in options.References.Where(reference =>
                page.Fields.Any(field =>
                    field.FieldName == reference.FieldName && !field.IsReadOnly
                )
            )
        )
        {
            var value = reference.CurrentValue(entity);
            var valid = string.IsNullOrEmpty(value)
                ? !reference.IsRequired(db.Model)
                : choices[reference.FieldName].Any(item => item.Value == value && !item.Disabled);
            if (!valid)
            {
                ModelState.AddModelError(
                    $"{ResourceFormPageViewModel.BindingPrefix}.{reference.FieldName}",
                    "Select a valid option."
                );
            }
        }
    }
}
