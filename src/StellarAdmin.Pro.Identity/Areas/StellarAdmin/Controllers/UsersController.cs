using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using StellarAdmin.Pro.Identity.Areas.StellarAdmin.ViewModels;
using StellarAdmin.Pro.Identity.Options;
using StellarAdmin.Pro.Areas.StellarAdmin;
using StellarAdmin.Pro.Areas.StellarAdmin.ViewModels;
using StellarAdmin.Pro.Resources.Controllers;
using StellarAdmin.Pro.Resources.Infrastructure.Query;

namespace StellarAdmin.Pro.Identity.Areas.StellarAdmin.Controllers;

[Area("StellarAdmin")]
public class UsersController<TUser, TKey>(
    UserManager<TUser> userManager,
    IdentityUsersOptions<TUser, TKey> options
) : ResourceControllerBase<TUser>
    where TUser : IdentityUser<TKey>
    where TKey : IEquatable<TKey>
{
    public IActionResult Create()
    {
        return View(
            BuildFormPageViewModel<UserCreateViewModel>(
                options.CreatePage,
                options.CreatePage.CreateInstance()
            )
        );
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(CreateUserPasswordInput passwordInput)
    {
        var user = options.CreatePage.CreateInstance();
        await BindFormFieldsAsync(user, options.CreatePage);

        if (!ModelState.IsValid)
        {
            return View(BuildFormPageViewModel<UserCreateViewModel>(options.CreatePage, user));
        }

        var result = await userManager.CreateAsync(user, passwordInput.Password);
        if (!result.Succeeded)
        {
            AddIdentityErrors(result);
            return View(BuildFormPageViewModel<UserCreateViewModel>(options.CreatePage, user));
        }

        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(string id)
    {
        // A user that is already gone is not an error: the refreshed grid shows the
        // outcome the caller asked for.
        var user = await userManager.FindByIdAsync(id);
        if (user is null)
        {
            return RedirectBackOrToIndex();
        }

        if (string.Equals(userManager.GetUserId(User), id, StringComparison.Ordinal))
        {
            TempData[TempDataKeys.ErrorMessage] =
                "You cannot delete the account you are signed in with.";
            return RedirectBackOrToIndex();
        }

        var result = await userManager.DeleteAsync(user);
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
        var user = await userManager.FindByIdAsync(id);
        if (user is null)
        {
            return NotFound();
        }

        return View(
            BuildFormPageViewModel(
                options.EditPage,
                user,
                BuildFormDeleteDialogViewModel(options.Delete, user, id)
            )
        );
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [ActionName("Edit")]
    public async Task<IActionResult> EditPost(string id)
    {
        var user = await userManager.FindByIdAsync(id);
        if (user is null)
        {
            return NotFound();
        }

        var delete = BuildFormDeleteDialogViewModel(options.Delete, user, id);
        await BindFormFieldsAsync(user, options.EditPage);

        if (!ModelState.IsValid)
        {
            return View(BuildFormPageViewModel(options.EditPage, user, delete));
        }

        var result = await userManager.UpdateAsync(user);
        if (!result.Succeeded)
        {
            AddIdentityErrors(result);
            return View(BuildFormPageViewModel(options.EditPage, user, delete));
        }

        return RedirectToAction(nameof(Index));
    }

    public IActionResult Index([FromQuery] IndexPageRequest request)
    {
        var result = IndexPageQuery.Execute(options.IndexPage, userManager.Users, request);

        return View(
            new ResourceIndexPageViewModel<TUser>(
                result,
                options.IndexPage,
                options.Delete,
                user => user.Id.ToString()!
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
                nameof(IdentityErrorDescriber.DuplicateUserName)
                or nameof(IdentityErrorDescriber.InvalidUserName) =>
                    $"{ResourceFormPageViewModel.BindingPrefix}.{nameof(IdentityUser.UserName)}",
                nameof(IdentityErrorDescriber.DuplicateEmail)
                or nameof(IdentityErrorDescriber.InvalidEmail) =>
                    $"{ResourceFormPageViewModel.BindingPrefix}.{nameof(IdentityUser.Email)}",
                nameof(IdentityErrorDescriber.PasswordTooShort)
                or nameof(IdentityErrorDescriber.PasswordRequiresDigit)
                or nameof(IdentityErrorDescriber.PasswordRequiresLower)
                or nameof(IdentityErrorDescriber.PasswordRequiresUpper)
                or nameof(IdentityErrorDescriber.PasswordRequiresNonAlphanumeric)
                or nameof(IdentityErrorDescriber.PasswordRequiresUniqueChars) =>
                    $"{nameof(UserCreateViewModel.PasswordInput)}.{nameof(CreateUserPasswordInput.Password)}",
                _ => string.Empty,
            };
            ModelState.AddModelError(key, error.Description);
        }
    }
}
