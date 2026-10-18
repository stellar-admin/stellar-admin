using System.Reflection;
using StellarAdmin.Areas.StellarAdmin.ViewModels.Internal;

namespace StellarAdmin.Home;

// The setup checklist's steps: what each one says, its code and the instructions for a coding
// agent, worded for what the app has already done
internal static class SetupSteps
{
    private const string ReferencesUrl =
        "https://github.com/stellar-admin/stellar-admin/blob/master/skills/stellar-admin/references/";

    // The code and instructions that turn the checklist off, in its footer
    public static SetupTabsViewModel TurnOff { get; } =
        new(
            "off",
            null,
            """
            builder.Services.AddStellarAdmin(admin =>
            {
                admin.ConfigureHomePage(home => home.ShowSetupChecklist = false);
            });
            """,
            "Turn off the StellarAdmin setup checklist on the panel's home page."
        );

    public static SetupChecklistViewModel Create(
        SetupStatus status,
        string title,
        string panelPath,
        string? themeStylesheet
    )
    {
        var steps = new List<SetupStepViewModel>
        {
            Resource(status),
            Protect(status, panelPath),
            Branding(status, title),
        };
        if (status.HasAuthentication)
        {
            steps.Add(UserMenu(status));
        }

        steps.Add(Theme(themeStylesheet));
        if (status.ResourceCount > 0)
        {
            steps.Add(Sidebar(status));
        }

        return new(steps);
    }

    private static SetupStepViewModel Branding(SetupStatus status, string title)
    {
        return new(
            "branding",
            status.HasBranding ? $"Named “{title}”" : "Set the name and logo",
            status.HasBranding
                ? "The sidebar, header and browser title show your name and logo."
                : $"The sidebar shows “{title}”, made from the application name. Set your app's name and logo.",
            $$"""
            builder.Services.AddStellarAdmin(admin =>
            {
                admin.ConfigureBranding(branding =>
                {
                    branding.Title = "{{title}}";
                    branding.Logo = BrandingLogo.Image("~/img/logo.svg");
                });
            });
            """,
            "Set the StellarAdmin panel's name and logo. Ask me for the name, and use our app's logo if it has one.",
            new("Branding docs", ReferencesUrl + "sidebar.md#branding")
        )
        {
            Done = status.HasBranding,
        };
    }

    // Names an entity property for the example code: the first writable string, so the column
    // reads as data rather than a key
    private static string ExampleProperty(Type entity)
    {
        var properties = entity
            .GetProperties(BindingFlags.Public | BindingFlags.Instance)
            .Where(property => property.CanRead && property.CanWrite)
            .ToArray();

        return (
                properties.FirstOrDefault(property => property.PropertyType == typeof(string))
                ?? properties.FirstOrDefault()
            )?.Name
            ?? "Id";
    }

    private static SetupStepViewModel Protect(SetupStatus status, string panelPath)
    {
        return new(
            "protect",
            status.IsProtected ? "Panel is protected" : "Protect the panel",
            status.IsProtected ? "Only users who meet its authorization requirements can use it."
                : status.HasAuthentication
                    ? $"Anyone who can reach {panelPath} can use it. Require sign-in, a role or a policy."
                : $"Anyone who can reach {panelPath} can use it, and the app has no authentication yet. Add authentication before you deploy, then require it here.",
            """
            builder.Services.AddStellarAdmin(admin =>
            {
                admin.RequireAuthorization(policy => policy.RequireRole("Staff"));
            });
            """,
            status.HasAuthentication
                ? "Protect the StellarAdmin panel so only signed-in users with the Staff role can use it. Anonymous users should be sent to sign in."
                : "This app has no authentication. Add cookie authentication with a sign-in page, then protect the StellarAdmin panel so only signed-in users can use it.",
            new("Authorization docs", ReferencesUrl + "authorization.md")
        )
        {
            Done = status.IsProtected,
            Essential = true,
            Warn = !status.IsProtected,
        };
    }

