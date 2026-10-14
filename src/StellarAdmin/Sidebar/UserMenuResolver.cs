using System.Security.Claims;

namespace StellarAdmin.Sidebar;

internal sealed class UserMenuResolver(StellarAdminOptions options)
{
    private const string DefaultAvatarClaimType = "picture";

    public SidebarUserMenuModel? CreateUserMenu(ClaimsPrincipal user)
    {
        if (user.Identity?.IsAuthenticated != true)
        {
            return null;
        }

        var menu = options.UserMenu;
        var name = menu.NameClaimType is { } nameClaimType
            ? Claim(user, nameClaimType)
            : Nonblank(user.Identity.Name) ?? Claim(user, ClaimTypes.Name, "name");
        var description = menu.DescriptionClaimType is { } descriptionClaimType
            ? Claim(user, descriptionClaimType)
            : Claim(user, ClaimTypes.Email, "email");
        var avatarUrl = Claim(user, menu.AvatarClaimType ?? DefaultAvatarClaimType);

        // A user without a name claim is shown by their description, or as a generic user
        return new(
            name ?? description ?? "User",
            name is null ? null : description,
            avatarUrl,
            [.. menu.Items.Where(item => item is not null)]
        );
    }

    private static string? Claim(ClaimsPrincipal user, params string[] claimTypes) =>
        claimTypes
            .Select(claimType => Nonblank(user.FindFirst(claimType)?.Value))
            .FirstOrDefault(value => value is not null);

    private static string? Nonblank(string? value) =>
        string.IsNullOrWhiteSpace(value) ? null : value;
}
