using Microsoft.AspNetCore.Hosting;
using StellarAdmin.Dashboard.Resources.Editors;

namespace StellarAdmin.Dashboard.IntegrationTests.Fixtures;

public sealed class ProductNameEditor(
    ProductNameEditorOptions options,
    IWebHostEnvironment environment
) : IFieldEditor<ProductNameEditorOptions>
{
    public string TemplateName => nameof(ProductNameEditor);

    public Task<object?> PrepareAsync(CancellationToken cancellationToken) =>
        Task.FromResult<object?>($"{options.Placeholder}:{environment.EnvironmentName}");
}
