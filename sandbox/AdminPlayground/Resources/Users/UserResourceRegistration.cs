using AdminPlayground.Data;
using StellarAdmin;
using StellarAdmin.EntityFrameworkCore;
using StellarAdmin.Resources.Editors;

namespace AdminPlayground.Resources.Users;

internal static class UserResourceRegistration
{
    internal static void AddUserResource(this StellarAdminBuilder admin)
    {
        admin.AddResource<ApplicationUser>(
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
                    item.Icon = "users";
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
                                section.AddGroup(group =>
                                {
                                    group.Columns(2);
                                    group.Add(model => model.FirstName);
                                    group.Add(model => model.LastName);
                                });
                                section.Add(
                                    model => model.DepartmentId,
                                    field =>
                                    {
                                        field.UseSelectEditor(options =>
                                        {
                                            options.UseResource<Department>();
                                            options.EmptyChoiceText = "Not specified";
                                        });
                                    }
                                );
                                section.AddGroup(group =>
                                {
                                    group.Columns(2);
                                    group
                                        .Add(model => model.PreferredLanguage)
                                        .UseSelectEditor(options =>
                                        {
                                            options.UseItems(UserLookups.Languages());
                                            options.EmptyChoiceText = "Not specified";
                                        });
                                    group
                                        .Add(model => model.TimeZoneId)
                                        .UseSelectEditor(options =>
                                        {
                                            options.UseItems(UserLookups.TimeZones());
                                            options.EmptyChoiceText = "Not specified";
                                        });
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
                                        field.UseCheckboxGroupEditor(options =>
                                        {
                                            options.UseResource<ApplicationRole>();
                                            options.Columns(2);
                                        });
                                    }
                                );
                            }
                        );
                        fields.AddSection(
                            "Password",
                            section =>
                                section.AddGroup(group =>
                                {
                                    group.Columns(2);
                                    group.Add(model => model.Password);
                                    group.Add(model => model.PasswordConfirmation);
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
                                section.AddGroup(group =>
                                {
                                    group.Columns(2);
                                    group.Add(model => model.FirstName);
                                    group.Add(model => model.LastName);
                                });
                                section.Add(
                                    model => model.DepartmentId,
                                    field =>
                                    {
                                        field.UseSelectEditor(options =>
                                        {
                                            options.UseResource<Department>();
                                            options.EmptyChoiceText = "Not specified";
                                        });
                                    }
                                );
                                section.AddGroup(group =>
                                {
                                    group.Columns(2);
                                    group
                                        .Add(model => model.PreferredLanguage)
                                        .UseSelectEditor(options =>
                                        {
                                            options.UseItems(UserLookups.Languages());
                                            options.EmptyChoiceText = "Not specified";
                                        });
                                    group
                                        .Add(model => model.TimeZoneId)
                                        .UseSelectEditor(options =>
                                        {
                                            options.UseItems(UserLookups.TimeZones());
                                            options.EmptyChoiceText = "Not specified";
                                        });
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
                                        field.UseCheckboxGroupEditor(options =>
                                        {
                                            options.UseResource<ApplicationRole>();
                                            options.Columns(2);
                                        });
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
