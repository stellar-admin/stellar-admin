using DashboardPlayground.Data;
using DashboardPlayground.Resources.Departments;
using StellarAdmin.Dashboard;
using StellarAdmin.Dashboard.Resources.Options;

namespace DashboardPlayground.Resources.Users;

internal static class UserResourceRegistration
{
    internal static void AddUserResource(this StellarAdminDashboardBuilder dashboard)
    {
        dashboard.AddResource<ApplicationUser>(resource =>
        {
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
                                    field.UseEditor<ReferenceLookupEditorOptions>(options =>
                                        options.UseLookup<DepartmentLookupProvider>()
                                    );
                                }
                            );
                            section.AddRow(row =>
                            {
                                row.Add(model => model.PreferredLanguage);
                                row.Add(model => model.TimeZoneId);
                            });
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
                                    field.UseEditor<ReferenceLookupEditorOptions>(options =>
                                        options.UseLookup<DepartmentLookupProvider>()
                                    );
                                }
                            );
                            section.AddRow(row =>
                            {
                                row.Add(model => model.PreferredLanguage);
                                row.Add(model => model.TimeZoneId);
                            });
                        }
                    );
                })
            );
            resource.AllowDelete(delete =>
                delete.Message = "Delete this user account? This cannot be undone."
            );
        });
    }
}
