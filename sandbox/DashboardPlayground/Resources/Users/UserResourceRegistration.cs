using DashboardPlayground.Data;
using StellarAdmin.Dashboard;
using StellarAdmin.Dashboard.EntityFrameworkCore;
using StellarAdmin.Dashboard.Resources.Options;

namespace DashboardPlayground.Resources.Users;

internal static class UserResourceRegistration
{
    internal static void AddUserResource(this StellarAdminDashboardBuilder dashboard)
    {
        dashboard.AddResource<ApplicationUser>(
            "users",
            resource =>
            {
                resource.RequireAuthorization(policyBuilder =>
                    policyBuilder.RequireRole("Administrator", "User Admin")
                );

                resource.SingularLabel = "user";
                resource.PluralLabel = "users";
                resource.SidebarItem(item =>
                {
                    item.Label = "Users";
                    item.Group = "Identity";
                });
                resource.UseDataSource<UserDataSource>();
                resource.UseKey(user => user.Id);
                resource.Index(index =>
                {
                    index.Columns(columns =>
                    {
                        columns.Add(user => user.FirstName, column => column.Sortable());
                        columns.Add(user => user.LastName, column => column.Sortable());
                        columns.Add(user => user.Email, column => column.Sortable());
                        columns.Add(
                            user => user.Department!.Name,
                            column => column.Title = "Department"
                        );
                        columns.Add(user => user.EmailConfirmed, column => column.Sortable());
                    });
                    index.DefaultSortBy(user => user.Email);
                    index.EnableSearch(search => search.Placeholder = "Search users...");
                    index.EnablePaging();
                });
                resource.AllowCreate<CreateUserModel, CreateUserHandler>(create =>
                    create.Fields(fields =>
                    {
                        fields.AddSection(
                            "Account",
                            section =>
                            {
                                section.Add(model => model.Email);
                                section.Add(model => model.EmailConfirmed);
                            }
                        );
                        fields.AddSection(
                            "Profile",
                            section =>
                            {
                                section.AddRow(row =>
                                {
                                    row.Add(model => model.FirstName);
                                    row.Add(model => model.LastName);
                                });
                                section.Add(
                                    model => model.DepartmentId,
                                    field =>
                                    {
                                        field.UseEditor<SelectListEditorOptions>(options =>
                                            options.UseItems<
                                                ApplicationDbContext,
                                                Department,
                                                Guid
                                            >(
                                                department => department.Id,
                                                department => department.Name,
                                                items =>
                                                {
                                                    items.OrderBy(department => department.Name);
                                                    items.IncludeEmptyOption("Not specified");
                                                }
                                            )
                                        );
                                    }
                                );
                                section.AddRow(row =>
                                {
                                    row.Add(model => model.PreferredLanguage)
                                        .UseEditor<SelectListEditorOptions>(options =>
                                            options.UseItems(UserLookups.Languages())
                                        );
                                    row.Add(model => model.TimeZoneId)
                                        .UseEditor<SelectListEditorOptions>(options =>
                                            options.UseItems(UserLookups.TimeZones())
                                        );
                                });
                            }
                        );
                        fields.AddSection(
                            "Roles",
                            section =>
                            {
                                section.Description = "Select the roles the user belong to";
                                section.Add(
                                    model => model.RoleIds,
                                    field =>
                                    {
                                        field.UseEditor<CheckboxGroupEditorOptions>(options =>
                                            options.UseItems<
                                                ApplicationDbContext,
                                                ApplicationRole,
                                                string
                                            >(
                                                role => role.Id,
                                                role => role.Name!,
                                                items => items.OrderBy(role => role.Name)
                                            )
                                        );
                                    }
                                );
                            }
                        );
                        fields.AddSection(
                            "Password",
                            section =>
                                section.AddRow(row =>
                                {
                                    row.Add(model => model.Password);
                                    row.Add(model => model.PasswordConfirmation);
                                })
                        );
                    })
                );
                resource.AllowEdit<EditUserModel, EditUserHandler>(edit =>
                    edit.Fields(fields =>
                    {
                        fields.AddSection(
                            "Account",
                            section =>
                            {
                                section.Add(model => model.Email);
                                section.Add(model => model.EmailConfirmed);
                            }
                        );
                        fields.AddSection(
                            "Profile",
                            section =>
                            {
                                section.AddRow(row =>
                                {
                                    row.Add(model => model.FirstName);
                                    row.Add(model => model.LastName);
                                });
                                section.Add(
                                    model => model.DepartmentId,
                                    field =>
                                    {
                                        field.UseEditor<SelectListEditorOptions>(options =>
                                            options.UseItems<
                                                ApplicationDbContext,
                                                Department,
                                                Guid
                                            >(
                                                department => department.Id,
                                                department => department.Name,
                                                items =>
                                                {
                                                    items.OrderBy(department => department.Name);
                                                    items.IncludeEmptyOption("Not specified");
                                                }
                                            )
                                        );
                                    }
                                );
                                section.AddRow(row =>
                                {
                                    row.Add(model => model.PreferredLanguage)
                                        .UseEditor<SelectListEditorOptions>(options =>
                                            options.UseItems(UserLookups.Languages())
                                        );
                                    row.Add(model => model.TimeZoneId)
                                        .UseEditor<SelectListEditorOptions>(options =>
                                            options.UseItems(UserLookups.TimeZones())
                                        );
                                });
                            }
                        );
                        fields.AddSection(
                            "Roles",
                            section =>
                            {
                                section.Description = "Select the roles the user belong to";
                                section.Add(
                                    model => model.RoleIds,
                                    field =>
                                    {
                                        field.UseEditor<CheckboxGroupEditorOptions>(options =>
                                            options.UseItems<
                                                ApplicationDbContext,
                                                ApplicationRole,
                                                string
                                            >(
                                                role => role.Id,
                                                role => role.Name!,
                                                items => items.OrderBy(role => role.Name)
                                            )
                                        );
                                    }
                                );
                            }
                        );
                    })
                );
                resource.AllowDelete(delete =>
                    delete.Message = "Delete this user account? This cannot be undone."
                );
            }
        );
    }
}
