using Microsoft.AspNetCore.Mvc;
using StellarAdmin.Pro.Areas.StellarAdmin.ViewModels;
using StellarAdmin.Pro.Resources.Options;

namespace StellarAdmin.Pro.Resources.Controllers;

/// <summary>
///     The base class of the controllers that serve a resource's screens.
/// </summary>
/// <typeparam name="TEntity">The entity type.</typeparam>
public abstract class ResourceControllerBase<TEntity> : Controller
    where TEntity : class
{
    /// <summary>
    ///     Returns the view model of the delete confirmation for the entity.
    /// </summary>
    protected static ResourceFormDeleteDialogViewModel BuildFormDeleteDialogViewModel(
        DeleteOptions<TEntity> options,
        TEntity entity,
        string id
    )
    {
        return new ResourceFormDeleteDialogViewModel(
            options.Title,
            options.FormatMessage(entity),
            options.ConfirmLabel,
            options.CancelLabel,
            id
        );
    }

    /// <summary>
    ///     Returns the view model of a form page rendering the entity.
    /// </summary>
    protected static ResourceFormPageViewModel BuildFormPageViewModel(
        FormPageOptions<TEntity> page,
        TEntity entity,
        ResourceFormDeleteDialogViewModel? delete = null
    )
    {
        return BuildFormPageViewModel<ResourceFormPageViewModel>(page, entity, delete);
    }

    /// <summary>
    ///     Returns the view model of a form page rendering the entity, as a derived view
    ///     model type carrying the page's extra inputs.
    /// </summary>
    protected static TViewModel BuildFormPageViewModel<TViewModel>(
        FormPageOptions<TEntity> page,
        TEntity entity,
        ResourceFormDeleteDialogViewModel? delete = null
    )
        where TViewModel : ResourceFormPageViewModel, new()
    {
        return new TViewModel
        {
            Fields = page.Fields,
            Items = page.Items,
            SectionLayout = page.SectionLayout,
            Title = page.EffectiveTitle,
            Subtitle = page.Subtitle,
            SubmitLabel = page.EffectiveSubmitLabel,
            Entity = entity,
            Delete = delete,
        };
    }

    /// <summary>
    ///     Binds the posted form values onto the entity. Only the page's writable
    ///     configured fields bind, so posting any other property is ignored.
    /// </summary>
    protected Task<bool> BindFormFieldsAsync(TEntity entity, FormPageOptions<TEntity> page)
    {
        var writable = page
            .Fields.Where(field => !field.IsReadOnly)
            .Select(field => field.FieldName)
            .ToHashSet();

        return TryUpdateModelAsync(
            entity,
            prefix: ResourceFormPageViewModel.BindingPrefix,
            propertyFilter: property => property.PropertyName is { } name && writable.Contains(name)
        );
    }

    /// <summary>
    ///     Redirects back to the page the request came from, or to the index page when
    ///     the referrer is absent or not local. Keeps the caller's query string - scope,
    ///     page, sort and search - intact.
    /// </summary>
    protected IActionResult RedirectBack()
    {
        if (
            Uri.TryCreate(Request.Headers.Referer.ToString(), UriKind.Absolute, out var referer)
            && string.Equals(referer.Host, Request.Host.Host, StringComparison.OrdinalIgnoreCase)
        )
        {
            return Redirect(referer.PathAndQuery);
        }

        return RedirectToAction("Index");
    }

    /// <summary>
    ///     Redirects an htmx request back to the page it came from - the response swaps
    ///     into that page in place - and a regular request to the index page.
    /// </summary>
    protected IActionResult RedirectBackOrToIndex()
    {
        return Request.Headers.ContainsKey("HX-Request")
            ? RedirectBack()
            : RedirectToAction("Index");
    }
}
