using StellarAdmin;
using StellarAdmin.UI;

namespace AdminPlayground.Resources.Customers;

internal static class CustomerResourceRegistration
{
    internal static void AddCustomerResource(this StellarAdminBuilder admin)
    {
        admin.AddResource<Customer>(resource =>
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
            {
                index.Columns(columns =>
                {
                    columns.Add(customer => customer.Name);
                    columns.Add(customer => customer.Email);
                });
                // Renders with the index, from the customer already loaded for the row
                index.EnableRowDetails(
                    "_CustomerSummary",
                    details =>
                    {
                        details.RowClick = true;
                        details.Emphasis = TableRowDetailEmphasis.Rail;
                    }
                );
            });
            resource.AllowCreate<CreateCustomerModel, CreateCustomerHandler>(create =>
                create.Fields(fields =>
                {
                    fields.AddSection(
                        "Customer details",
                        section =>
                            section.AddGroup(group =>
                            {
                                group.Columns(2);
                                group.Add(model => model.Name);
                                group.Add(model => model.Email);
                            })
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
