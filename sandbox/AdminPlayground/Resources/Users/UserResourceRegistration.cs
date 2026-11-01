using AdminPlayground.Data;
using StellarAdmin;
using StellarAdmin.EntityFrameworkCore;
using StellarAdmin.Resources.Editors;

namespace AdminPlayground.Resources.Users;

internal static class UserResourceRegistration
{
    internal static void AddUserResource(this StellarAdminBuilder admin)
    {
        // Users are an EF Core resource of ApplicationDbContext. Their forms wrap the user with the password and roles
        // that UserManager saves, through events
        admin.AddEfCoreResource<ApplicationUser>(
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
                    index.EnableSearch(
                        term =>
                            user =>
                                (user.Email != null && user.Email.Contains(term))
                                || user.FirstName.Contains(term)
                                || user.LastName.Contains(term),
                        search => search.Placeholder = "Search users..."
                    );
                    index.EnablePaging();
                });
                resource.AllowCreate<CreateUserModel>(
                    model => model.User,
                    create =>
                    {
                        create.UseEvents<CreateUserEvents>();
                        create.Fields(fields =>
                        {
                            fields.AddSection(
                                "Account",
                                section =>
                                {
                                    section.Add(model => model.User.Email);
                                    section.Add(
                                        model => model.User.EmailConfirmed,
                                        field => field.Title = "Email confirmed"
                                    );
                                }
                            );
                            fields.AddSection(
                                "Profile",
                                section =>
                                {
                                    section.AddGroup(group =>
                                    {
                                        group.Columns(2);
                                        group.Add(
                                            model => model.User.FirstName,
                                            field => field.Title = "First name"
                                        );
                                        group.Add(
                                            model => model.User.LastName,
                                            field => field.Title = "Last name"
                                        );
                                    });
                                    section.Add(
                                        model => model.User.DepartmentId,
                                        field =>
                                        {
                                            field.Title = "Department";
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
                                        group.Add(
                                            model => model.User.PreferredLanguage,
                                            field =>
                                            {
                                                field.Title = "Preferred language";
                                                field.UseSelectEditor(options =>
                                                {
                                                    options.UseItems(UserLookups.Languages());
                                                    options.EmptyChoiceText = "Not specified";
                                                });
                                            }
                                        );
                                        group.Add(
                                            model => model.User.TimeZoneId,
                                            field =>
                                            {
                                                field.Title = "Time zone";
                                                field.UseSelectEditor(options =>
                                                {
                                                    options.UseItems(UserLookups.TimeZones());
                                                    options.EmptyChoiceText = "Not specified";
                                                });
                                            }
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
                        });
                    }
                );
                resource.AllowEdit<EditUserModel>(
                    model => model.User,
                    edit =>
                    {
                        edit.UseEvents<EditUserEvents>();
                        edit.Fields(fields =>
                        {
                            fields.AddSection(
                                "Account",
                                section =>
                                {
                                    section.Add(model => model.User.Email);
                                    section.Add(
                                        model => model.User.EmailConfirmed,
                                        field => field.Title = "Email confirmed"
                                    );
                                }
                            );
                            fields.AddSection(
                                "Profile",
                                section =>
                                {
                                    section.AddGroup(group =>
                                    {
                                        group.Columns(2);
                                        group.Add(
                                            model => model.User.FirstName,
                                            field => field.Title = "First name"
                                        );
                                        group.Add(
                                            model => model.User.LastName,
                                            field => field.Title = "Last name"
                                        );
                                    });
                                    section.Add(
                                        model => model.User.DepartmentId,
                                        field =>
                                        {
                                            field.Title = "Department";
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
                                        group.Add(
                                            model => model.User.PreferredLanguage,
                                            field =>
                                            {
                                                field.Title = "Preferred language";
                                                field.UseSelectEditor(options =>
                                                {
                                                    options.UseItems(UserLookups.Languages());
                                                    options.EmptyChoiceText = "Not specified";
                                                });
                                            }
                                        );
                                        group.Add(
                                            model => model.User.TimeZoneId,
                                            field =>
                                            {
                                                field.Title = "Time zone";
                                                field.UseSelectEditor(options =>
                                                {
                                                    options.UseItems(UserLookups.TimeZones());
                                                    options.EmptyChoiceText = "Not specified";
                                                });
                                            }
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
                                            field.UseCheckboxGroupEditor(options =>
                                            {
                                                options.UseResource<ApplicationRole>();
                                                options.Columns(2);
                                            });
                                        }
                                    );
                                }
                            );
                        });
                    }
                );
                resource.AllowDelete(delete =>
                    delete.Message = "Delete this user account? This cannot be undone."
                );
            }
        );
    }
}
