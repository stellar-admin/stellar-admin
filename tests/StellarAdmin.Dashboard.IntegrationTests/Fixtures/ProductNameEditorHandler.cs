using Microsoft.AspNetCore.Hosting;
using StellarAdmin.Dashboard.Resources.Editors;

namespace StellarAdmin.Dashboard.IntegrationTests.Fixtures;

public sealed class ProductNameEditorHandler(
    ProductNameEditor editor,
    IWebHostEnvironment environment
) : IFieldEditorHandler<ProductNameEditor>
{
    public string TemplateName => nameof(ProductNameEditor);

    public Task<object?> PrepareAsync(CancellationToken cancellationToken) =>
        Task.FromResult<object?>($"{editor.Placeholder}:{environment.EnvironmentName}");
}
