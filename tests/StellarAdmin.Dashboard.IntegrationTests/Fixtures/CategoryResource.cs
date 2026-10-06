using StellarAdmin.Dashboard.Resources.Builders;

namespace StellarAdmin.Dashboard.IntegrationTests.Fixtures;

internal static class CategoryResource
{
    // The resource a category lookup creates items with; the host registers CategoryStore
    public static StellarAdminDashboardBuilder AddCategoryResource(
        this StellarAdminDashboardBuilder dashboard,
        Action<ResourceBuilder<Category>>? configure = null
    ) =>
        dashboard.AddResource<Category>(resource =>
        {
            resource.UseDataSource<CategoryDataSource>();
            resource.UseKey(category => category.Id);
            resource.Index(index =>
                index.Columns(columns => columns.Add(category => category.Name))
            );
            resource.AllowCreate(create =>
                create.Fields(fields =>
                {
                    fields.Add(category => category.Name);
                    fields.Add(category => category.Code);
                })
            );
            configure?.Invoke(resource);
        });
}
