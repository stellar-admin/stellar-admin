using StellarAdmin.Resources.Displays;

namespace StellarAdmin.IntegrationTests.Fixtures;

// A display with a handler: the handler finds the category name for the key on each request
public sealed class CategoryNameDisplay : FieldDisplay, IFieldDisplay<CategoryNameDisplayHandler>
{
    public string Missing { get; set; } = "Unknown";

    public override string TemplateName => "CategoryName";
}

public sealed class CategoryNameDisplayHandler(CategoryNameDisplay display, CategoryStore store)
    : FieldDisplayHandler<CategoryNameDisplay>(display)
{
    public override Task<object?> PrepareAsync(
        FieldDisplayContext context,
        CancellationToken cancellationToken
    ) =>
        Task.FromResult<object?>(
            store.Categories.FirstOrDefault(category => Equals(category.Id, context.Value))?.Name
                ?? Display.Missing
        );
}
