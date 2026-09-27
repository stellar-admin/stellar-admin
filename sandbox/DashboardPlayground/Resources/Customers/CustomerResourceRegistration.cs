using StellarAdmin.Dashboard;

namespace DashboardPlayground.Resources.Customers;

internal static class CustomerResourceRegistration
{
    internal static void AddCustomerResource(this StellarAdminDashboardBuilder dashboard)
    {
        dashboard.AddResource<Customer>(resource =>
        {
            resource.SidebarItem(item =>
            {
                item.Group = "Commerce";
                item.Order = 20;
            });
            resource.UseDataSource<CustomerDataSource>();
            resource.AllowDelete();
            resource.UseKey(customer => customer.Id);
            resource.Index(index =>
                index.Columns(columns =>
                {
                    columns.Add(customer => customer.Name);
                    columns.Add(customer => customer.Email);
                })
            );
            resource.AllowCreate<CreateCustomerModel, CreateCustomerHandler>(create =>
                create.Fields(fields =>
                {
                    fields.AddSection(
                        "Customer details",
                        section =>
                            section.AddRow(row =>
                            {
                                row.Add(model => model.Name);
                                row.Add(model => model.Email);
                            })
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
            resource.AllowEdit<EditCustomerModel, EditCustomerHandler>(edit =>
                edit.Fields(fields =>
                {
                    fields.Add(model => model.DisplayName);
                    fields.Add(model => model.Email);
                })
            );
        });
    }
}