    private static SetupStepViewModel Resource(SetupStatus status)
    {
        var entities = status.Entities;
        var candidate = entities.Contexts.FirstOrDefault();
        var entity = candidate?.EntityTypes[0];
        var entityName = entity?.Name ?? "Booking";
        var contextName = candidate?.ContextType.Name ?? "AppDbContext";
        var variable = char.ToLowerInvariant(entityName[0]) + entityName[1..];
        var property = entity is null ? "Reference" : ExampleProperty(entity);

        var detail =
            status.ResourceCount > 0 ? null
            : candidate is null
                ? "No EF Core DbContext found. Any class works: a data source implementing IResourceDataSource<T> loads and saves it."
            : string.Join(
                " ",
                entities.Contexts.Select(context =>
                    $"Found in {context.ContextType.Name}: {string.Join(", ", context.EntityTypes.Select(type => type.Name))}."
                )
            )
                + (
                    entities.HasEfCorePackage
                        ? ""
                        : " EF Core resources need the StellarAdmin.EntityFrameworkCore package."
                );

        var code =
            candidate is not null || entities.HasEfCorePackage
                ? $$"""
                    using StellarAdmin.EntityFrameworkCore;

                    builder.Services.AddStellarAdmin(admin =>
                    {
                        admin.AddEfCoreResource<{{contextName}}, {{entityName}}>(resource =>
                        {
                            resource.Index(index => index.Columns(columns =>
                            {
                                columns.Add({{variable}} => {{variable}}.{{property}});
                            }));
                            resource.AllowCreate(create => create.Fields(fields =>
                            {
                                fields.Add({{variable}} => {{variable}}.{{property}});
                            }));
                            resource.AllowEdit(edit => edit.Fields(fields =>
                            {
                                fields.Add({{variable}} => {{variable}}.{{property}});
                            }));
                            resource.AllowDelete();
                        });
                    });
                    """
                : """
                    builder.Services.AddStellarAdmin(admin =>
                    {
                        admin.AddResource<Booking>(resource =>
                        {
                            resource.UseDataSource<BookingDataSource>();
                            resource.UseKey(booking => booking.Id);
                            resource.Index(index => index.Columns(columns =>
                            {
                                columns.Add(booking => booking.Reference);
                            }));
                        });
                    });
                    """;

        return new(
            "resource",
            status.ResourceCount switch
            {
                0 => "Add your first resource",
                1 => "1 resource registered",
                var count => $"{count} resources registered",
            },
            status.ResourceCount > 0
                ? "Each resource gets list, create, edit and delete pages. Add more the same way."
                : "Register an entity to get list, create, edit and delete pages for it.",
            code,
            candidate is not null
                    ? $"Add {entityName} from {contextName} to the StellarAdmin panel as a resource. Show its main properties in the list, and allow create, edit and delete. Use a select or lookup for any foreign key."
                : entities.HasEfCorePackage
                    ? "Add an entity from our EF Core DbContext to the StellarAdmin panel as a resource; ask me which one. Show its main properties in the list, and allow create, edit and delete. Use a select or lookup for any foreign key."
                : "Add one of our classes to the StellarAdmin panel as a resource, with a data source that loads and saves it; ask me which one. Show its main properties in the list, and allow create, edit and delete.",
            new("Resources docs", ReferencesUrl + "resources.md")
        )
        {
            Detail = detail,
            Done = status.ResourceCount > 0,
            Essential = true,
            Install =
                candidate is not null && !entities.HasEfCorePackage
                    ? "dotnet add package StellarAdmin.EntityFrameworkCore"
                    : null,
        };
    }

    private static SetupStepViewModel Sidebar(SetupStatus status)
    {
        return new(
            "sidebar",
            status.HasSidebarLayout ? "Sidebar organised" : "Organise the sidebar",
            status.HasSidebarLayout
                ? "Resources have groups, icons or an order, or the sidebar links to your own pages."
                : "Group resources, give them icons, and link to your own pages.",
            """
            // In a resource's configuration
            resource.SidebarItem(item =>
            {
                item.Group = "Sales";
                item.Icon = "plane";
            });

            // In AddStellarAdmin
            admin.AddSidebarLink("Reports", SidebarLinkTarget.Page("/Reports"));
            """,
            "Organise the StellarAdmin sidebar: put related resources in groups, give each an icon, and add links to our own admin pages if we have any.",
            new("Sidebar docs", ReferencesUrl + "sidebar.md#groups-and-order")
        )
        {
            Done = status.HasSidebarLayout,
            Optional = true,
        };
    }

    private static SetupStepViewModel Theme(string? themeStylesheet)
    {
        return new(
            "theme",
            themeStylesheet is null ? "Choose a theme" : $"Theme: {themeStylesheet}",
            themeStylesheet is null
                ? "Build a theme in the theme builder, save it to wwwroot and add it as a stylesheet."
                : "Your stylesheet loads after the panel's own.",
            """
            builder.Services.AddStellarAdmin(admin =>
            {
                admin.AddStylesheet("~/css/theme.css");
            });
            """,
            "Create a StellarAdmin theme that matches our brand. Ask me for the brand colours, save it to wwwroot/css/theme.css and add it to the panel.",
            new("Themes docs", "https://www.stellaradmin.com/docs/ui/theming")
        )
        {
            Done = themeStylesheet is not null,
            ExtraLink = new("Open the theme builder", "https://www.stellaradmin.com/theme-builder"),
            Optional = true,
        };
    }

    private static SetupStepViewModel UserMenu(SetupStatus status)
    {
        return new(
            "usermenu",
            status.HasUserMenu ? "User menu customised" : "Add to the user menu",
            status.HasUserMenu
                ? "The sidebar footer menu shows your changes."
                : "The sidebar footer shows the signed-in user with Log out. Link to their account page, or change how log out works.",
            """
            builder.Services.AddStellarAdmin(admin =>
            {
                admin.ConfigureUserMenu(menu =>
                {
                    menu.Add(UserMenuItem.Page("Account", "/Account/Manage/Index", "Identity"));
                });
            });
            """,
            "Add a link to the account management page to the StellarAdmin user menu, above Log out.",
            new("User menu docs", ReferencesUrl + "sidebar.md#user-menu")
        )
        {
            Done = status.HasUserMenu,
            Optional = true,
        };
    }
}
