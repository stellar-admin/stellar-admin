using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using StellarAdmin.Areas.StellarAdmin.ViewModels;
using StellarAdmin.Resources;

namespace StellarAdmin.Areas.StellarAdmin.Controllers;

internal sealed partial class ResourceController<TResource>
{
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(
        [FromRoute] string id,
        [FromQuery] ResourceIndexQuery query,
        [FromForm] string? origin,
        CancellationToken cancellationToken
    )
    {
        if (!ModelState.IsValid)
        {
            return BadRequest();
        }

        if (
            _resourceOptions.Delete is null
            || _resourceOptions.KeySelector is null
            || string.IsNullOrEmpty(id)
            || dataSource is not IResourceDeleteHandler<TResource> handler
        )
        {
            return NotFound();
        }

        if (!TryCreateListRequest(query, out var request))
        {
            return BadRequest();
        }

        var result = await RunHandlerAsync(
            () => handler.DeleteAsync(id, cancellationToken),
            "delete",
            "This record couldn't be deleted. Try again.",
            cancellationToken
        );
        if (result.IsNotFound)
        {
            return NotFound();
        }

        if (!result.IsSuccess)
        {
            if (origin is "edit" or "view")
            {
                TempData[TempDataKeys.DeleteErrors] = JsonSerializer.Serialize(
                    result.Errors.Select(error => error.Message)
                );
                return RedirectToAction(
                    origin == "edit" ? nameof(Edit) : ViewActionName,
                    new { id }
                );
            }

            AddValidationErrors(result, [], [], ResourceFormPageViewModel.BindingPrefix);
            return await IndexView(query, request, cancellationToken, redirectOutOfRange: false);
        }

        return RedirectToAction(
            nameof(Index),
            new
            {
                id = (string?)null,
                page = query.Page,
                pageSize = query.PageSize,
                search = query.Search,
                scope = query.Scope,
                sortBy = query.SortBy,
                sortDirection = query.SortDirection,
            }
        );
    }

    // A rejected delete from the edit or view page returns there with its errors
    private void AddDeleteErrors()
    {
        if (TempData[TempDataKeys.DeleteErrors] is string deleteErrors)
        {
            foreach (var message in JsonSerializer.Deserialize<string[]>(deleteErrors) ?? [])
            {
                ModelState.AddModelError(string.Empty, message);
            }
        }
    }

    private ResourceFormDeleteDialogViewModel? CreateDeleteDialog(
        string id,
        string origin,
        ResourceLabelContext labels
    ) =>
        _resourceOptions.Delete is { } delete
            ? new(
                delete.Title ?? _labelOptions.Delete.Title(labels),
                delete.Message ?? _labelOptions.Delete.Message(labels),
                delete.ConfirmLabel ?? _labelOptions.Delete.ConfirmLabel(labels),
                delete.CancelLabel ?? _labelOptions.Delete.CancelLabel(labels),
                id,
                origin
            )
            : null;
}
