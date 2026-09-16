using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using StellarAdmin.Pro.Identity.Options;
using StellarAdmin.Pro.Areas.StellarAdmin;
using StellarAdmin.Pro.Areas.StellarAdmin.ViewModels;
using StellarAdmin.Pro.Resources.Controllers;
using StellarAdmin.Pro.Resources.Infrastructure.Query;

namespace StellarAdmin.Pro.Identity.Areas.StellarAdmin.Controllers;

[Area("StellarAdmin")]
public class RolesController<TRole, TKey>(
    RoleManager<TRole> roleManager,
    IdentityRolesOptions<TRole, TKey> options
) : ResourceControllerBase<TRole>
    where TRole : IdentityRole<TKey>
    where TKey : IEquatable<TKey>
{
    public IActionResult Create()
    {
        return View(BuildFormPageViewModel(options.CreatePage, options.CreatePage.CreateInstance()));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [ActionName("Create")]
    public async Task<IActionResult> CreatePost()
    {
        var role = options.CreatePage.CreateInstance();
        await BindFormFieldsAsync(role, options.CreatePage);

        if (!ModelState.IsValid)
        {
            return View(BuildFormPageViewModel(options.CreatePage, role));
        }

        var result = await roleManager.CreateAsync(role);
        if (!result.Succeeded)
        {
            AddIdentityErrors(result);
            return View(BuildFormPageViewModel(options.CreatePage, role));
        }

        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(string id)
    {
        // A role that is already gone is not an error: the refreshed grid shows the
        // outcome the caller asked for.
        var role = await roleManager.FindByIdAsync(id);
        if (role is null)
        {
            return RedirectBackOrToIndex();
        }

        var result = await roleManager.DeleteAsync(role);
        if (!result.Succeeded)
        {
            TempData[TempDataKeys.ErrorMessage] = string.Join(
                " ",
                result.Errors.Select(error => error.Description)
            );
            return RedirectBackOrToIndex();
        }

        return RedirectBackOrToIndex();
    }

    public async Task<IActionResult> Edit(string id)
    {
        var role = await roleManager.FindByIdAsync(id);
        if (role is null)
        {
            return NotFound();
        }

        return View(
            BuildFormPageViewModel(
                options.EditPage,
                role,
                BuildFormDeleteDialogViewModel(options.Delete, role, id)
            )
        );
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [ActionName("Edit")]
    public async Task<IActionResult> EditPost(string id)
    {
        var role = await roleManager.FindByIdAsync(id);
        if (role is null)
        {
            return NotFound();
        }

        var delete = BuildFormDeleteDialogViewModel(options.Delete, role, id);
        await BindFormFieldsAsync(role, options.EditPage);

        if (!ModelState.IsValid)
        {
            return View(BuildFormPageViewModel(options.EditPage, role, delete));
        }

        var result = await roleManager.UpdateAsync(role);
        if (!result.Succeeded)
        {
            AddIdentityErrors(result);
            return View(BuildFormPageViewModel(options.EditPage, role, delete));
        }

        return RedirectToAction(nameof(Index));
    }

    public IActionResult Index([FromQuery] IndexPageRequest request)
    {
        var result = IndexPageQuery.Execute(options.IndexPage, roleManager.Roles, request);

        return View(
            new ResourceIndexPageViewModel<TRole>(
                result,
                options.IndexPage,
                options.Delete,
                role => role.Id.ToString()!
            )
        );
    }

    /// <summary>
    ///     Adds the errors of a failed <see cref="IdentityResult" /> to model state.
    ///     Errors with a known code attach to the form field they concern so the
    ///     message renders at the input; the rest go to the validation summary.
    /// </summary>
    private void AddIdentityErrors(IdentityResult result)
    {
        foreach (var error in result.Errors)
        {
            var key = error.Code switch
            {
                nameof(IdentityErrorDescriber.DuplicateRoleName)
                or nameof(IdentityErrorDescriber.InvalidRoleName) =>
                    $"{ResourceFormPageViewModel.BindingPrefix}.{nameof(IdentityRole.Name)}",
                _ => string.Empty,
            };
            ModelState.AddModelError(key, error.Description);
        }
    }
}
