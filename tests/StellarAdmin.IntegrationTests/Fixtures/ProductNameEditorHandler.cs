using Microsoft.AspNetCore.Hosting;
using StellarAdmin.Resources.Editors;

namespace StellarAdmin.IntegrationTests.Fixtures;

public sealed class ProductNameEditorHandler(
    ProductNameEditor editor,
    IWebHostEnvironment environment
) : IFieldEditorHandler<ProductNameEditor>
{
    public string TemplateName => nameof(ProductNameEditor);

    public Task<object?> PrepareAsync(
        FieldEditorContext context,
        CancellationToken cancellationToken
    ) =>
        Task.FromResult<object?>(
            $"{editor.Placeholder}:{environment.EnvironmentName}:{context.FieldName}={context.Value}"
        );
}
